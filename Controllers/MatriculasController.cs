using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parciaaaal.Data;
using Parciaaaal.Models;
using System.Security.Claims;

namespace Parciaaaal.Controllers
{
    [Authorize] // Solo usuarios autenticados
    public class MatriculasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public MatriculasController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Matriculas
        public async Task<IActionResult> Index()
        {
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (User.IsInRole("Coordinador"))
            {
                // Coordinador ve todas las matrículas
                var todas = _context.Matriculas.Include(m => m.Curso);
                return View(await todas.ToListAsync());
            }
            else
            {
                // Estudiante ve solo sus matrículas
                var misMatriculas = _context.Matriculas
                    .Include(m => m.Curso)
                    .Where(m => m.UsuarioId == usuarioId);

                return View(await misMatriculas.ToListAsync());
            }
        }

        // POST: Matriculas/Create
        [HttpPost]
        [Authorize(Roles = "Estudiante")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int cursoId)
        {
            var curso = await _context.Cursos.FindAsync(cursoId);
            if (curso == null || !curso.Activo)
            {
                TempData["Error"] = "Curso no encontrado o inactivo.";
                return RedirectToAction("Index", "Cursos");
            }

            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (usuarioId == null) return Forbid();

            // Verificar si ya existe cualquier matrícula
            bool yaMatriculado = await _context.Matriculas
                .AnyAsync(m => m.CursoId == cursoId && m.UsuarioId == usuarioId);

            if (yaMatriculado)
            {
                TempData["Error"] = "Ya estás matriculado en este curso.";
                return RedirectToAction("Index", "Cursos");
            }

            // Verificar cupo
            int inscritos = await _context.Matriculas.CountAsync(m => m.CursoId == cursoId && m.Estado == "Confirmada");
            if (inscritos >= curso.CupoMaximo)
            {
                TempData["Error"] = "El curso ya alcanzó el cupo máximo.";
                return RedirectToAction("Index", "Cursos");
            }

            // Verificar solapamiento de horarios
            var misCursos = await _context.Matriculas
                .Include(m => m.Curso)
                .Where(m => m.UsuarioId == usuarioId && m.Estado == "Confirmada")
                .ToListAsync();

            foreach (var m in misCursos)
            {
                if (m.Curso == null) continue;

                if ((curso.HorarioInicio < m.Curso.HorarioFin) && (curso.HorarioFin > m.Curso.HorarioInicio))
                {
                    TempData["Error"] = "El curso se solapa con otro curso que ya matriculaste.";
                    return RedirectToAction("Index", "Cursos");
                }
            }

            // Crear matrícula
            var matricula = new Matricula
            {
                CursoId = cursoId,
                UsuarioId = usuarioId,
                Estado = "Confirmada",
                FechaRegistro = DateTime.Now
            };

            _context.Matriculas.Add(matricula);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = $"Te has matriculado correctamente en {curso.Nombre}.";
            return RedirectToAction("Index", "Cursos");
        }

        // GET: Matriculas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var matricula = await _context.Matriculas
                .Include(m => m.Curso)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (matricula == null) return NotFound();

            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!User.IsInRole("Coordinador") && matricula.UsuarioId != usuarioId)
                return Forbid();

            return View(matricula);
        }

        // GET: Matriculas/Delete/5
        [Authorize(Roles = "Estudiante")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var matricula = await _context.Matriculas
                .Include(m => m.Curso)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (matricula == null) return NotFound();

            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (matricula.UsuarioId != usuarioId) return Forbid();

            return View(matricula);
        }

        // POST: Matriculas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Estudiante")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var matricula = await _context.Matriculas.FindAsync(id);
            if (matricula != null)
            {
                var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (matricula.UsuarioId != usuarioId) return Forbid();

                _context.Matriculas.Remove(matricula);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "Matrícula cancelada correctamente.";
            }
            return RedirectToAction(nameof(Index));
        }

        // ACCIONES DE COORDINADOR: Confirmar o Cancelar matrícula
        [Authorize(Roles = "Coordinador")]
        public async Task<IActionResult> Confirmar(int id)
        {
            var matricula = await _context.Matriculas.FindAsync(id);
            if (matricula == null) return NotFound();

            matricula.Estado = "Confirmada";
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Matrícula confirmada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Coordinador")]
        public async Task<IActionResult> Cancelar(int id)
        {
            var matricula = await _context.Matriculas.FindAsync(id);
            if (matricula == null) return NotFound();

            matricula.Estado = "Cancelada";
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Matrícula cancelada correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}










