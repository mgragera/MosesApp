
using Microsoft.EntityFrameworkCore;

namespace Moses.Models
{
    public class ApplicationDbContext: DbContext
    {
        public DbSet<Usuario> Usuarios { get; set; }

        public DbSet<Memoria> Memorias { get; set; }

        public DbSet<Proyecto> Proyectos { get; set; }

        public DbSet<Proyecto_Memoria> Proyecto_Memorias { get; set; }

        public DbSet<Lenguaje> Lenguajes { get; set; }

        public DbSet<Glosario> Glosarios { get; set; }

        public DbSet<Proyecto_Glosario> Proyecto_Glosarios { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=./traductor.db");
        }
    }
}