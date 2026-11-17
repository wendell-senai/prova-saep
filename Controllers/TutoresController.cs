using sistema.Data;
using sistema.Models;
using sistema.Services;
using sistema.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace sistema.Controllers;
[Authorize]
public class TutoresController : Controller
{
    private readonly AppDbContext _context;
    private readonly CriptografiaService _crypto;
    public TutoresController(AppDbContext context, CriptografiaService crypto)
    { _context = context; _crypto = crypto; }
    public async Task<IActionResult> Index(string? busca)
    {
        var query = _context.Tutores.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(busca))
            query = query.Where(t => t.Nome.Contains(busca) || t.Email.Contains(busca) || t.Telefone.Contains(
busca));
        ViewBag.Busca = busca;
        return View(await query.OrderBy(t => t.Nome).ToListAsync());
    }
    [HttpGet]
    public IActionResult Create() => View(new TutorFormViewModel());
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TutorFormViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var tutor = new Tutor
        {
            Nome = vm.Nome,
            CpfCriptografado = _crypto.Criptografar(vm.Cpf),
            Telefone = vm.Telefone,
            Email = vm.Email
        };
        _context.Add(tutor); await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var t = await _context.Tutores.FindAsync(id);
        if (t is null) return NotFound();
        return View(new TutorFormViewModel
        {
            Id = t.Id,
            Nome = t.Nome,
            Cpf = _crypto.Descriptografar(t.CpfCriptografado),
            Telefone = t.Telefone,
            Email = t.Email
        });
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, TutorFormViewModel vm)
    {
        if (id != vm.Id) return BadRequest();
        if (!ModelState.IsValid) return View(vm);
        var t = await _context.Tutores.FindAsync(id);
        if (t is null) return NotFound();
        t.Nome = vm.Nome; t.CpfCriptografado = _crypto.Criptografar(vm.Cpf); t.Telefone = vm.Telefone; t.Email = vm.Email;
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var t = await _context.Tutores.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        return t is null ? NotFound() : View(t);
    }
    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var t = await _context.Tutores.FindAsync(id);
        if (t is null) return NotFound();
        bool possuiPets = await _context.Pets.AnyAsync(p => p.TutorId == id);
        bool possuiAgenda = await _context.Agendamentos.AnyAsync(a => a.TutorId == id);
        if (possuiPets || possuiAgenda)
        {
            TempData["Erro"] = "Tutor não pode ser excluído porque possui pets ou agendamentos.";
            return RedirectToAction(nameof(Index));
        }
        _context.Remove(t); await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}