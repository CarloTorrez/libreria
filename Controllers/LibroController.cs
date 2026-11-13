using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Npgsql;
using PrototipoCRUD.Data;
using PrototipoCRUD.Models;

namespace PrototipoCRUD.Controllers
{
    public class LibroController : Controller
    {
        private readonly LibroDAO _dao = new LibroDAO();
        private readonly ProveedorDAO _proveedorDao = new ProveedorDAO();

        // READ: lista con búsqueda opcional
        public IActionResult Index(string? busqueda)
        {
            ViewData["Busqueda"] = busqueda;
            CargarProveedores(); // para el desplegable del diálogo "Nuevo libro"
            var libros = _dao.ObtenerTodos(busqueda);
            return View(libros);
        }

        // CREATE: mostrar formulario
        public IActionResult Create()
        {
            CargarProveedores();
            return View(new Libro());
        }

        // CREATE: recibir datos y guardar
        [HttpPost]
        public IActionResult Create(Libro libro)
        {
            if (!ModelState.IsValid)
            {
                CargarProveedores();
                return View(libro);
            }
            try
            {
                _dao.Insertar(libro);
            }
            catch (PostgresException ex) when (ex.SqlState == "23505") // ISBN duplicado
            {
                ModelState.AddModelError("Isbn", "Ya existe un libro con ese ISBN.");
                CargarProveedores();
                return View(libro);
            }
            TempData["Ok"] = "Libro registrado correctamente.";
            return RedirectToAction("Index");
        }


        // DIÁLOGO "NUEVO LIBRO" (modal): recibe los datos por AJAX y responde en JSON.
        // Los mensajes están redactados de forma amable y dicen cómo corregir el dato.
        [HttpPost]
        public IActionResult CrearRapido(string? titulo, string? autor, string? isbn,
                                         string? precio, int? stock, int? proveedorId)
        {
            var errores = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(titulo))
                errores["titulo"] = "Escriba el título del libro para poder guardarlo.";

            if (string.IsNullOrWhiteSpace(autor))
                errores["autor"] = "Indique el nombre del autor. Si no lo conoce, escriba \"Anónimo\".";

            if (string.IsNullOrWhiteSpace(isbn))
                errores["isbn"] = "Escriba el código ISBN que está en la contraportada del libro.";

            decimal precioValor = 0;
            if (string.IsNullOrWhiteSpace(precio))
            {
                errores["precio"] = "Indique el precio de venta. Por ejemplo: 45,50.";
            }
            else if (!decimal.TryParse(precio.Trim().Replace(',', '.'), NumberStyles.Number,
                                       CultureInfo.InvariantCulture, out precioValor) || precioValor < 0)
            {
                errores["precio"] = "El precio debe ser un número igual o mayor a 0. Por ejemplo: 45,50.";
            }

            if (stock == null || stock < 0)
                errores["stock"] = "Escriba cuántas unidades hay (0 o más).";

            if (proveedorId == null || proveedorId <= 0)
                errores["proveedor"] = "Elija el proveedor al que le compra este libro.";

            if (errores.Count > 0)
                return Json(new { ok = false, errores });

            var libro = new Libro
            {
                Titulo = titulo!.Trim(),
                Autor = autor!.Trim(),
                Isbn = isbn!.Trim(),
                Precio = precioValor,
                Stock = stock!.Value,
                ProveedorId = proveedorId!.Value
            };

            try
            {
                _dao.Insertar(libro);
            }
            catch (PostgresException ex) when (ex.SqlState == "23505") // ISBN repetido
            {
                return Json(new
                {
                    ok = false,
                    errores = new Dictionary<string, string>
                    {
                        ["isbn"] = "Ya tiene un libro registrado con este ISBN. Revise el código o edite el libro existente."
                    }
                });
            }

            TempData["Ok"] = "Libro registrado correctamente.";
            return Json(new { ok = true });
        }

        // UPDATE: mostrar formulario con los datos actuales
        public IActionResult Edit(int id)
        {
            var libro = _dao.ObtenerPorId(id);
            if (libro == null) return NotFound();
            CargarProveedores();
            return View(libro);
        }

        // UPDATE: recibir datos y guardar
        [HttpPost]
        public IActionResult Edit(Libro libro)
        {
            if (!ModelState.IsValid)
            {
                CargarProveedores();
                return View(libro);
            }
            try
            {
                _dao.Actualizar(libro);
            }
            catch (PostgresException ex) when (ex.SqlState == "23505")
            {
                ModelState.AddModelError("Isbn", "Ya existe otro libro con ese ISBN.");
                CargarProveedores();
                return View(libro);
            }
            TempData["Ok"] = "Libro actualizado correctamente.";
            return RedirectToAction("Index");
        }

        // DELETE: pantalla de confirmación
        public IActionResult Delete(int id)
        {
            var libro = _dao.ObtenerPorId(id);
            if (libro == null) return NotFound();
            return View(libro);
        }

        // DELETE: ejecutar la eliminación
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            _dao.Eliminar(id);
            TempData["Ok"] = "Libro eliminado correctamente.";
            return RedirectToAction("Index");
        }

        private void CargarProveedores()
        {
            ViewBag.Proveedores = new SelectList(_proveedorDao.ObtenerTodos(), "Id", "Nombre");
        }
    }
}
