using Microsoft.AspNetCore.DataProtection;
namespace sistema.Services;
public class CriptografiaService
{
    private readonly IDataProtector _protetor;
    public CriptografiaService(IDataProtectionProvider provider)
    {
        _protetor = provider.CreateProtector("CpfTutor.v1");
    }
    public string Criptografar(string texto) => _protetor.Protect(texto);
    public string Descriptografar(string valor) => _protetor.Unprotect(valor);
}