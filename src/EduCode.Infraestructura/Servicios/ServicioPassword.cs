using System.Security.Cryptography;
using EduCode.Aplicacion.Interfaces;

namespace EduCode.Infraestructura.Servicios;

public sealed class ServicioPassword : IServicioPassword
{
    private const int Iteraciones = 210_000;
    private const int TamanioSalt = 16;
    private const int TamanioClave = 32;

    public string GenerarHash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(TamanioSalt);

        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iteraciones,
            HashAlgorithmName.SHA256,
            TamanioClave);

        return $"PBKDF2-SHA256${Iteraciones}$" +
               $"{Convert.ToBase64String(salt)}$" +
               $"{Convert.ToBase64String(hash)}";
    }

    public bool Verificar(string password, string passwordHash)
    {
        try
        {
            var partes = passwordHash.Split('$');
            if (partes.Length != 4 || partes[0] != "PBKDF2-SHA256")
                return false;

            var iteraciones = int.Parse(partes[1]);
            var salt = Convert.FromBase64String(partes[2]);
            var hashEsperado = Convert.FromBase64String(partes[3]);

            var hashActual = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                iteraciones,
                HashAlgorithmName.SHA256,
                hashEsperado.Length);

            return CryptographicOperations.FixedTimeEquals(
                hashActual,
                hashEsperado);
        }
        catch
        {
            return false;
        }
    }
}
