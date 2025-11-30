using System.ComponentModel.DataAnnotations;

namespace GestionDocumentos.dto;

public class CrearDocumentoAuditoriaDto
{
    [Required]
    [MaxLength(1)]
    public string EstadoAnterior { get; set; } = string.Empty;

    [Required]
    [MaxLength(1)]
    public string EstadoNuevo { get; set; } = string.Empty;

    public Guid? UserId { get; set; }

    [Required]
    public Guid DocumentoId { get; set; }
}

public class ActualizarDocumentoAuditoriaDto
{
    [Required]
    [MaxLength(1)]
    public string EstadoAnterior { get; set; } = string.Empty;

    [Required]
    [MaxLength(1)]
    public string EstadoNuevo { get; set; } = string.Empty;

    [Required]
    public DateTime FechaCambio { get; set; }

    public Guid? UserId { get; set; }
}
