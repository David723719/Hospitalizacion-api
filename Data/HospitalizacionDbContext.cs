using HospitalizacionAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace HospitalizacionAPI.Data;

public class HospitalizacionDbContext : DbContext
{
    public HospitalizacionDbContext(DbContextOptions<HospitalizacionDbContext> options) : base(options) {}
    public DbSet<Cama> Camas => Set<Cama>();
    public DbSet<Paciente> Pacientes => Set<Paciente>();
    public DbSet<Admision> Admisiones => Set<Admision>();
    public DbSet<Tratamiento> Tratamientos => Set<Tratamiento>();
}
