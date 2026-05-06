using Microsoft.EntityFrameworkCore;
using HospitalizacionAPI.Models;

namespace HospitalizacionAPI.Data;

public class HospitalizacionDbContext : DbContext
{
    public HospitalizacionDbContext(DbContextOptions<HospitalizacionDbContext> options) : base(options) { }

    public DbSet<Cama> Camas => Set<Cama>();
    public DbSet<Paciente> Pacientes => Set<Paciente>();
    public DbSet<Admision> Admisiones => Set<Admision>();
    public DbSet<Tratamiento> Tratamientos => Set<Tratamiento>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Claves primarias
        modelBuilder.Entity<Cama>().HasKey(c => c.Codigo);
        modelBuilder.Entity<Paciente>().HasKey(p => p.Codigo);
        modelBuilder.Entity<Admision>().HasKey(a => a.Codigo);
        modelBuilder.Entity<Tratamiento>().HasKey(t => t.Codigo);

        // Relaciones (sin cascada para evitar errores)
        modelBuilder.Entity<Admision>()
            .HasOne<Paciente>().WithMany()
            .HasForeignKey(a => a.PacienteCodigo)
            .HasPrincipalKey(p => p.Codigo)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Admision>()
            .HasOne<Cama>().WithMany()
            .HasForeignKey(a => a.CamaCodigo)
            .HasPrincipalKey(c => c.Codigo)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Tratamiento>()
            .HasOne<Admision>().WithMany()
            .HasForeignKey(t => t.AdmisionCodigo)
            .HasPrincipalKey(a => a.Codigo)
            .OnDelete(DeleteBehavior.Restrict);
    }
}