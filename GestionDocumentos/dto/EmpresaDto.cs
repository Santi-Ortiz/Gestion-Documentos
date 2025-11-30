using System.ComponentModel.DataAnnotations;

namespace GestionDocumentos.dto;

public class CrearEmpresaDto
{
    [Required]
    public int NIT { get; set; }

    [Required]
    [MaxLength(100)]
    public string RazonSocial { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Ubicacion { get; set; } = string.Empty;

    [Required]
    public int NumeroEmpleados { get; set; }
}

public class ActualizarEmpresaDto
{
    [Required]
    public int NIT { get; set; }

    [Required]
    [MaxLength(100)]
    public string RazonSocial { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Ubicacion { get; set; } = string.Empty;

    [Required]
    public int NumeroEmpleados { get; set; }
}
