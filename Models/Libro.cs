using System.ComponentModel.DataAnnotations;

namespace PrototipoCRUD.Models
{
    public class Libro
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El título es obligatorio")]
        [StringLength(150)]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El autor es obligatorio")]
        [StringLength(100)]
        public string Autor { get; set; } = string.Empty;

        [Required(ErrorMessage = "El ISBN es obligatorio")]
        [StringLength(20)]
        public string Isbn { get; set; } = string.Empty;

        [Range(0, 100000, ErrorMessage = "El precio debe ser mayor o igual a 0")]
        public decimal Precio { get; set; }

        [Range(0, 100000, ErrorMessage = "El stock debe ser mayor o igual a 0")]
        public int Stock { get; set; }

        [Display(Name = "Proveedor")]
        [Range(1, int.MaxValue, ErrorMessage = "Seleccione un proveedor")]
        public int ProveedorId { get; set; }

        // Solo para mostrar en la lista (viene del JOIN con proveedores)
        public string? ProveedorNombre { get; set; }
    }
}
