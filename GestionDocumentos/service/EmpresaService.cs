using Microsoft.EntityFrameworkCore;
using GestionDocumentos.data;
using GestionDocumentos.model;

namespace GestionDocumentos.service;

public class EmpresaService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<EmpresaService> _logger;

    public EmpresaService(ApplicationDbContext context, ILogger<EmpresaService> logger)
    {
        _context = context;
        _logger = logger;
    }

    // CREATE
    public async Task<Empresa> CrearEmpresaAsync(Empresa empresa)
    {
        _context.Empresas.Add(empresa);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Empresa creada: {EmpresaId}", empresa.EmpresaId);

        return empresa;
    }

    // READ - Obtener todas
    public async Task<List<Empresa>> ObtenerTodasEmpresasAsync()
    {
        return await _context.Empresas
            .Include(e => e.Documentos)
            .ToListAsync();
    }

    // READ - Obtener por ID
    public async Task<Empresa?> ObtenerEmpresaPorIdAsync(Guid empresaId)
    {
        return await _context.Empresas
            .Include(e => e.Documentos)
            .FirstOrDefaultAsync(e => e.EmpresaId == empresaId);
    }

    // READ - Obtener por NIT
    public async Task<Empresa?> ObtenerEmpresaPorNITAsync(int nit)
    {
        return await _context.Empresas
            .FirstOrDefaultAsync(e => e.NIT == nit);
    }

    // UPDATE
    public async Task<Empresa> ActualizarEmpresaAsync(Guid empresaId, Empresa empresaActualizada)
    {
        var empresa = await _context.Empresas.FindAsync(empresaId);
        if (empresa == null)
        {
            throw new KeyNotFoundException($"Empresa con ID {empresaId} no encontrada");
        }

        empresa.NIT = empresaActualizada.NIT;
        empresa.RazonSocial = empresaActualizada.RazonSocial;
        empresa.Ubicacion = empresaActualizada.Ubicacion;
        empresa.NumeroEmpleados = empresaActualizada.NumeroEmpleados;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Empresa actualizada: {EmpresaId}", empresaId);

        return empresa;
    }

    // DELETE
    public async Task<bool> EliminarEmpresaAsync(Guid empresaId)
    {
        var empresa = await _context.Empresas.FindAsync(empresaId);
        if (empresa == null)
        {
            return false;
        }

        _context.Empresas.Remove(empresa);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Empresa eliminada: {EmpresaId}", empresaId);

        return true;
    }

    // Consulta: Empresas con más de X empleados
    public async Task<List<Empresa>> ObtenerEmpresasPorNumeroEmpleadosAsync(int minimoEmpleados)
    {
        return await _context.Empresas
            .Where(e => e.NumeroEmpleados >= minimoEmpleados)
            .OrderByDescending(e => e.NumeroEmpleados)
            .ToListAsync();
    }
}
