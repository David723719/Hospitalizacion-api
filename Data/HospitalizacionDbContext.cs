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
        modelBuilder.Entity<Cama>().HasKey(c => c.Codigo);
        modelBuilder.Entity<Paciente>().HasKey(p => p.Codigo);
        modelBuilder.Entity<Admision>().HasKey(a => a.Codigo);
        modelBuilder.Entity<Tratamiento>().HasKey(t => t.Codigo);
    }
}