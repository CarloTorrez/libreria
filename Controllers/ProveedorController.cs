using Microsoft.AspNetCore.Mvc;
using Npgsql;
using PrototipoCRUD.Data;
using PrototipoCRUD.Models;

namespace PrototipoCRUD.Controllers
{
    public class ProveedorController : Controller
    {
        private readonly ProveedorDAO _dao = new ProveedorDAO();

        public IActionResult Index()
        {
            return View(_dao.ObtenerTodos());
        }

        public IActionResult Create()
        {
            return View(new Proveedor());
        }

        [HttpPost]
        public IActionResult Create(Proveedor proveedor)
        {
            if (!ModelState.IsValid) return View(proveedor);
            _dao.Insertar(proveedor);
            TempData["Ok"] = "Proveedor registrado correctamente.";
            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            var proveedor = _dao.ObtenerPorId(id);
            if (proveedor == null) return NotFound();
            return View(proveedor);
        }

        [HttpPost]
        public IActionResult Edit(Proveedor proveedor)
        {
            if (!ModelState.IsValid) return View(proveedor);
            _dao.Actualizar(proveedor);
            TempData["Ok"] = "Proveedor actualizado correctamente.";
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var proveedor = _dao.ObtenerPorId(id);
            if (proveedor == null) return NotFound();
            return View(proveedor);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                _dao.Eliminar(id);
                TempData["Ok"] = "Proveedor eliminado correctamente.";
            }
            catch (PostgresException ex) when (ex.SqlState == "23503") // tiene libros asociados
            {
                TempData["Error"] = "No se puede eliminar: el proveedor tiene libros registrados.";
            }
            return RedirectToAction("Index");
        }
    }
}
