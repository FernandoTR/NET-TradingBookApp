using Application.Interfaces;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Application.Services;

public class StringUtilitiesService : IStringUtilitiesService
{
    private static Random random = new Random();

    private readonly ILogService _logService;
    public StringUtilitiesService(ILogService logService)
    {
        _logService = logService;
    }

    /// <summary>
    /// Genera una cadena random de la longitud indicada.
    /// </summary>
    /// <param name="length"></param>
    /// <returns></returns>
    public string GetRandomString(int length)
    {
        const string charset = @"0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ.:;[]<>{}/\*_-()¿?'=¡!#$%&@|°~";
        return new string(Enumerable.Repeat(charset, length).Select(s => s[random.Next(s.Length)]).ToArray());
    }

    /// <summary>
    /// Genera una contraseña segura aleatoria de la longitud indicada.
    /// </summary>
    /// <param name="length"></param>
    /// <returns></returns>
    public string GetRandomPassword(int length)
    {
        string password = string.Empty;

        try
        {
            do
            {
                password = GeneratePassword(length, 1);

                //Reemplazar caracteres complicados: ~`^\¬|° por un $
                password = Regex.Replace(password, @"[~`^\¬|°]", m => "$");

            } while (!password.Any(x => char.IsDigit(x)) ||
                     !password.Any(x => char.IsLower(x)) ||
                     !password.Any(x => char.IsUpper(x)));
        }
        catch (Exception ex)
        {
            _logService.ErrorLog("StringUtilitiesService.GetRandomPassword", ex);
            return string.Empty;
        }

        return password;
    }

    /// <summary>
    /// Genera un Hash SHA256 de una cadena aleatoria de 25 caracteres.
    /// </summary>
    /// <returns></returns>
    public string GetRandomHash()
    {
        try
        {
            string randomString = GetRandomString(25);

            using (SHA256 sha256Hash = SHA256.Create())
            {
                return GetHash(sha256Hash, randomString);
            }
        }
        catch (Exception ex)
        {
            _logService.ErrorLog("StringUtilitiesService.GetRandomHash", ex);
            return string.Empty;
        }
    }

    /// <summary>
    /// Genera un Hash con el adgoritmo y valor indicado.
    /// </summary>
    /// <param name="hashAlgorithm"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    public string GetHash(HashAlgorithm hashAlgorithm, string input)
    {
        try
        {
            // Convert the input string to a byte array and compute the hash.
            byte[] data = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            var sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }
        catch (Exception ex)
        {
            _logService.ErrorLog("StringUtilitiesService.GetHash", ex);
            throw;
        }
    }

    public static DateTime? ConvertToDateTime(String dateString)
    {
        DateTime datetime;
        string format = "dd-MM-yyyy";

        if (DateTime.TryParseExact(dateString.Trim(), format, null, DateTimeStyles.None, out datetime))

            return datetime;
        else
            return null;
    }

    public static string GeneratePassword(int length, int numberOfNonAlphanumericCharacters)
    {
        if (length < numberOfNonAlphanumericCharacters)
        {
            throw new ArgumentException("The number of non-alphanumeric characters cannot exceed the total length.");
        }

        const string alphanumericChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        const string nonAlphanumericChars = "!@#$%^&*()_-+=<>?";

        var random = new Random();

        // Generate the alphanumeric part
        var alphanumericPart = new string(Enumerable.Range(0, length - numberOfNonAlphanumericCharacters)
            .Select(_ => alphanumericChars[random.Next(alphanumericChars.Length)])
            .ToArray());

        // Generate the non-alphanumeric part
        var nonAlphanumericPart = new string(Enumerable.Range(0, numberOfNonAlphanumericCharacters)
            .Select(_ => nonAlphanumericChars[random.Next(nonAlphanumericChars.Length)])
            .ToArray());

        // Combine and shuffle the characters
        var combined = alphanumericPart + nonAlphanumericPart;
        return new string(combined.OrderBy(_ => random.Next()).ToArray());
    }



}
