using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using PrototipoCRUD.Data;
using PrototipoCRUD.Models;

namespace PrototipoCRUD.Controllers
{
    public class VentaController : Controller
    {
        private readonly LibroDAO _libros = new LibroDAO();
        private readonly VentaDAO _ventas = new VentaDAO();

        // Línea que llega desde la pantalla (solo id y cantidad; el precio se toma de la BD)
        public class ItemEntrada
        {
            public int IdProducto { get; set; }
            public int Cantidad { get; set; }
        }

        // Muestra la pantalla de venta
        public IActionResult Registrar()
        {
            return View(_libros.ObtenerTodos());
        }

        // Recibe la venta completa desde la pantalla (detalle en JSON) y la guarda
        [HttpPost]
        public IActionResult Registrar(string? cliente, string? detallesJson)
        {
            List<ItemEntrada>? items = null;
            try
            {
                if (!string.IsNullOrWhiteSpace(detallesJson))
                    items = JsonSerializer.Deserialize<List<ItemEntrada>>(detallesJson,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (JsonException) { items = null; }

            // Validación: venta vacía
            if (items == null || items.Count == 0)
                return Json(new { ok = false, mensaje = "Agregue al menos un libro a la venta antes de confirmar." });

            var venta = new Venta
            {
                Fecha = DateTime.Now,
                Cliente = string.IsNullOrWhiteSpace(cliente) ? "Consumidor Final" : cliente.Trim()
            };
            if (venta.Cliente.Length > 150) venta.Cliente = venta.Cliente.Substring(0, 150);

            foreach (var item in items)
            {
                var libro = _libros.ObtenerPorId(item.IdProducto);
                if (libro == null)
                    return Json(new { ok = false, mensaje = "Uno de los libros ya no está en el catálogo. Quítelo de la lista y vuelva a intentar." });
                // Validación: cantidad inválida (0 o negativa)
                if (item.Cantidad <= 0)
                    return Json(new { ok = false, mensaje = $"La cantidad de \"{libro.Titulo}\" debe ser 1 o más. Corríjala y confirme de nuevo." });

                venta.Detalles.Add(new DetalleVenta
                {
                    IdProducto = libro.Id,
                    NombreProducto = libro.Titulo,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = libro.Precio     // precio real desde la base de datos
                });
            }

            try
            {
                int idVenta = _ventas.RegistrarVenta(venta);   // transacción Commit / Rollback
                string total = venta.Total.ToString("0.00", CultureInfo.InvariantCulture);
                return Json(new
                {
                    ok = true,
                    id = idVenta,
                    mensaje = $"¡Venta #{idVenta} registrada con éxito por un total de Bs. {total}!"
                });
            }
            catch (Exception)
            {
                return Json(new { ok = false, mensaje = "No pudimos guardar la venta y no se registró nada. Revise la conexión con la base de datos e intente de nuevo." });
            }
        }
    }
}
