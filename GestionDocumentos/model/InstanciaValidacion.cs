using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionDocumentos.model;

[Table("INSTANCIA_VALIDACION")]
public class InstanciaValidacion
{
    [Key]
    [Column("validacionId")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid ValidacionId { get; set; }

    [Required]
    [Column("userId")]
    public Guid UserId { get; set; }

    [Required]
    [Column("ordenPaso")]
    public int OrdenPaso { get; set; }

    [Required]
    [MaxLength(10)]
    [Column("accion")]
    public string Accion { get; set; } = string.Empty; 

    [Required]
    [Column("fechaRevision")]
    public DateTime FechaRevision { get; set; } = DateTime.Now;

    [Required]
    [Column("documentoId")]
    public Guid DocumentoId { get; set; }

    // Navegación
    [ForeignKey("DocumentoId")]
    public virtual Documento? Documento { get; set; }
}
