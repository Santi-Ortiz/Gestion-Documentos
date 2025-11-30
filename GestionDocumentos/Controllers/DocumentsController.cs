using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionDocumentos.Data;
using GestionDocumentos.Models;
using GestionDocumentos.DTOs;
using Microsoft.Data.SqlClient;

namespace GestionDocumentos.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DocumentsController> _logger;

    public DocumentsController(ApplicationDbContext context, ILogger<DocumentsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // POST /api/documents
    [HttpPost]
    public async Task<ActionResult<Documento>> CrearDocumento([FromBody] CrearDocumentoDto dto)
    {
        try
        {
            // Verificar que la empresa existe
            var empresaExiste = await _context.Empresas.AnyAsync(e => e.Id == dto.EmpresaId);
            if (!empresaExiste)
            {
                return BadRequest(new { error = "La empresa especificada no existe" });
            }

            // Simular URL de archivo (en producción aquí subirías al bucket)
            var urlSimulada = dto.UrlArchivo ?? $"https://storage.example.com/docs/{Guid.NewGuid()}.pdf";

            var documento = new Documento
            {
                EmpresaId = dto.EmpresaId,
                Titulo = dto.Titulo,
                TipoDocumento = dto.TipoDocumento ?? "General",
                UrlArchivo = urlSimulada,
                Estado = "Pendiente",
                FechaCreacion = DateTime.Now
            };

            _context.Documentos.Add(documento);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(ObtenerDocumento), new { documentId = documento.Id }, documento);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear documento");
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    // POST /api/documents/{documentId}/actions
    [HttpPost("{documentId}/actions")]
    public async Task<ActionResult> ProcesarAccionValidacion(int documentId, [FromBody] AccionValidacionDto dto)
    {
        try
        {
            // Verificar que el documento existe
            var documento = await _context.Documentos.FindAsync(documentId);
            if (documento == null)
            {
                return NotFound(new { error = "Documento no encontrado" });
            }

            // Llamar al Stored Procedure
            var parametros = new[]
            {
                new SqlParameter("@DocumentoId", documentId),
                new SqlParameter("@ActorUserId", dto.ActorUserId),
                new SqlParameter("@Accion", dto.Accion),
                new SqlParameter("@Razon", (object?)dto.Razon ?? DBNull.Value)
            };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC sp_ProcesarAccionValidacion @DocumentoId, @ActorUserId, @Accion, @Razon",
                parametros
            );

            // Refrescar el documento para obtener el estado actualizado
            await _context.Entry(documento).ReloadAsync();

            return Ok(new
            {
                message = "Acción procesada exitosamente",
                documentId = documento.Id,
                nuevoEstado = documento.Estado,
                fechaActualizacion = documento.FechaActualizacion
            });
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Error al ejecutar stored procedure");
            return StatusCode(500, new { error = "Error al procesar la acción", detalle = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al procesar acción de validación");
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    // GET /api/documents/{documentId}/download
    [HttpGet("{documentId}/download")]
    public async Task<ActionResult> ObtenerUrlDescarga(int documentId)
    {
        try
        {
            var documento = await _context.Documentos
                .Include(d => d.Empresa)
                .Include(d => d.InstanciasValidacion)
                .FirstOrDefaultAsync(d => d.Id == documentId);

            if (documento == null)
            {
                return NotFound(new { error = "Documento no encontrado" });
            }

            // URL de descarga simulada
            var urlDescarga = documento.UrlArchivo ?? $"https://storage.example.com/docs/download/{documentId}";

            return Ok(new
            {
                documentId = documento.Id,
                titulo = documento.Titulo,
                estado = documento.Estado,
                urlDescarga = urlDescarga,
                empresa = new
                {
                    id = documento.Empresa?.Id,
                    nombre = documento.Empresa?.Nombre
                },
                validaciones = documento.InstanciasValidacion.Select(v => new
                {
                    id = v.Id,
                    accion = v.Accion,
                    razon = v.Razon,
                    fecha = v.FechaAccion
                }).ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener documento");
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    // GET /api/documents/{documentId} - Método auxiliar
    [HttpGet("{documentId}")]
    public async Task<ActionResult<Documento>> ObtenerDocumento(int documentId)
    {
        var documento = await _context.Documentos
            .Include(d => d.Empresa)
            .FirstOrDefaultAsync(d => d.Id == documentId);

        if (documento == null)
        {
            return NotFound();
        }

        return Ok(documento);
    }
}
