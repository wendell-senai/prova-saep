using System.ComponentModel.DataAnnotations;
namespace sistema.ViewModels;
public class TutorFormViewModel
{
    public int Id { get; set; }
    [Required, StringLength(120)]
    public string Nome { get; set; } = string.Empty;
    [Required, Display(Name = "CPF"), RegularExpression(@"\d{11}", ErrorMessage = "Informe 11 dígitos, sem pontuação.")]
    public string Cpf { get; set; } = string.Empty;
    [Required, StringLength(20)]
    public string Telefone { get; set; } = string.Empty;
    [Required, EmailAddress, StringLength(150)]
    public string Email { get; set; } = string.Empty;
}