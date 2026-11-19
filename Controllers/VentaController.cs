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
        public IActionResult Registrar(string? cliente, string? vendedor, string? descuento, string? detallesJson)
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

            // Validación (CR-01): el vendedor es obligatorio para poder auditar
            if (string.IsNullOrWhiteSpace(vendedor))
                return Json(new { ok = false, mensaje = "Escriba el nombre del vendedor para poder registrar la venta." });

            // Validación (CR-01): el descuento debe ser un número
            decimal porcentaje = 0;
            if (!string.IsNullOrWhiteSpace(descuento) &&
                !decimal.TryParse(descuento.Trim().Replace(',', '.'), NumberStyles.Number,
                                  CultureInfo.InvariantCulture, out porcentaje))
                return Json(new { ok = false, mensaje = "El descuento debe ser un número entre 0 y 20. Por ejemplo: 10." });

            var venta = new Venta
            {
                Vendedor = vendedor.Trim().Length > 100 ? vendedor.Trim().Substring(0, 100) : vendedor.Trim(),
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

                // Validación: no vender más de lo disponible
                if (libro.Stock <= 0)
                    return Json(new { ok = false, mensaje = $"\"{libro.Titulo}\" no tiene unidades disponibles. Quítelo de la lista para continuar." });
                if (item.Cantidad > libro.Stock)
                    return Json(new { ok = false, mensaje = $"Solo hay {libro.Stock} unidad(es) disponible(s) de \"{libro.Titulo}\". Reduzca la cantidad para continuar." });

                venta.Detalles.Add(new DetalleVenta
                {
                    IdProducto = libro.Id,
                    NombreProducto = libro.Titulo,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = libro.Precio     // precio real desde la base de datos
                });
            }

            // Regla de negocio CR-01 (se valida en la entidad Venta, no solo en la pantalla)
            try
            {
                venta.AplicarDescuento(porcentaje);
            }
            catch (ArgumentOutOfRangeException)
            {
                return Json(new { ok = false, mensaje = "El descuento máximo permitido es 20%. Ingrese un valor entre 0 y 20 e intente de nuevo." });
            }

            try
            {
                int idVenta = _ventas.RegistrarVenta(venta);   // transacción Commit / Rollback
                string total = venta.TotalFinal.ToString("0.00", CultureInfo.InvariantCulture);
                string detalleDescuento = venta.PorcentajeDescuento > 0
                    ? $" (descuento de {venta.PorcentajeDescuento.ToString("0.##", CultureInfo.InvariantCulture)}% = Bs. {venta.MontoDescuento.ToString("0.00", CultureInfo.InvariantCulture)}, vendedor: {venta.Vendedor})"
                    : $" (vendedor: {venta.Vendedor})";
                return Json(new
                {
                    ok = true,
                    id = idVenta,
                    mensaje = $"¡Venta #{idVenta} registrada con éxito por un total de Bs. {total}!{detalleDescuento}"
                });
            }
            catch (StockInsuficienteException ex)
            {
                return Json(new { ok = false, mensaje = ex.Message });
            }
            catch (Exception)
            {
                return Json(new { ok = false, mensaje = "No pudimos guardar la venta y no se registró nada. Revise la conexión con la base de datos e intente de nuevo." });
            }
        }
    }
}
