using sistema.Data;
using sistema.Models;
using sistema.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
namespace sistema.Controllers;
[Authorize]
public class AgendamentosController : Controller
{
    private readonly AppDbContext _context;
    public AgendamentosController(AppDbContext context) => _context = context;
    public async Task<IActionResult> Index()
    {
        var dados = await _context.Agendamentos.AsNoTracking()
            .Include(a => a.Tutor).Include(a => a.Pet)
            .OrderBy(a => a.DataHora).ToListAsync();
        return View(dados);
    }
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var vm = new AgendamentoFormViewModel { DataHora = DateTime.Now.AddHours(1) };
        await CarregarListas(vm); return View(vm);
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AgendamentoFormViewModel vm)
    {
        await ValidarRelacao(vm);
        if (!ModelState.IsValid) { await CarregarListas(vm); return View(vm); }
        _context.Add(new Agendamento
        {
            DataHora = vm.DataHora,
            Motivo = vm.Motivo,
            Status = vm.Status,
            TutorId = vm.
         TutorId,
            PetId = vm.PetId
        });
        await _context.SaveChangesAsync(); return RedirectToAction(nameof(Index));
    }
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var a = await _context.Agendamentos.FindAsync(id); if (a is null) return NotFound();
        var vm = new AgendamentoFormViewModel
        {
            Id = a.Id,
            DataHora = a.DataHora,
            Motivo = a.Motivo,
            Status = a.Status,
            TutorId = a.TutorId,
            PetId = a.PetId
        };
        await CarregarListas(vm); return View(vm);
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AgendamentoFormViewModel vm)
    {
        if (id != vm.Id) return BadRequest(); await ValidarRelacao(vm);
        if (!ModelState.IsValid) { await CarregarListas(vm); return View(vm); }
        var a = await _context.Agendamentos.FindAsync(id); if (a is null) return NotFound();
        a.DataHora = vm.DataHora; a.Motivo = vm.Motivo; a.Status = vm.Status; a.TutorId = vm.TutorId; a.PetId = vm.PetId;
        await _context.SaveChangesAsync(); return RedirectToAction(nameof(Index));
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var a = await _context.Agendamentos.FindAsync(id); if (a is null) return NotFound();
        _context.Remove(a); await _context.SaveChangesAsync(); return RedirectToAction(nameof(Index));
    }
    private async Task ValidarRelacao(AgendamentoFormViewModel vm)
    {
        if (!new[] { "Agendado", "Concluído", "Cancelado" }.Contains(vm.Status))
            ModelState.AddModelError("Status", "Status inválido.");
        if (vm.DataHora == default)
            ModelState.AddModelError("DataHora", "Informe data e hora válidas.");
        bool tutorExiste = await _context.Tutores.AnyAsync(t => t.Id == vm.TutorId);
        bool petDoTutor = await _context.Pets.AnyAsync(p => p.Id == vm.PetId && p.TutorId == vm.TutorId);
        if (!tutorExiste) ModelState.AddModelError("TutorId", "Tutor inválido.");
        if (!petDoTutor) ModelState.AddModelError("PetId", "O pet selecionado não pertence ao tutor selecionado.");
    }
    private async Task CarregarListas(AgendamentoFormViewModel vm)
    {
        vm.Tutores = await _context.Tutores.OrderBy(t => t.Nome).Select(t => new SelectListItem(t.Nome, t.Id.
ToString())).ToListAsync();
        vm.Pets = await _context.Pets.Include(p => p.Tutor).OrderBy(p => p.Nome).Select(p => new SelectListItem(p.
Nome + " - " + p.Tutor.Nome, p.Id.ToString())).ToListAsync();
    }
}