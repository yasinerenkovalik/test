using System.Text.Json;

namespace Test.Application.Common.Behaviors;

/// <summary>Request'i loglamadan önce password/token/secret/hash alanlarını maskeler.</summary>
public static class SensitiveDataSanitizer
{
    private static readonly string[] SensitiveKeywords = ["password", "token", "secret", "hash"];

    public static string ToSafeJson(object request)
    {
        try
        {
            var values = new Dictionary<string, object?>();
            foreach (var prop in request.GetType().GetProperties())
            {
                var sensitive = SensitiveKeywords.Any(k => prop.Name.Contains(k, StringComparison.OrdinalIgnoreCase));
                values[prop.Name] = sensitive ? "***" : prop.GetValue(request);
            }
            return JsonSerializer.Serialize(values);
        }
        catch
        {
            return "{}"; // loglama asla isteği bozmamalı
        }
    }
}
