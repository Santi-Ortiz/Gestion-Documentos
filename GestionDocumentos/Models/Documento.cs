using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionDocumentos.Models;

[Table("Documento")]
public class Documento
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int EmpresaId { get; set; }

    [Required]
    [MaxLength(255)]
    public string Titulo { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? TipoDocumento { get; set; }

    [MaxLength(500)]
    public string? UrlArchivo { get; set; }

    [MaxLength(50)]
    public string Estado { get; set; } = "Pendiente"; // Pendiente, Aprobado, Rechazado

    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    public DateTime? FechaActualizacion { get; set; }

    // Navegación
    [ForeignKey("EmpresaId")]
    public virtual Empresa? Empresa { get; set; }

    public virtual ICollection<InstanciaValidacion> InstanciasValidacion { get; set; } = new List<InstanciaValidacion>();
}
