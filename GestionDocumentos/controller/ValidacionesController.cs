using Microsoft.AspNetCore.Mvc;
using GestionDocumentos.model;
using GestionDocumentos.dto;
using GestionDocumentos.service;

namespace GestionDocumentos.controller;

[ApiController]
[Route("api/[controller]")]
public class ValidacionesController : ControllerBase
{
    private readonly InstanciaValidacionService _validacionService;
    private readonly ILogger<ValidacionesController> _logger;

    public ValidacionesController(InstanciaValidacionService validacionService, ILogger<ValidacionesController> logger)
    {
        _validacionService = validacionService;
        _logger = logger;
    }

    // POST /api/validaciones
    [HttpPost]
    public async Task<ActionResult<InstanciaValidacion>> CrearValidacion([FromBody] CrearInstanciaValidacionDto dto)
    {
        try
        {
            var validacion = new InstanciaValidacion
            {
                UserId = dto.UserId,
                OrdenPaso = dto.OrdenPaso,
                Accion = dto.Accion,
                DocumentoId = dto.DocumentoId,
                FechaRevision = DateTime.Now
            };

            var validacionCreada = await _validacionService.CrearValidacionAsync(validacion);
            return CreatedAtAction(nameof(ObtenerValidacion), new { id = validacionCreada.ValidacionId }, validacionCreada);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear validación");
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    // GET /api/validaciones
    [HttpGet]
    public async Task<ActionResult<List<InstanciaValidacion>>> ObtenerTodasValidaciones()
    {
        try
        {
            var validaciones = await _validacionService.ObtenerTodasValidacionesAsync();
            return Ok(validaciones);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener validaciones");
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    // GET /api/validaciones/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<InstanciaValidacion>> ObtenerValidacion(Guid id)
    {
        try
        {
            var validacion = await _validacionService.ObtenerValidacionPorIdAsync(id);
            if (validacion == null)
            {
                return NotFound(new { error = "Validación no encontrada" });
            }
            return Ok(validacion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener validación");
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    // GET /api/validaciones/documento/{documentoId}
    [HttpGet("documento/{documentoId}")]
    public async Task<ActionResult<List<InstanciaValidacion>>> ObtenerValidacionesPorDocumento(Guid documentoId)
    {
        try
        {
            var validaciones = await _validacionService.ObtenerValidacionesPorDocumentoAsync(documentoId);
            return Ok(validaciones);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener validaciones por documento");
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    // GET /api/validaciones/usuario/{userId}
    [HttpGet("usuario/{userId}")]
    public async Task<ActionResult<List<InstanciaValidacion>>> ObtenerValidacionesPorUsuario(Guid userId)
    {
        try
        {
            var validaciones = await _validacionService.ObtenerValidacionesPorUsuarioAsync(userId);
            return Ok(validaciones);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener validaciones por usuario");
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    // GET /api/validaciones/accion/{accion}
    [HttpGet("accion/{accion}")]
    public async Task<ActionResult<List<InstanciaValidacion>>> ObtenerValidacionesPorAccion(string accion)
    {
        try
        {
            var validaciones = await _validacionService.ObtenerValidacionesPorAccionAsync(accion);
            return Ok(validaciones);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener validaciones por acción");
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    // PUT /api/validaciones/{id}
    [HttpPut("{id}")]
    public async Task<ActionResult<InstanciaValidacion>> ActualizarValidacion(Guid id, [FromBody] ActualizarInstanciaValidacionDto dto)
    {
        try
        {
            var validacionActualizada = new InstanciaValidacion
            {
                Accion = dto.Accion,
                FechaRevision = dto.FechaRevision
            };

            var validacion = await _validacionService.ActualizarValidacionAsync(id, validacionActualizada);
            return Ok(validacion);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar validación");
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    // DELETE /api/validaciones/{id}
    [HttpDelete("{id}")]
    public async Task<ActionResult> EliminarValidacion(Guid id)
    {
        try
        {
            var resultado = await _validacionService.EliminarValidacionAsync(id);
            if (!resultado)
            {
                return NotFound(new { error = "Validación no encontrada" });
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar validación");
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }
}
