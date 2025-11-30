using System.ComponentModel.DataAnnotations;

namespace GestionDocumentos.DTOs;

public class AccionValidacionDto
{
    [Required]
    public int ActorUserId { get; set; }

    [Required]
    [RegularExpression("Aprobar|Rechazar", ErrorMessage = "La acción debe ser 'Aprobar' o 'Rechazar'")]
    public string Accion { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Razon { get; set; }
}

public class CrearDocumentoDto
{
    [Required]
    public int EmpresaId { get; set; }

    [Required]
    [MaxLength(255)]
    public string Titulo { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? TipoDocumento { get; set; }

    [MaxLength(500)]
    public string? UrlArchivo { get; set; }
}
