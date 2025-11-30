using System.ComponentModel.DataAnnotations;

namespace GestionDocumentos.dto;

public class AccionValidacionDto
{
    [Required]
    public Guid ActorUserId { get; set; }

    [Required]
    [RegularExpression("Aprobar|Rechazar|APROBAR|RECHAZAR", ErrorMessage = "La acción debe ser 'Aprobar' o 'Rechazar'")]
    [MaxLength(10)]
    public string Accion { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Razon { get; set; }
}

public class CrearDocumentoDto
{
    [Required]
    public Guid EmpresaId { get; set; }

    [Required]
    [MaxLength(500)]
    public string UrlArchivo { get; set; } = string.Empty;

    [Required]
    public string FlujoValidacionJson { get; set; } = string.Empty;
}
