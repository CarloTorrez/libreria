using Npgsql;
using NpgsqlTypes;
using PrototipoCRUD.Models;

namespace PrototipoCRUD.Data
{
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
                    "INSERT INTO ventas (fecha, cliente, total) VALUES (@fecha, @cliente, @total) RETURNING id";
                int idVenta;
                using (var cmdCab = new NpgsqlCommand(sqlCabecera, conexion, transaccion))
                {
                    // timestamp sin zona horaria: se indica el tipo explícitamente (evita error de Kind=Local)
                    cmdCab.Parameters.Add(new NpgsqlParameter("fecha", NpgsqlDbType.Timestamp)
                    {
                        Value = DateTime.SpecifyKind(venta.Fecha, DateTimeKind.Unspecified)
                    });
                    cmdCab.Parameters.AddWithValue("cliente", venta.Cliente);
                    cmdCab.Parameters.AddWithValue("total", venta.Total);
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
