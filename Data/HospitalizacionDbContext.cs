using Microsoft.EntityFrameworkCore;
using HospitalizacionAPI.Models;

namespace HospitalizacionAPI.Data;

public class HospitalizacionDbContext : DbContext
{
    public HospitalizacionDbContext(DbContextOptions<HospitalizacionDbContext> options) : base(options) { }

    public DbSet<Paciente> Pacientes => Set<Paciente>();
    public DbSet<Cama> Camas => Set<Cama>();
    public DbSet<Admision> Admisiones => Set<Admision>();
    public DbSet<Tratamiento> Tratamientos => Set<Tratamiento>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Paciente>().HasIndex(p => p.Codigo).IsUnique();
        modelBuilder.Entity<Cama>().HasIndex(c => c.Codigo).IsUnique();
        modelBuilder.Entity<Admision>().HasIndex(a => a.Codigo).IsUnique();
        modelBuilder.Entity<Tratamiento>().HasIndex(t => t.Codigo).IsUnique();

        modelBuilder.Entity<Admision>()
            .HasOne(a => a.Paciente)
            .WithMany()
            .HasForeignKey(a => a.PacienteCodigo)
            .HasPrincipalKey(p => p.Codigo)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Admision>()
            .HasOne(a => a.Cama)
            .WithMany()
            .HasForeignKey(a => a.CamaCodigo)
            .HasPrincipalKey(c => c.Codigo)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Tratamiento>()
            .HasOne(t => t.Admision)
            .WithMany()
            .HasForeignKey(t => t.AdmisionCodigo)
            .HasPrincipalKey(a => a.Codigo)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Paciente>().HasQueryFilter(p => p.Estado == "Activo");
        modelBuilder.Entity<Admision>().HasQueryFilter(a => a.Estado == "Activo");
        modelBuilder.Entity<Tratamiento>().HasQueryFilter(t => t.Estado == "Activo");
    }
}