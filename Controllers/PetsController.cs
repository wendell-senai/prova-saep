using sistema.Data;
using sistema.Models;
using sistema.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace sistema.Controllers;
[Authorize]
public class PetsController : Controller
{
    private readonly AppDbContext _context;
    public PetsController(AppDbContext context) => _context = context;
    public async Task<IActionResult> Index() => View(await _context.Pets.AsNoTracking()
        .Include(p => p.Tutor).OrderBy(p => p.Nome).ToListAsync());

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await CarregarTutores(); return View(new PetFormViewModel());
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PetFormViewModel vm)
    {
        await Validar(vm);
        if (!ModelState.IsValid) { await CarregarTutores(vm.TutorId); return View(vm); }
        var pet = new Pet();
        Preencher(pet, vm);
        _context.Pets.Add(pet); await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var pet = await _context.Pets.FindAsync(id);
        if (pet is null) return NotFound();
        await CarregarTutores(pet.TutorId);
        return View(new PetFormViewModel
        {
            Id = pet.Id,
            Nome = pet.Nome,
            Especie = pet.Especie,
            Raca = pet.Raca,
            DataNascimento = pet.DataNascimento,
            TutorId = pet.TutorId
        });
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, PetFormViewModel vm)
    {
        if (id != vm.Id) return BadRequest();
        var pet = await _context.Pets.FindAsync(id);
        if (pet is null) return NotFound();
        await Validar(vm);
        if (pet.TutorId != vm.TutorId && await _context.Agendamentos.AnyAsync(a => a.PetId == id))
            ModelState.AddModelError("TutorId", "Pet com agendamentos não pode trocar de tutor. Exclua os agendamentos antes de transferir.");
        if (!ModelState.IsValid) { await CarregarTutores(vm.TutorId); return View(vm); }
        Preencher(pet, vm); await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var pet = await _context.Pets.AsNoTracking().Include(p => p.Tutor).FirstOrDefaultAsync(p => p.Id == id);
        return pet is null ? NotFound() : View(pet);
    }
    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var pet = await _context.Pets.FindAsync(id);
        if (pet is null) return NotFound();
        if (await _context.Agendamentos.AnyAsync(a => a.PetId == id))
        {
            TempData["Erro"] = "Pet possui agendamentos e não pode ser excluído.";
            return RedirectToAction(nameof(Index));
        }
        _context.Remove(pet); await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
    private async Task Validar(PetFormViewModel vm)
    {
        if (!await _context.Tutores.AnyAsync(t => t.Id == vm.TutorId))
            ModelState.AddModelError("TutorId", "Tutor inválido.");
        if (vm.DataNascimento > DateTime.Today)
            ModelState.AddModelError("DataNascimento", "Nascimento não pode estar no futuro.");
    }
    private static void Preencher(Pet pet, PetFormViewModel vm)
    {
        pet.Nome = vm.Nome; pet.Especie = vm.Especie; pet.Raca = vm.Raca;
        pet.DataNascimento = vm.DataNascimento; pet.TutorId = vm.TutorId;
    }
    private async Task CarregarTutores(int? selecionado = null) =>
        ViewBag.TutorId = new SelectList(await _context.Tutores.OrderBy(t => t.Nome).ToListAsync(), "Id", "Nome", selecionado);
}