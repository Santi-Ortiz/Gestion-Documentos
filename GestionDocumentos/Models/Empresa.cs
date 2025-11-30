using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionDocumentos.Models;

[Table("Empresa")]
public class Empresa
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(255)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? NIT { get; set; }

    [MaxLength(500)]
    public string? Direccion { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    // Navegación
    public virtual ICollection<Documento> Documentos { get; set; } = new List<Documento>();
}
