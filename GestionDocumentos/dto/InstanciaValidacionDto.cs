using System.ComponentModel.DataAnnotations;

namespace GestionDocumentos.dto;

public class CrearInstanciaValidacionDto
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public int OrdenPaso { get; set; }

    [Required]
    [MaxLength(10)]
    public string Accion { get; set; } = string.Empty;

    [Required]
    public Guid DocumentoId { get; set; }
}

public class ActualizarInstanciaValidacionDto
{
    [Required]
    [MaxLength(10)]
    public string Accion { get; set; } = string.Empty;

    [Required]
    public DateTime FechaRevision { get; set; }
}
