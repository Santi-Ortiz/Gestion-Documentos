using Microsoft.EntityFrameworkCore;
using GestionDocumentos.model;

namespace GestionDocumentos.data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Empresa> Empresas { get; set; }
    public DbSet<Documento> Documentos { get; set; }
    public DbSet<InstanciaValidacion> InstanciasValidacion { get; set; }
    public DbSet<DocumentoAuditoria> DocumentosAuditoria { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

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

        modelBuilder.Entity<DocumentoAuditoria>()
            .HasOne(a => a.Documento)
            .WithMany(d => d.Auditorias)
            .HasForeignKey(a => a.DocumentoId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices
        modelBuilder.Entity<Documento>()
            .HasIndex(d => d.Estado)
            .HasDatabaseName("idxDocumentoEstado");

        modelBuilder.Entity<InstanciaValidacion>()
            .HasIndex(i => i.UserId)
            .HasDatabaseName("idxUserId");

        modelBuilder.Entity<InstanciaValidacion>()
            .HasIndex(i => new { i.DocumentoId, i.OrdenPaso })
            .HasDatabaseName("idxDocumentoOrden");

        modelBuilder.Entity<DocumentoAuditoria>()
            .HasIndex(a => a.UserId)
            .HasDatabaseName("idxUserIdAuditoria");

        modelBuilder.Entity<DocumentoAuditoria>()
            .HasIndex(a => a.DocumentoId)
            .HasDatabaseName("idxDocumentoAuditoria");
    }
}
