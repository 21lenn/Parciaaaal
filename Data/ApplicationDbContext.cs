using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Parciaaaal.Models; 

namespace Parciaaaal.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        
        public DbSet<Curso> Cursos { get; set; }
        public DbSet<Matricula> Matriculas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Curso: código único
            modelBuilder.Entity<Curso>()
                .HasIndex(c => c.Codigo)
                .IsUnique();

            // Matrícula: un usuario no puede matricularse dos veces en el mismo curso
            modelBuilder.Entity<Matricula>()
                .HasIndex(m => new { m.CursoId, m.UsuarioId })
                .IsUnique();

            // Restricciones tipo CHECK 
            modelBuilder.Entity<Curso>()
                .ToTable(t =>
                {
                    t.HasCheckConstraint("CK_Curso_Creditos", "Creditos > 0");
                    t.HasCheckConstraint("CK_Curso_Horarios", "HorarioInicio < HorarioFin");
                });
        }
    }
}

