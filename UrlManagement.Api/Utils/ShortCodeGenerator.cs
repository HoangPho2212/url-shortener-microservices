namespace UrlManagement.Api.Utils;

public static class ShortCodeGenerator
{
    private const string Alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public static string Encode(long input)
    {
        if (input == 0) return Alphabet[0].ToString();

        var shortCode = string.Empty;
        while (input > 0)
        {
            shortCode = Alphabet[(int)(input % 62)] + shortCode;
            input /= 62;
        }
        return shortCode;
    }
}