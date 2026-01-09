using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Weddy.Application.Services;

namespace Weddy.Infrastructure.Services;

public class TokenGenerator : ITokenGenerator
{
    private const int TokenLength = 48; // 48 bytes = 64 hex chars или ~64 base64url chars
    private const int RandomSuffixLength = 5; // Длина случайного суффикса для красивого токена
    private const string Letters = "abcdefghijklmnopqrstuvwxyz"; // Только строчные буквы

    public string GenerateToken(string? coupleNames = null)
    {
        // Если переданы имена пары, генерируем красивый токен
        if (!string.IsNullOrWhiteSpace(coupleNames))
        {
            return GeneratePrettyToken(coupleNames);
        }
        
        // Иначе используем старый метод для обратной совместимости
        var bytes = new byte[TokenLength];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }
        
        // Используем base64url для URL-safe токена
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }

    private string GeneratePrettyToken(string coupleNames)
    {
        // Нормализуем имена: убираем лишние пробелы, приводим к lowercase, заменяем пробелы на дефисы
        var normalized = Regex.Replace(coupleNames.ToLowerInvariant().Trim(), @"\s+", "-");
        // Убираем все символы кроме букв, цифр и дефисов
        normalized = Regex.Replace(normalized, @"[^a-z0-9\-]", "");
        // Убираем множественные дефисы
        normalized = Regex.Replace(normalized, @"-+", "-");
        // Убираем дефисы в начале и конце
        normalized = normalized.Trim('-');
        
        // Генерируем случайный суффикс из букв
        var randomSuffix = GenerateRandomLetters(RandomSuffixLength);
        
        // Формируем токен: имена-суффикс
        return $"{normalized}-{randomSuffix}";
    }

    private string GenerateRandomLetters(int length)
    {
        var random = new byte[length];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(random);
        }
        
        var result = new StringBuilder(length);
        for (int i = 0; i < length; i++)
        {
            result.Append(Letters[random[i] % Letters.Length]);
        }
        
        return result.ToString();
    }
}

