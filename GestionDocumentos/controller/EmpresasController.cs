using Microsoft.AspNetCore.Mvc;
using GestionDocumentos.model;
using GestionDocumentos.dto;
using GestionDocumentos.service;

namespace GestionDocumentos.controller;

[ApiController]
[Route("api/[controller]")]
public class EmpresasController : ControllerBase
{
    private readonly EmpresaService _empresaService;
    private readonly ILogger<EmpresasController> _logger;

    public EmpresasController(EmpresaService empresaService, ILogger<EmpresasController> logger)
    {
        _empresaService = empresaService;
        _logger = logger;
    }

    // POST /api/empresas
    [HttpPost]
    public async Task<ActionResult<Empresa>> CrearEmpresa([FromBody] CrearEmpresaDto dto)
    {
        try
        {
            var empresa = new Empresa
            {
                NIT = dto.NIT,
                RazonSocial = dto.RazonSocial,
                Ubicacion = dto.Ubicacion,
                NumeroEmpleados = dto.NumeroEmpleados
            };

            var empresaCreada = await _empresaService.CrearEmpresaAsync(empresa);
            return CreatedAtAction(nameof(ObtenerEmpresa), new { id = empresaCreada.EmpresaId }, empresaCreada);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear empresa");
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    // GET /api/empresas
    [HttpGet]
    public async Task<ActionResult<List<Empresa>>> ObtenerTodasEmpresas()
    {
        try
        {
            var empresas = await _empresaService.ObtenerTodasEmpresasAsync();
            return Ok(empresas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener empresas");
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    // GET /api/empresas/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<Empresa>> ObtenerEmpresa(Guid id)
    {
        try
        {
            var empresa = await _empresaService.ObtenerEmpresaPorIdAsync(id);
            if (empresa == null)
            {
                return NotFound(new { error = "Empresa no encontrada" });
            }
            return Ok(empresa);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener empresa");
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    // GET /api/empresas/nit/{nit}
    [HttpGet("nit/{nit}")]
    public async Task<ActionResult<Empresa>> ObtenerEmpresaPorNIT(int nit)
    {
        try
        {
            var empresa = await _empresaService.ObtenerEmpresaPorNITAsync(nit);
            if (empresa == null)
            {
                return NotFound(new { error = "Empresa no encontrada" });
            }
            return Ok(empresa);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener empresa por NIT");
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    // PUT /api/empresas/{id}
    [HttpPut("{id}")]
    public async Task<ActionResult<Empresa>> ActualizarEmpresa(Guid id, [FromBody] ActualizarEmpresaDto dto)
    {
        try
        {
            var empresaActualizada = new Empresa
            {
                NIT = dto.NIT,
                RazonSocial = dto.RazonSocial,
                Ubicacion = dto.Ubicacion,
                NumeroEmpleados = dto.NumeroEmpleados
            };

            var empresa = await _empresaService.ActualizarEmpresaAsync(id, empresaActualizada);
            return Ok(empresa);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar empresa");
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    // DELETE /api/empresas/{id}
    [HttpDelete("{id}")]
    public async Task<ActionResult> EliminarEmpresa(Guid id)
    {
        try
        {
            var resultado = await _empresaService.EliminarEmpresaAsync(id);
            if (!resultado)
            {
                return NotFound(new { error = "Empresa no encontrada" });
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar empresa");
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }

    // GET /api/empresas/empleados/{minimo}
    [HttpGet("empleados/{minimo}")]
    public async Task<ActionResult<List<Empresa>>> ObtenerEmpresasPorEmpleados(int minimo)
    {
        try
        {
            var empresas = await _empresaService.ObtenerEmpresasPorNumeroEmpleadosAsync(minimo);
            return Ok(empresas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener empresas por empleados");
            return StatusCode(500, new { error = "Error interno del servidor" });
        }
    }
}
