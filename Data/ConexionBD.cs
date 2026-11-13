namespace PrototipoCRUD.Data
{
    // Centraliza la cadena de conexión para no repetirla en cada DAO.
    // Cambiar por las credenciales de su PostgreSQL local.
    public static class ConexionBD
    {
        public static string Cadena =>
            Environment.GetEnvironmentVariable("LIBRERIA_DB")
            ?? "Host=localhost;Port=5432;Database=libreria;Username=postgres;Password=postgres";
    }
}
