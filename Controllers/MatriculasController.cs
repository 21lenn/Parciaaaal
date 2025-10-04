using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parciaaaal.Data;
using Parciaaaal.Models;
using System.Security.Claims;

namespace Parciaaaal.Controllers
{
    [Authorize] // Solo usuarios autenticados pueden usar este controlador
    public class MatriculasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MatriculasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: Matriculas/Inscribirse
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Inscribirse(int cursoId)
        {
            // Obtener el usuario actual
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(usuarioId))
            {
                TempData["Error"] = "Debe estar autenticado para inscribirse.";
                return RedirectToAction("Index", "Cursos");
            }

            // Evitar que coordinadores se matriculen
            if (User.IsInRole("Coordinador"))
            {
                TempData["Error"] = "Los coordinadores no pueden matricularse.";
                return RedirectToAction("Index", "Cursos");
            }

            // Obtener el curso
            var curso = await _context.Cursos.FirstOrDefaultAsync(c => c.Id == cursoId && c.Activo);
            if (curso == null)
            {
                TempData["Error"] = "Curso no encontrado.";
                return RedirectToAction("Index", "Cursos");
            }

            // Validación de cupo
            var matriculasActuales = await _context.Matriculas
                .CountAsync(m => m.CursoId == cursoId && (m.Estado == "Pendiente" || m.Estado == "Confirmada"));

            if (matriculasActuales >= curso.CupoMaximo)
            {
                TempData["Error"] = "El curso ya alcanzó su cupo máximo.";
                return RedirectToAction("Details", "Cursos", new { id = cursoId });
            }

            // Validación de solapamiento de horarios
            var cursosUsuario = await _context.Matriculas
                .Include(m => m.Curso)
                .Where(m => m.UsuarioId == usuarioId && (m.Estado == "Pendiente" || m.Estado == "Confirmada"))
                .Select(m => m.Curso)
                .ToListAsync();

            foreach (var c in cursosUsuario)
            {
                if (c != null && curso.HorarioInicio < c.HorarioFin && curso.HorarioFin > c.HorarioInicio)
                {
                    TempData["Error"] = $"No puede inscribirse porque se solapa con el curso {c.Nombre}.";
                    return RedirectToAction("Details", "Cursos", new { id = cursoId });
                }
            }

            // Crear matrícula
            var matricula = new Matricula
            {
                CursoId = cursoId,
                UsuarioId = usuarioId,
                FechaRegistro = DateTime.Now,
                Estado = "Pendiente"
            };

            _context.Matriculas.Add(matricula);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Inscripción realizada correctamente (Pendiente).";
            return RedirectToAction("Details", "Cursos", new { id = cursoId });
        }
    }
}


