using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionDocumentos.Models;

[Table("InstanciaValidacion")]
public class InstanciaValidacion
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int DocumentoId { get; set; }

    [Required]
    public int ActorUserId { get; set; }

    [Required]
    [MaxLength(50)]
    public string Accion { get; set; } = string.Empty; // Aprobar, Rechazar

    [MaxLength(500)]
    public string? Razon { get; set; }

    public DateTime FechaAccion { get; set; } = DateTime.Now;

    // Navegación
    [ForeignKey("DocumentoId")]
    public virtual Documento? Documento { get; set; }
}
