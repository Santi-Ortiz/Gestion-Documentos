using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionDocumentos.model;

[Table("DOCUMENTO")]
public class Documento
{
    [Key]
    [Column("documentoId")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid DocumentoId { get; set; }

    [Required]
    [MaxLength(500)]
    [Column("urlArchivo")]
    public string UrlArchivo { get; set; } = string.Empty;

    [Required]
    [Column("estado", TypeName = "varchar(1)")]
    [MaxLength(1)]
    public string Estado { get; set; } = "P"; // P = Pendiente, A = Aprobado, R = Rechazado

    [Required]
    [Column("fechaCreacion")]
    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    [Required]
    [Column("flujoValidacionJson", TypeName = "nvarchar(MAX)")]
    public string FlujoValidacionJson { get; set; } = string.Empty;

    [Required]
    [Column("empresaId")]
    public Guid EmpresaId { get; set; }

    // Navegación
    [ForeignKey("EmpresaId")]
    public virtual Empresa? Empresa { get; set; }

    public virtual ICollection<InstanciaValidacion> InstanciasValidacion { get; set; } = new List<InstanciaValidacion>();
    public virtual ICollection<DocumentoAuditoria> Auditorias { get; set; } = new List<DocumentoAuditoria>();
}
