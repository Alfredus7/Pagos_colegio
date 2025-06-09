using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Core_Models.Models;

namespace Core_Models.Data
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
