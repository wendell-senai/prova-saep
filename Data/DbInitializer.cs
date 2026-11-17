using sistema.Models;
using sistema.Services;
using Microsoft.EntityFrameworkCore;
namespace sistema.Data;
public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var crypto = scope.ServiceProvider.GetRequiredService<CriptografiaService>();
        await db.Database.MigrateAsync();
        if (!await db.Usuarios.AnyAsync())
        {
            db.Usuarios.AddRange(new Usuario { Nome = "Administrador", Email = "admin@clinica.com", SenhaHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"), Perfil = "Administrador" }, new Usuario { Nome = "Ana Recepção", Email = "ana@clinica.com", SenhaHash = BCrypt.Net.BCrypt.HashPassword("Ana@123"), Perfil = "Usuario" }, new Usuario { Nome = "Carlos Recepção", Email = "carlos@clinica.com", SenhaHash = BCrypt.Net.BCrypt.HashPassword("Carlos@123"), Perfil = "Usuario" }
                         );
            await db.SaveChangesAsync();
        }
        if (!await db.Tutores.AnyAsync())
        {
            db.Tutores.AddRange(new Tutor { Nome = "João Silva", CpfCriptografado = crypto.Criptografar("11111111111"), Telefone = "54999990001", Email = "joao@email.com" }, new Tutor { Nome = "Maria Souza", CpfCriptografado = crypto.Criptografar("22222222222"), Telefone = "54999990002", Email = "maria@email.com" }, new Tutor { Nome = "Pedro Lima", CpfCriptografado = crypto.Criptografar("33333333333"), Telefone = "54999990003", Email = "pedro@email.com" }
                         );
            await db.SaveChangesAsync();
        }
        if (!await db.Pets.AnyAsync())
        {
            var tutores = await db.Tutores.OrderBy(t => t.Id).Take(3).ToListAsync();
            db.Pets.AddRange(new Pet { Nome = "Rex", Especie = "Cachorro", Raca = "Labrador", TutorId = tutores[0].Id }, new Pet { Nome = "Mia", Especie = "Gato", Raca = "Siamês", TutorId = tutores[1].Id }, new Pet { Nome = "Thor", Especie = "Cachorro", Raca = "SRD", TutorId = tutores[2].Id }
                          );
            await db.SaveChangesAsync();
        }
        if (!await db.Agendamentos.AnyAsync())
        {
            var pets = await db.Pets.Include(p => p.Tutor).OrderBy(p => p.Id).Take(3).ToListAsync();
            db.Agendamentos.AddRange(new Agendamento { DataHora = DateTime.Today.AddDays(1).AddHours(9), Motivo = "Vacinação", Status = "Agendado", TutorId = pets[0].TutorId, PetId = pets[0].Id }, new Agendamento
            {
                DataHora = DateTime.Today.AddDays(2).AddHours(14),
                Motivo = "Consulta",
                Status = "Agendado",
                TutorId = pets[1].TutorId,
                PetId = pets[1].Id
            }, new Agendamento { DataHora = DateTime.Today.AddDays(3).AddHours(10), Motivo = "Retorno", Status = "Agendado", TutorId = pets[2].TutorId, PetId = pets[2].Id }
                         );
            await db.SaveChangesAsync();
        }
    }
}