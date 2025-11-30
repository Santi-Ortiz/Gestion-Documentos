using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionDocumentos.model;

[Table("EMPRESA")]
public class Empresa
{
    [Key]
    [Column("empresaId")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid EmpresaId { get; set; }

    [Required]
    [Column("NIT")]
    public int NIT { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("razonSocial")]
    public string RazonSocial { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    [Column("ubicacion")]
    public string Ubicacion { get; set; } = string.Empty;

    [Required]
    [Column("numeroEmpleados")]
    public int NumeroEmpleados { get; set; }

    // Navegación
    public virtual ICollection<Documento> Documentos { get; set; } = new List<Documento>();
}
