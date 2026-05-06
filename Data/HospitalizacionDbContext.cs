using Microsoft.EntityFrameworkCore;

namespace HospitalizacionAPI.Data;

public class HospitalizacionDbContext : DbContext
{
    public HospitalizacionDbContext(DbContextOptions<HospitalizacionDbContext> options) : base(options) {}
    public DbSet<object> Camas => Set<object>();
    public DbSet<object> Pacientes => Set<object>();
    public DbSet<object> Admisiones => Set<object>();
    public DbSet<object> Tratamientos => Set<object>();
}