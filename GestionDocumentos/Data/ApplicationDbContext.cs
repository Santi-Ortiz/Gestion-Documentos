using Microsoft.EntityFrameworkCore;
using GestionDocumentos.Models;

namespace GestionDocumentos.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Empresa> Empresas { get; set; }
    public DbSet<Documento> Documentos { get; set; }
    public DbSet<InstanciaValidacion> InstanciasValidacion { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuraciones adicionales si necesitas
        modelBuilder.Entity<Documento>()
            .HasOne(d => d.Empresa)
            .WithMany(e => e.Documentos)
            .HasForeignKey(d => d.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<InstanciaValidacion>()
            .HasOne(i => i.Documento)
            .WithMany(d => d.InstanciasValidacion)
            .HasForeignKey(i => i.DocumentoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
