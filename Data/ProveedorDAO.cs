using Npgsql;
using PrototipoCRUD.Models;

namespace PrototipoCRUD.Data
{
    public class ProveedorDAO
    {
        // CREATE: devuelve el id generado por PostgreSQL
        public int Insertar(Proveedor p)
        {
            using (var conexion = new NpgsqlConnection(ConexionBD.Cadena))
            {
                conexion.Open();
                string sql = "INSERT INTO proveedores (nombre, telefono, correo) " +
                             "VALUES (@nombre, @telefono, @correo) RETURNING id";
                using (var comando = new NpgsqlCommand(sql, conexion))
                {
                    comando.Parameters.AddWithValue("nombre", p.Nombre);
                    comando.Parameters.AddWithValue("telefono", (object?)p.Telefono ?? DBNull.Value);
                    comando.Parameters.AddWithValue("correo", (object?)p.Correo ?? DBNull.Value);
                    return Convert.ToInt32(comando.ExecuteScalar());
                }
            }
        }

        // READ: lista completa
        public List<Proveedor> ObtenerTodos()
        {
            var lista = new List<Proveedor>();
            using (var conexion = new NpgsqlConnection(ConexionBD.Cadena))
            {
                conexion.Open();
                string sql = "SELECT id, nombre, telefono, correo FROM proveedores ORDER BY nombre";
                using (var comando = new NpgsqlCommand(sql, conexion))
                using (var reader = comando.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(LeerProveedor(reader));
                    }
                }
            }
            return lista;
        }

        // READ: uno por id
        public Proveedor? ObtenerPorId(int id)
        {
            using (var conexion = new NpgsqlConnection(ConexionBD.Cadena))
            {
                conexion.Open();
                string sql = "SELECT id, nombre, telefono, correo FROM proveedores WHERE id = @id";
                using (var comando = new NpgsqlCommand(sql, conexion))
                {
                    comando.Parameters.AddWithValue("id", id);
                    using (var reader = comando.ExecuteReader())
                    {
                        return reader.Read() ? LeerProveedor(reader) : null;
                    }
                }
            }
        }

        // UPDATE
        public void Actualizar(Proveedor p)
        {
            using (var conexion = new NpgsqlConnection(ConexionBD.Cadena))
            {
                conexion.Open();
                string sql = "UPDATE proveedores SET nombre = @nombre, telefono = @telefono, " +
                             "correo = @correo WHERE id = @id";
                using (var comando = new NpgsqlCommand(sql, conexion))
                {
                    comando.Parameters.AddWithValue("nombre", p.Nombre);
                    comando.Parameters.AddWithValue("telefono", (object?)p.Telefono ?? DBNull.Value);
                    comando.Parameters.AddWithValue("correo", (object?)p.Correo ?? DBNull.Value);
                    comando.Parameters.AddWithValue("id", p.Id);
                    comando.ExecuteNonQuery();
                }
            }
        }

        // DELETE (falla con PostgresException 23503 si tiene libros asociados)
        public void Eliminar(int id)
        {
            using (var conexion = new NpgsqlConnection(ConexionBD.Cadena))
            {
                conexion.Open();
                using (var comando = new NpgsqlCommand("DELETE FROM proveedores WHERE id = @id", conexion))
                {
                    comando.Parameters.AddWithValue("id", id);
                    comando.ExecuteNonQuery();
                }
            }
        }

        private static Proveedor LeerProveedor(NpgsqlDataReader reader)
        {
            return new Proveedor
            {
                Id = reader.GetInt32(0),
                Nombre = reader.GetString(1),
                Telefono = reader.IsDBNull(2) ? null : reader.GetString(2),
                Correo = reader.IsDBNull(3) ? null : reader.GetString(3)
            };
        }
    }
}
