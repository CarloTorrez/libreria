namespace PrototipoCRUD.Models
{
    public class Venta
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Now;          // valor por defecto: ahora
        public string Cliente { get; set; } = "Consumidor Final";    // valor por defecto
        public List<DetalleVenta> Detalles { get; set; } = new();
        // Total calculado automáticamente con LINQ
        public decimal Total => Detalles.Sum(d => d.Subtotal);
    }
}
