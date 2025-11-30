namespace GestionDocumentos.dto;

public class HistorialValidacionDto
{
    public Guid ValidacionId { get; set; }
    public Guid UserId { get; set; }
    public int OrdenPaso { get; set; }
    public string Accion { get; set; } = string.Empty;
    public DateTime FechaRevision { get; set; }
}
