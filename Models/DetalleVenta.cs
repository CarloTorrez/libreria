namespace PrototipoCRUD.Models
{
    public class DetalleVenta
    {
        public int IdProducto { get; set; }                       // id del libro
        public string NombreProducto { get; set; } = string.Empty; // título del libro
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        // Propiedad calculada automáticamente
        public decimal Subtotal => Cantidad * PrecioUnitario;
    }
}
