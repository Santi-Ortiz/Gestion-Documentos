using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using GestionDocumentos.data;
using GestionDocumentos.model;
using GestionDocumentos.dto;

namespace GestionDocumentos.service;

public class InstanciaValidacionService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<InstanciaValidacionService> _logger;

    public InstanciaValidacionService(ApplicationDbContext context, ILogger<InstanciaValidacionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    // CREATE
    public async Task<InstanciaValidacion> CrearValidacionAsync(InstanciaValidacion validacion)
    {
        _context.InstanciasValidacion.Add(validacion);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Instancia de validación creada: {ValidacionId}", validacion.ValidacionId);

        return validacion;
    }

    // READ - Obtener todas
    public async Task<List<InstanciaValidacion>> ObtenerTodasValidacionesAsync()
    {
        return await _context.InstanciasValidacion
            .Include(v => v.Documento)
            .ToListAsync();
    }

    // READ - Obtener por ID
    public async Task<InstanciaValidacion?> ObtenerValidacionPorIdAsync(Guid validacionId)
    {
        return await _context.InstanciasValidacion
            .Include(v => v.Documento)
            .FirstOrDefaultAsync(v => v.ValidacionId == validacionId);
    }

    // READ - Obtener por documento
    public async Task<List<InstanciaValidacion>> ObtenerValidacionesPorDocumentoAsync(Guid documentoId)
    {
        return await _context.InstanciasValidacion
            .Where(v => v.DocumentoId == documentoId)
            .OrderBy(v => v.OrdenPaso)
            .ToListAsync();
    }

    // READ - Obtener por usuario
    public async Task<List<InstanciaValidacion>> ObtenerValidacionesPorUsuarioAsync(Guid userId)
    {
        return await _context.InstanciasValidacion
            .Include(v => v.Documento)
            .Where(v => v.UserId == userId)
            .OrderByDescending(v => v.FechaRevision)
            .ToListAsync();
    }

    // UPDATE
    public async Task<InstanciaValidacion> ActualizarValidacionAsync(Guid validacionId, InstanciaValidacion validacionActualizada)
    {
        var validacion = await _context.InstanciasValidacion.FindAsync(validacionId);
        if (validacion == null)
        {
            throw new KeyNotFoundException($"Validación con ID {validacionId} no encontrada");
        }

        validacion.Accion = validacionActualizada.Accion;
        validacion.FechaRevision = validacionActualizada.FechaRevision;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Validación actualizada: {ValidacionId}", validacionId);

        return validacion;
    }

    // DELETE
    public async Task<bool> EliminarValidacionAsync(Guid validacionId)
    {
        var validacion = await _context.InstanciasValidacion.FindAsync(validacionId);
        if (validacion == null)
        {
            return false;
        }

        _context.InstanciasValidacion.Remove(validacion);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Validación eliminada: {ValidacionId}", validacionId);

        return true;
    }

    // SP - Obtener historial de validaciones de un documento
    public async Task<List<HistorialValidacionDto>> ObtenerHistorialValidacionesDocumentoAsync(Guid documentoId)
    {
        var historial = new List<HistorialValidacionDto>();

        using (var command = _context.Database.GetDbConnection().CreateCommand())
        {
            command.CommandText = "EXEC sp_HistorialValidacionesDocumentos @documentoId";
            command.Parameters.Add(new SqlParameter("@documentoId", documentoId));

            await _context.Database.OpenConnectionAsync();

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    historial.Add(new HistorialValidacionDto
                    {
                        ValidacionId = reader.GetGuid(reader.GetOrdinal("validacionId")),
                        UserId = reader.GetGuid(reader.GetOrdinal("userId")),
                        OrdenPaso = reader.GetInt32(reader.GetOrdinal("ordenPaso")),
                        Accion = reader.GetString(reader.GetOrdinal("accion")),
                        FechaRevision = reader.GetDateTime(reader.GetOrdinal("fechaRevision"))
                    });
                }
            }

            await _context.Database.CloseConnectionAsync();
        }

        _logger.LogInformation("Historial de validaciones obtenido para DocumentoId={DocumentoId}", documentoId);

        return historial;
    }

}
