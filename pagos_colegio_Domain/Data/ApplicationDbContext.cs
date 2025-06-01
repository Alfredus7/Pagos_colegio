using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using pagos_colegio_Core.Models;

namespace pagos_colegio_Core.Data
{
    public class AppDbContext : DbContext
    {
      

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configura relaciones y nombres si es necesario
        }

        public DbSet<Familia> Familias { get; set; }
        public DbSet<Cuentas> Cuentas { get; set; }
        public DbSet<Estudiante> Estudiantes { get; set; }
        public DbSet<Tarifa> Tarifas { get; set; }
        public DbSet<Pago> Pagos { get; set; }
        public DbSet<Recibo> Recibos { get; set; }
    }

}
