using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionDocumentos.model;

[Table("DOCUMENTO_AUDITORIA")]
public class DocumentoAuditoria
{
    [Key]
    [Column("auditoriaId")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid AuditoriaId { get; set; }

    [Required]
    [Column("estadoAnterior", TypeName = "varchar(1)")]
    [MaxLength(1)]
    public string EstadoAnterior { get; set; } = string.Empty;

    [Required]
    [Column("estadoNuevo", TypeName = "varchar(1)")]
    [MaxLength(1)]
    public string EstadoNuevo { get; set; } = string.Empty;

    [Required]
    [Column("fechaCambio")]
    public DateTime FechaCambio { get; set; } = DateTime.Now;

    [Column("userId")]
    public Guid? UserId { get; set; }

    [Required]
    [Column("documentoId")]
    public Guid DocumentoId { get; set; }

    [ForeignKey("DocumentoId")]
    public virtual Documento? Documento { get; set; }
}
