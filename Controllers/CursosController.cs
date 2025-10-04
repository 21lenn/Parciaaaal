using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parciaaaal.Data;
using Parciaaaal.Models;

namespace Parciaaaal.Controllers
{
    public class CursosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CursosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Cursos
        public async Task<IActionResult> Index(string? buscar, int? minCreditos, int? maxCreditos, TimeSpan? horarioInicio, TimeSpan? horarioFin)
        {
            var cursos = from c in _context.Cursos
                         where c.Activo
                         select c;

            if (!string.IsNullOrEmpty(buscar))
                cursos = cursos.Where(c => c.Nombre.Contains(buscar) || c.Codigo.Contains(buscar));

            if (minCreditos.HasValue)
                cursos = cursos.Where(c => c.Creditos >= minCreditos.Value);

            if (maxCreditos.HasValue)
                cursos = cursos.Where(c => c.Creditos <= maxCreditos.Value);

            if (horarioInicio.HasValue && horarioFin.HasValue)
                cursos = cursos.Where(c => c.HorarioInicio >= horarioInicio.Value && c.HorarioFin <= horarioFin.Value);

            return View(await cursos.ToListAsync());
        }

        // GET: Cursos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var curso = await _context.Cursos.FirstOrDefaultAsync(c => c.Id == id);
            if (curso == null) return NotFound();

            return View(curso);
        }

        // POST: Cursos/Inscribirse/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Inscribirse(int id)
        {
            var curso = await _context.Cursos.FirstOrDefaultAsync(c => c.Id == id && c.Activo);
            if (curso == null) return NotFound();

            // Validaciones server-side
            if (curso.Creditos < 0)
            {
                ModelState.AddModelError("", "Los créditos no pueden ser negativos.");
                return View("Details", curso);
            }

            if (curso.HorarioFin < curso.HorarioInicio)
            {
                ModelState.AddModelError("", "El horario de fin no puede ser anterior al horario de inicio.");
                return View("Details", curso);
            }

            // Simular inscripción
            TempData["Mensaje"] = $"Te has inscrito al curso {curso.Nombre} correctamente.";

            return RedirectToAction(nameof(Details), new { id = curso.Id });
        }

        // GET: Cursos/Create
        public IActionResult Create() => View();

        // POST: Cursos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Curso curso)
        {
            if (ModelState.IsValid)
            {
                _context.Add(curso);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(curso);
        }

        // GET: Cursos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var curso = await _context.Cursos.FindAsync(id);
            if (curso == null) return NotFound();

            return View(curso);
        }

        // POST: Cursos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Curso curso)
        {
            if (id != curso.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(curso);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Cursos.Any(e => e.Id == curso.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(curso);
        }

        // GET: Cursos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var curso = await _context.Cursos.FirstOrDefaultAsync(c => c.Id == id);
            if (curso == null) return NotFound();

            return View(curso);
        }

        // POST: Cursos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var curso = await _context.Cursos.FindAsync(id);
            if (curso != null)
            {
                _context.Cursos.Remove(curso);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}


