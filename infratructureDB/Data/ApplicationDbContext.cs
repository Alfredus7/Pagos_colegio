using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Pagos_colegio.Models;

namespace Pagos_colegio.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        
        public DbSet<Familia> Familias { get; set; }
        public DbSet<Estudiante> Estudiantes { get; set; }
        public DbSet<Tarifa> Tarifas { get; set; }
        public DbSet<Pago> Pagos { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

    }
}
