using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using GestionDocumentos.data;
using GestionDocumentos.model;
using GestionDocumentos.dto;

namespace GestionDocumentos.service;

public class DocumentoAuditoriaService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DocumentoAuditoriaService> _logger;

    public DocumentoAuditoriaService(ApplicationDbContext context, ILogger<DocumentoAuditoriaService> logger)
    {
        _context = context;
        _logger = logger;
    }

    // CREATE
    public async Task<DocumentoAuditoria> CrearAuditoriaAsync(DocumentoAuditoria auditoria)
    {
        _context.DocumentosAuditoria.Add(auditoria);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Auditoría creada: {AuditoriaId}", auditoria.AuditoriaId);

        return auditoria;
    }

    // READ - Obtener todas
    public async Task<List<DocumentoAuditoria>> ObtenerTodasAuditoriasAsync()
    {
        return await _context.DocumentosAuditoria
            .Include(a => a.Documento)
            .OrderByDescending(a => a.FechaCambio)
            .ToListAsync();
    }

    // READ - Obtener por ID
    public async Task<DocumentoAuditoria?> ObtenerAuditoriaPorIdAsync(Guid auditoriaId)
    {
        return await _context.DocumentosAuditoria
            .Include(a => a.Documento)
            .FirstOrDefaultAsync(a => a.AuditoriaId == auditoriaId);
    }

    // READ - Obtener por documento
    public async Task<List<DocumentoAuditoria>> ObtenerAuditoriasPorDocumentoAsync(Guid documentoId)
    {
        return await _context.DocumentosAuditoria
            .Where(a => a.DocumentoId == documentoId)
            .OrderByDescending(a => a.FechaCambio)
            .ToListAsync();
    }

    // READ - Obtener por usuario
    public async Task<List<DocumentoAuditoria>> ObtenerAuditoriasPorUsuarioAsync(Guid userId)
    {
        return await _context.DocumentosAuditoria
            .Include(a => a.Documento)
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.FechaCambio)
            .ToListAsync();
    }

    // UPDATE
    public async Task<DocumentoAuditoria> ActualizarAuditoriaAsync(Guid auditoriaId, DocumentoAuditoria auditoriaActualizada)
    {
        var auditoria = await _context.DocumentosAuditoria.FindAsync(auditoriaId);
        if (auditoria == null)
        {
            throw new KeyNotFoundException($"Auditoría con ID {auditoriaId} no encontrada");
        }

        auditoria.EstadoAnterior = auditoriaActualizada.EstadoAnterior;
        auditoria.EstadoNuevo = auditoriaActualizada.EstadoNuevo;
        auditoria.FechaCambio = auditoriaActualizada.FechaCambio;
        auditoria.UserId = auditoriaActualizada.UserId;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Auditoría actualizada: {AuditoriaId}", auditoriaId);

        return auditoria;
    }

    // DELETE
    public async Task<bool> EliminarAuditoriaAsync(Guid auditoriaId)
    {
        var auditoria = await _context.DocumentosAuditoria.FindAsync(auditoriaId);
        if (auditoria == null)
        {
            return false;
        }

        _context.DocumentosAuditoria.Remove(auditoria);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Auditoría eliminada: {AuditoriaId}", auditoriaId);

        return true;
    }

    // Consulta: Auditorías por cambio de estado
    public async Task<List<DocumentoAuditoria>> ObtenerAuditoriasPorCambioEstadoAsync(string estadoAnterior, string estadoNuevo)
    {
        return await _context.DocumentosAuditoria
            .Include(a => a.Documento)
            .Where(a => a.EstadoAnterior == estadoAnterior && a.EstadoNuevo == estadoNuevo)
            .OrderByDescending(a => a.FechaCambio)
            .ToListAsync();
    }

    // SP - Obtener historial de auditoría de un documento
    public async Task<List<HistorialAuditoriaDto>> ObtenerHistorialAuditoriaDocumentoAsync(Guid documentoId)
    {
        var historial = new List<HistorialAuditoriaDto>();

        using (var command = _context.Database.GetDbConnection().CreateCommand())
        {
            command.CommandText = "EXEC sp_HistorialDocumentoAuditoria @documentoId";
            command.Parameters.Add(new SqlParameter("@documentoId", documentoId));

            await _context.Database.OpenConnectionAsync();

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    historial.Add(new HistorialAuditoriaDto
                    {
                        AuditoriaId = reader.GetGuid(reader.GetOrdinal("auditoriaId")),
                        EstadoAnterior = reader.GetString(reader.GetOrdinal("estadoAnterior")),
                        EstadoNuevo = reader.GetString(reader.GetOrdinal("estadoNuevo")),
                        FechaCambio = reader.GetDateTime(reader.GetOrdinal("fechaCambio")),
                        UserId = reader.IsDBNull(reader.GetOrdinal("userId")) ? null : reader.GetGuid(reader.GetOrdinal("userId"))
                    });
                }
            }

            await _context.Database.CloseConnectionAsync();
        }

        _logger.LogInformation("Historial de auditoría obtenido para DocumentoId={DocumentoId}", documentoId);

        return historial;
    }

}
