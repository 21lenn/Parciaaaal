using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; 

namespace Parciaaaal.Models
{
    public class Curso
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string Codigo { get; set; } = string.Empty;

        [Required]
        public string Nombre { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Los créditos deben ser mayores a 0.")]
        public int Creditos { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "El cupo máximo debe ser mayor a 0.")]
        public int CupoMaximo { get; set; }

        [Required]
        public TimeSpan HorarioInicio { get; set; }

        [Required]
        public TimeSpan HorarioFin { get; set; }

        public bool Activo { get; set; } = true;
    }
}

