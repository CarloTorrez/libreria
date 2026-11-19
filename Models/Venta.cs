namespace PrototipoCRUD.Models
{
    public class Venta
    {
        // Regla de negocio CR-01: descuento máximo permitido (en %)
        public const decimal DescuentoMaximo = 20m;

        public int Id { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Now;          // valor por defecto: ahora
        public string Cliente { get; set; } = "Consumidor Final";    // valor por defecto
        public string Vendedor { get; set; } = string.Empty;         // NUEVO: trazabilidad
        public List<DetalleVenta> Detalles { get; set; } = new();

        // NUEVO: solo se puede cambiar con AplicarDescuento (así la regla no se puede saltar)
        public decimal PorcentajeDescuento { get; private set; }

        // Cálculos automáticos (LINQ)
        public decimal SubtotalBruto => Detalles.Sum(d => d.Subtotal);
        public decimal MontoDescuento =>
            Math.Round(SubtotalBruto * (PorcentajeDescuento / 100m), 2, MidpointRounding.AwayFromZero);
        public decimal TotalFinal => SubtotalBruto - MontoDescuento;

        // Se conserva por compatibilidad con el código de la Actividad 7 (ahora igual al total final)
        public decimal Total => TotalFinal;

        // NUEVO: valida y aplica el descuento (0% a 20%)
        public void AplicarDescuento(decimal porcentaje)
        {
            if (porcentaje < 0 || porcentaje > DescuentoMaximo)
            {
                throw new ArgumentOutOfRangeException(nameof(porcentaje),
                    "El descuento permitido debe estar entre 0% y un máximo de 20%.");
            }
            PorcentajeDescuento = porcentaje;
        }
    }
}
