using Microsoft.AspNetCore.Mvc;
using GestionDocumentos.model;
using GestionDocumentos.dto;
using GestionDocumentos.service;

namespace GestionDocumentos.controller;

[ApiController]
[Route("api/[controller]")]
public class AuditoriasController : ControllerBase
{
    private readonly DocumentoAuditoriaService _auditoriaService;
    private readonly ILogger<AuditoriasController> _logger;

    public AuditoriasController(DocumentoAuditoriaService auditoriaService, ILogger<AuditoriasController> logger)
    {
        _auditoriaService = auditoriaService;
        _logger = logger;
    }

    // POST /api/auditorias
    [HttpPost]
    public async Task<ActionResult<DocumentoAuditoria>> CrearAuditoria([FromBody] CrearDocumentoAuditoriaDto dto)
    {
        try
        {
            var auditoria = new DocumentoAuditoria
            {
                EstadoAnterior = dto.EstadoAnterior,
                EstadoNuevo = dto.EstadoNuevo,
                UserId = dto.UserId,
                DocumentoId = dto.DocumentoId,
                FechaCambio = DateTime.Now
            };

            var auditoriaCreada = await _auditoriaService.CrearAuditoriaAsync(auditoria);
            return CreatedAtAction(nameof(ObtenerAuditoria), new { id = auditoriaCreada.AuditoriaId }, auditoriaCreada);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear auditoría");
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    // GET /api/auditorias
    [HttpGet]
    public async Task<ActionResult<List<DocumentoAuditoria>>> ObtenerTodasAuditorias()
    {
        try
        {
            var auditorias = await _auditoriaService.ObtenerTodasAuditoriasAsync();
            return Ok(auditorias);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener auditorías");
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    // GET /api/auditorias/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<DocumentoAuditoria>> ObtenerAuditoria(Guid id)
    {
        try
        {
            var auditoria = await _auditoriaService.ObtenerAuditoriaPorIdAsync(id);
            if (auditoria == null)
            {
                return NotFound(new { error = "Auditoría no encontrada" });
            }
            return Ok(auditoria);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener auditoría");
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    // GET /api/auditorias/documento/{documentoId}
    [HttpGet("documento/{documentoId}")]
    public async Task<ActionResult<List<DocumentoAuditoria>>> ObtenerAuditoriasPorDocumento(Guid documentoId)
    {
        try
        {
            var auditorias = await _auditoriaService.ObtenerAuditoriasPorDocumentoAsync(documentoId);
            return Ok(auditorias);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener auditorías por documento");
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    // GET /api/auditorias/usuario/{userId}
    [HttpGet("usuario/{userId}")]
    public async Task<ActionResult<List<DocumentoAuditoria>>> ObtenerAuditoriasPorUsuario(Guid userId)
    {
        try
        {
            var auditorias = await _auditoriaService.ObtenerAuditoriasPorUsuarioAsync(userId);
            return Ok(auditorias);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener auditorías por usuario");
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    // GET /api/auditorias/historial/{documentoId}
    [HttpGet("historial/{documentoId}")]
    public async Task<ActionResult<List<DocumentoAuditoria>>> ObtenerHistorialDocumento(Guid documentoId)
    {
        try
        {
            var historial = await _auditoriaService.ObtenerHistorialDocumentoAsync(documentoId);
            return Ok(historial);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener historial del documento");
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    // PUT /api/auditorias/{id}
    [HttpPut("{id}")]
    public async Task<ActionResult<DocumentoAuditoria>> ActualizarAuditoria(Guid id, [FromBody] ActualizarDocumentoAuditoriaDto dto)
    {
        try
        {
            var auditoriaActualizada = new DocumentoAuditoria
            {
                EstadoAnterior = dto.EstadoAnterior,
                EstadoNuevo = dto.EstadoNuevo,
                FechaCambio = dto.FechaCambio,
                UserId = dto.UserId
            };

            var auditoria = await _auditoriaService.ActualizarAuditoriaAsync(id, auditoriaActualizada);
            return Ok(auditoria);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar auditoría");
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    // DELETE /api/auditorias/{id}
    [HttpDelete("{id}")]
    public async Task<ActionResult> EliminarAuditoria(Guid id)
    {
        try
        {
            var resultado = await _auditoriaService.EliminarAuditoriaAsync(id);
            if (!resultado)
            {
                return NotFound(new { error = "Auditoría no encontrada" });
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar auditoría");
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }
}
