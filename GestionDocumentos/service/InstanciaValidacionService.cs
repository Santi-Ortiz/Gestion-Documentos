using Microsoft.EntityFrameworkCore;
using GestionDocumentos.data;
using GestionDocumentos.model;

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

    // Consulta: Validaciones por acción
    public async Task<List<InstanciaValidacion>> ObtenerValidacionesPorAccionAsync(string accion)
    {
        return await _context.InstanciasValidacion
            .Include(v => v.Documento)
            .Where(v => v.Accion == accion)
            .OrderByDescending(v => v.FechaRevision)
            .ToListAsync();
    }

    // Consulta: Validaciones en rango de fechas
    public async Task<List<InstanciaValidacion>> ObtenerValidacionesPorRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        return await _context.InstanciasValidacion
            .Include(v => v.Documento)
            .Where(v => v.FechaRevision >= fechaInicio && v.FechaRevision <= fechaFin)
            .OrderByDescending(v => v.FechaRevision)
            .ToListAsync();
    }
}
