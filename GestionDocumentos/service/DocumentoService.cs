using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using GestionDocumentos.data;
using GestionDocumentos.model;
using GestionDocumentos.dto;

namespace GestionDocumentos.service;

public class DocumentoService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DocumentoService> _logger;

    public DocumentoService(ApplicationDbContext context, ILogger<DocumentoService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> EmpresaExisteAsync(Guid empresaId)
    {
        return await _context.Empresas.AnyAsync(e => e.EmpresaId == empresaId);
    }

    public async Task<Documento> CrearDocumentoAsync(CrearDocumentoDto dto)
    {
        var documento = new Documento
        {
            EmpresaId = dto.EmpresaId,
            UrlArchivo = dto.UrlArchivo,
            Estado = "P", // P = Pendiente
            FlujoValidacionJson = dto.FlujoValidacionJson,
            FechaCreacion = DateTime.Now
        };

        _context.Documentos.Add(documento);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Documento creado: {DocumentoId}", documento.DocumentoId);

        return documento;
    }

    public async Task<Documento> ProcesarAccionValidacionAsync(Guid documentId, AccionValidacionDto dto)
    {
        var documento = await _context.Documentos.FindAsync(documentId);
        if (documento == null)
        {
            throw new KeyNotFoundException($"Documento con ID {documentId} no encontrado");
        }

        try
        {
            // Llamar al Stored Procedure
            var parametros = new[]
            {
                new SqlParameter("@documentoId", documentId),
                new SqlParameter("@actorUserId", dto.ActorUserId),
                new SqlParameter("@accion", dto.Accion),
                new SqlParameter("@razon", (object?)dto.Razon ?? DBNull.Value)
            };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC sp_ProcesarAccionValidacion @documentoId, @actorUserId, @accion, @razon",
                parametros
            );

            // Refrescar el documento para obtener el estado actualizado
            await _context.Entry(documento).ReloadAsync();

            _logger.LogInformation(
                "Acción de validación procesada: DocumentoId={DocumentoId}, ActorUserId={ActorUserId}, Accion={Accion}",
                documentId, dto.ActorUserId, dto.Accion
            );

            return documento;
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Error al ejecutar stored procedure sp_ProcesarAccionValidacion");
            throw new InvalidOperationException("Error al procesar la acción de validación", ex);
        }
    }

    public async Task<Documento?> ObtenerDocumentoPorIdAsync(Guid documentId)
    {
        return await _context.Documentos
            .Include(d => d.Empresa)
            .Include(d => d.InstanciasValidacion)
            .FirstOrDefaultAsync(d => d.DocumentoId == documentId);
    }

    public async Task<Documento?> ObtenerDocumentoConDetallesAsync(Guid documentId)
    {
        return await _context.Documentos
            .Include(d => d.Empresa)
            .Include(d => d.InstanciasValidacion)
            .Include(d => d.Auditorias)
            .FirstOrDefaultAsync(d => d.DocumentoId == documentId);
    }
}
