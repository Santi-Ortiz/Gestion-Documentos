using Microsoft.AspNetCore.Mvc;
using GestionDocumentos.model;
using GestionDocumentos.dto;
using GestionDocumentos.service;

namespace GestionDocumentos.controller;

[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly DocumentoService _documentoService;
    private readonly ILogger<DocumentsController> _logger;

    public DocumentsController(DocumentoService documentoService, ILogger<DocumentsController> logger)
    {
        _documentoService = documentoService;
        _logger = logger;
    }

    // POST /api/documents
    [HttpPost]
    public async Task<ActionResult<Documento>> CrearDocumento([FromBody] CrearDocumentoDto dto)
    {
        try
        {
            // Verificar que la empresa existe
            var empresaExiste = await _documentoService.EmpresaExisteAsync(dto.EmpresaId);
            if (!empresaExiste)
            {
                return BadRequest(new { error = "La empresa especificada no existe" });
            }

            var documento = await _documentoService.CrearDocumentoAsync(dto);

            return CreatedAtAction(nameof(ObtenerDocumento), new { documentId = documento.DocumentoId }, documento);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear documento");
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    // POST /api/documents/actions/{documentId}
    [HttpPost("/actions/{documentId}")]
    public async Task<ActionResult> ProcesarAccionValidacion(Guid documentId, [FromBody] AccionValidacionDto dto)
    {
        try
        {
            var documento = await _documentoService.ProcesarAccionValidacionAsync(documentId, dto);

            return Ok(new
            {
                message = "Acción procesada exitosamente",
                documentId = documento.DocumentoId,
                nuevoEstado = documento.Estado,
                fechaCreacion = documento.FechaCreacion
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Error al procesar acción de validación");
            return StatusCode(500, new { error = "Error al procesar la acción", detalle = ex.InnerException?.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al procesar acción de validación");
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    // GET /api/documents/{documentId}/download
    [HttpGet("{documentId}/download")]
    public async Task<ActionResult> ObtenerUrlDescarga(Guid documentId)
    {
        try
        {
            var documento = await _documentoService.ObtenerDocumentoConDetallesAsync(documentId);

            if (documento == null)
            {
                return NotFound(new { error = "Documento no encontrado" });
            }

            return Ok(new
            {
                documentId = documento.DocumentoId,
                estado = documento.Estado,
                urlDescarga = documento.UrlArchivo,
                fechaCreacion = documento.FechaCreacion,
                flujoValidacionJson = documento.FlujoValidacionJson,
                empresa = new
                {
                    id = documento.Empresa?.EmpresaId,
                    razonSocial = documento.Empresa?.RazonSocial,
                    nit = documento.Empresa?.NIT
                },
                validaciones = documento.InstanciasValidacion.Select(v => new
                {
                    id = v.ValidacionId,
                    userId = v.UserId,
                    ordenPaso = v.OrdenPaso,
                    accion = v.Accion,
                    fechaRevision = v.FechaRevision
                }).ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener documento");
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    // GET /api/documents/{documentId}
    [HttpGet("{documentId}")]
    public async Task<ActionResult<Documento>> ObtenerDocumento(Guid documentId)
    {
        var documento = await _documentoService.ObtenerDocumentoPorIdAsync(documentId);

        if (documento == null)
        {
            return NotFound();
        }

        return Ok(documento);
    }
}
