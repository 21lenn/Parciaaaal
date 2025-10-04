using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parciaaaal.Data;
using Parciaaaal.Models;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Parciaaaal.Controllers
{
    public class CursosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IDistributedCache _cache;

        public CursosController(ApplicationDbContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // GET: Cursos
        public async Task<IActionResult> Index(string? buscar, int? minCreditos, int? maxCreditos, TimeSpan? horarioInicio, TimeSpan? horarioFin)
        {
            List<Curso> cursos;

            // Intentar obtener cursos desde cache
            var cachedCursos = await _cache.GetStringAsync("CursosActivos");
            if (cachedCursos != null)
            {
                cursos = JsonSerializer.Deserialize<List<Curso>>(cachedCursos)!;
            }
            else
            {
                cursos = await _context.Cursos.Where(c => c.Activo).ToListAsync();

                // Guardar en cache
                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                };
                await _cache.SetStringAsync("CursosActivos", JsonSerializer.Serialize(cursos), cacheOptions);
            }

            // Aplicar filtros
            if (!string.IsNullOrEmpty(buscar))
                cursos = cursos.Where(c => c.Nombre.Contains(buscar) || c.Codigo.Contains(buscar)).ToList();
            if (minCreditos.HasValue)
                cursos = cursos.Where(c => c.Creditos >= minCreditos.Value).ToList();
            if (maxCreditos.HasValue)
                cursos = cursos.Where(c => c.Creditos <= maxCreditos.Value).ToList();
            if (horarioInicio.HasValue && horarioFin.HasValue)
                cursos = cursos.Where(c => c.HorarioInicio >= horarioInicio.Value && c.HorarioFin <= horarioFin.Value).ToList();

            return View(cursos);
        }

        // GET: Cursos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var curso = await _context.Cursos.FirstOrDefaultAsync(c => c.Id == id);
            if (curso == null) return NotFound();

            // Guardar último curso visitado en sesión
            HttpContext.Session.SetString("UltimoCursoId", curso.Id.ToString());
            HttpContext.Session.SetString("UltimoCursoNombre", curso.Nombre);

            return View(curso);
        }

        // POST: Cursos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Curso curso)
        {
            if (ModelState.IsValid)
            {
                _context.Add(curso);
                await _context.SaveChangesAsync();

                // INVALIDAR CACHE
                await _cache.RemoveAsync("CursosActivos");

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

                    // INVALIDAR CACHE
                    await _cache.RemoveAsync("CursosActivos");
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

                // INVALIDAR CACHE
                await _cache.RemoveAsync("CursosActivos");
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Cursos/Inscribirse/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Inscribirse(int id)
        {
            var curso = await _context.Cursos.FirstOrDefaultAsync(c => c.Id == id && c.Activo);
            if (curso == null) return NotFound();

            // Validaciones server-side (opcional)
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

            TempData["Mensaje"] = $"Te has inscrito al curso {curso.Nombre} correctamente.";

            return RedirectToAction(nameof(Details), new { id = curso.Id });
        }
    }
}




