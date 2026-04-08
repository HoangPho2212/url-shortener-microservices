using System.Security.Cryptography;
using System.Text;

namespace UrlManagement.Api.Services;

public interface IUrlShorteningService
{
    string GenerateShortCode(int length = 8);
}

public class UrlShorteningService : IUrlShorteningService
{
    private const string Alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public string GenerateShortCode(int length = 8)
    {
        var result = new StringBuilder(length);
        using (var rng = RandomNumberGenerator.Create())
        {
            var bytes = new byte[length];
            rng.GetBytes(bytes);

            foreach (var b in bytes)
            {
                result.Append(Alphabet[b % Alphabet.Length]);
            }
        }

        return result.ToString();
    }
}
