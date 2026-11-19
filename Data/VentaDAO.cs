using Npgsql;
using NpgsqlTypes;
using PrototipoCRUD.Models;

namespace PrototipoCRUD.Data
{
    // Se lanza cuando se intenta vender más unidades de las que hay
    public class StockInsuficienteException : Exception
    {
        public StockInsuficienteException(string mensaje) : base(mensaje) { }
    }

    public class VentaDAO
    {
        // Registra cabecera + detalle de forma ATÓMICA (todo o nada).
        public int RegistrarVenta(Venta venta)
        {
            using var conexion = new NpgsqlConnection(ConexionBD.Cadena);
            conexion.Open();
            using var transaccion = conexion.BeginTransaction();   // inicia la transacción

            try
            {
                // 1. Cabecera: se recupera el id generado con RETURNING id
                const string sqlCabecera =
                    @"INSERT INTO ventas
                        (fecha, cliente, vendedor, subtotal_bruto, porcentaje_descuento, monto_descuento, total_final, total)
                      VALUES
                        (@fecha, @cliente, @vendedor, @bruto, @pct, @monto, @final, @total)
                      RETURNING id";
                int idVenta;
                using (var cmdCab = new NpgsqlCommand(sqlCabecera, conexion, transaccion))
                {
                    // timestamp sin zona horaria: se indica el tipo explícitamente (evita error de Kind=Local)
                    cmdCab.Parameters.Add(new NpgsqlParameter("fecha", NpgsqlDbType.Timestamp)
                    {
                        Value = DateTime.SpecifyKind(venta.Fecha, DateTimeKind.Unspecified)
                    });
                    cmdCab.Parameters.AddWithValue("cliente", venta.Cliente);
                    cmdCab.Parameters.AddWithValue("vendedor", venta.Vendedor);                 // trazabilidad
                    cmdCab.Parameters.AddWithValue("bruto", venta.SubtotalBruto);
                    cmdCab.Parameters.AddWithValue("pct", venta.PorcentajeDescuento);           // la BD tiene CHECK <= 20
                    cmdCab.Parameters.AddWithValue("monto", venta.MontoDescuento);
                    cmdCab.Parameters.AddWithValue("final", venta.TotalFinal);
                    cmdCab.Parameters.AddWithValue("total", venta.TotalFinal);                  // columna original conservada
                    idVenta = Convert.ToInt32(cmdCab.ExecuteScalar());
                }

                // 2. Detalle: una fila por libro, vinculada al id de la cabecera
                const string sqlDetalle =
                    @"INSERT INTO detalle_ventas
                        (id_venta, id_producto, nombre_producto, cantidad, precio_unitario, subtotal)
                      VALUES (@idVenta, @idProd, @nomProd, @cant, @precio, @sub)";
                foreach (var item in venta.Detalles)
                {
                    using var cmdDet = new NpgsqlCommand(sqlDetalle, conexion, transaccion);
                    cmdDet.Parameters.AddWithValue("idVenta", idVenta);
                    cmdDet.Parameters.AddWithValue("idProd", item.IdProducto);
                    cmdDet.Parameters.AddWithValue("nomProd", item.NombreProducto);
                    cmdDet.Parameters.AddWithValue("cant", item.Cantidad);
                    cmdDet.Parameters.AddWithValue("precio", item.PrecioUnitario);
                    cmdDet.Parameters.AddWithValue("sub", item.Subtotal);
                    cmdDet.ExecuteNonQuery();

                    // Descuenta el stock; la condición "stock >= cantidad" evita quedar en negativo
                    const string sqlStock =
                        "UPDATE libros SET stock = stock - @cant WHERE id = @id AND stock >= @cant";
                    using var cmdStock = new NpgsqlCommand(sqlStock, conexion, transaccion);
                    cmdStock.Parameters.AddWithValue("cant", item.Cantidad);
                    cmdStock.Parameters.AddWithValue("id", item.IdProducto);
                    if (cmdStock.ExecuteNonQuery() == 0)
                        throw new StockInsuficienteException(
                            $"No hay unidades suficientes de \"{item.NombreProducto}\". Reduzca la cantidad e intente de nuevo.");
                }

                // 3. Todo salió bien: se confirma
                transaccion.Commit();
                return idVenta;
            }
            catch (Exception)
            {
                // Algo falló: se deshace TODO (no quedan ventas huérfanas)
                transaccion.Rollback();
                throw;
            }
        }
    }
}
