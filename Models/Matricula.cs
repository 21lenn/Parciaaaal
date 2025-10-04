using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Parciaaaal.Models
{
    public class Matricula
    {
        public int Id { get; set; }

        [ForeignKey("Curso")]
        public int CursoId { get; set; }
        public Curso? Curso { get; set; }

        [Required]
        public string UsuarioId { get; set; } = string.Empty;

        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        [Required]
        public string Estado { get; set; } = "Pendiente"; // Pendiente, Confirmada, Cancelada
    }
}

