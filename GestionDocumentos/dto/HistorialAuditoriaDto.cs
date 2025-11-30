namespace GestionDocumentos.dto;

public class HistorialAuditoriaDto
{
    public Guid AuditoriaId { get; set; }
    public string EstadoAnterior { get; set; } = string.Empty;
    public string EstadoNuevo { get; set; } = string.Empty;
    public DateTime FechaCambio { get; set; }
    public Guid? UserId { get; set; }
}
