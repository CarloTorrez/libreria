using Npgsql;
using PrototipoCRUD.Models;

namespace PrototipoCRUD.Data
{
    public class LibroDAO
    {
        private const string SelectBase =
            "SELECT l.id, l.titulo, l.autor, l.isbn, l.precio, l.stock, l.proveedor_id, p.nombre " +
            "FROM libros l INNER JOIN proveedores p ON p.id = l.proveedor_id ";

        // CREATE
        public void Insertar(Libro l)
        {
            using (var conexion = new NpgsqlConnection(ConexionBD.Cadena))
            {
                conexion.Open();
                string sql = "INSERT INTO libros (titulo, autor, isbn, precio, stock, proveedor_id) " +
                             "VALUES (@titulo, @autor, @isbn, @precio, @stock, @proveedor_id)";
                using (var comando = new NpgsqlCommand(sql, conexion))
                {
                    comando.Parameters.AddWithValue("titulo", l.Titulo);
                    comando.Parameters.AddWithValue("autor", l.Autor);
                    comando.Parameters.AddWithValue("isbn", l.Isbn);
                    comando.Parameters.AddWithValue("precio", l.Precio);
                    comando.Parameters.AddWithValue("stock", l.Stock);
                    comando.Parameters.AddWithValue("proveedor_id", l.ProveedorId);
                    comando.ExecuteNonQuery();
                }
            }
        }

        // READ: lista (opcionalmente filtrada por título o autor)
        public List<Libro> ObtenerTodos(string? busqueda = null)
        {
            var lista = new List<Libro>();
            using (var conexion = new NpgsqlConnection(ConexionBD.Cadena))
            {
                conexion.Open();
                string sql = SelectBase;
                bool filtrar = !string.IsNullOrWhiteSpace(busqueda);
                if (filtrar)
                {
                    sql += "WHERE l.titulo ILIKE @busqueda OR l.autor ILIKE @busqueda ";
                }
                sql += "ORDER BY l.titulo";

                using (var comando = new NpgsqlCommand(sql, conexion))
                {
                    if (filtrar)
                    {
                        comando.Parameters.AddWithValue("busqueda", "%" + busqueda!.Trim() + "%");
                    }
                    using (var reader = comando.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(LeerLibro(reader));
                        }
                    }
                }
            }
            return lista;
        }

        // READ: uno por id
        public Libro? ObtenerPorId(int id)
        {
            using (var conexion = new NpgsqlConnection(ConexionBD.Cadena))
            {
                conexion.Open();
                using (var comando = new NpgsqlCommand(SelectBase + "WHERE l.id = @id", conexion))
                {
                    comando.Parameters.AddWithValue("id", id);
                    using (var reader = comando.ExecuteReader())
                    {
                        return reader.Read() ? LeerLibro(reader) : null;
                    }
                }
            }
        }

        // UPDATE
        public void Actualizar(Libro l)
        {
            using (var conexion = new NpgsqlConnection(ConexionBD.Cadena))
            {
                conexion.Open();
                string sql = "UPDATE libros SET titulo = @titulo, autor = @autor, isbn = @isbn, " +
                             "precio = @precio, stock = @stock, proveedor_id = @proveedor_id " +
                             "WHERE id = @id";
                using (var comando = new NpgsqlCommand(sql, conexion))
                {
                    comando.Parameters.AddWithValue("titulo", l.Titulo);
                    comando.Parameters.AddWithValue("autor", l.Autor);
                    comando.Parameters.AddWithValue("isbn", l.Isbn);
                    comando.Parameters.AddWithValue("precio", l.Precio);
                    comando.Parameters.AddWithValue("stock", l.Stock);
                    comando.Parameters.AddWithValue("proveedor_id", l.ProveedorId);
                    comando.Parameters.AddWithValue("id", l.Id);
                    comando.ExecuteNonQuery();
                }
            }
        }

        // DELETE
        public void Eliminar(int id)
        {
            using (var conexion = new NpgsqlConnection(ConexionBD.Cadena))
            {
                conexion.Open();
                using (var comando = new NpgsqlCommand("DELETE FROM libros WHERE id = @id", conexion))
                {
                    comando.Parameters.AddWithValue("id", id);
                    comando.ExecuteNonQuery();
                }
            }
        }

        private static Libro LeerLibro(NpgsqlDataReader reader)
        {
            return new Libro
            {
                Id = reader.GetInt32(0),
                Titulo = reader.GetString(1),
                Autor = reader.GetString(2),
                Isbn = reader.GetString(3),
                Precio = reader.GetDecimal(4),
                Stock = reader.GetInt32(5),
                ProveedorId = reader.GetInt32(6),
                ProveedorNombre = reader.GetString(7)
            };
        }
    }
}
