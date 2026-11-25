using System.Security.Cryptography;

namespace Ec.Edu.Sigh.Gr04.Security;

public static class PasswordUtil
{
    private const int Iteraciones = 600_000;
    private const int TamanoSal = 16;
    private const int TamanoHash = 32; // 256 bits
    private const string Prefijo = "pbkdf2_sha256";

    public static string GenerarHash(string contrasena)
    {
        if (string.IsNullOrEmpty(contrasena))
            throw new ArgumentException("Contraseña vacía", nameof(contrasena));

        byte[] sal = RandomNumberGenerator.GetBytes(TamanoSal);
        byte[] hash = Derivar(contrasena, sal, Iteraciones);

        return $"{Prefijo}${Iteraciones}${Convert.ToBase64String(sal)}${Convert.ToBase64String(hash)}";
    }

    public static bool Verificar(string contrasena, string almacenado)
    {
        if (contrasena == null || almacenado == null) return false;

        string[] partes = almacenado.Split('$');
        if (partes.Length != 4 || partes[0] != Prefijo) return false;

        if (!int.TryParse(partes[1], out int iteraciones)
            || iteraciones < 100_000 || iteraciones > 2_000_000)
            return false;

        try
        {
            byte[] sal = Convert.FromBase64String(partes[2]);
            byte[] hashAlmacenado = Convert.FromBase64String(partes[3]);

            if (sal.Length != TamanoSal || hashAlmacenado.Length != TamanoHash)
                return false;

            byte[] hashCalculado = Derivar(contrasena, sal, iteraciones);
            return CryptographicOperations.FixedTimeEquals(hashCalculado, hashAlmacenado);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static byte[] Derivar(string contrasena, byte[] sal, int iteraciones) =>
        Rfc2898DeriveBytes.Pbkdf2(contrasena, sal, iteraciones, HashAlgorithmName.SHA256, TamanoHash);
}
