using Microsoft.Extensions.Configuration;

namespace Test.Infrastructure.Identity;

public sealed class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenDays { get; set; } = 7;

    /// <summary>Eksik/zayıf yapılandırmada uygulama açılışta düşer (fail fast).</summary>
    public static JwtSettings FromConfiguration(IConfiguration configuration)
    {
        var settings = configuration.GetSection(SectionName).Get<JwtSettings>() ?? new JwtSettings();

        if (settings.Secret.Length < 32)
            throw new InvalidOperationException(
                "Jwt:Secret must be configured with at least 32 characters. " +
                "Use 'dotnet user-secrets' or the JWT__SECRET environment variable.");
        if (string.IsNullOrWhiteSpace(settings.Issuer) || string.IsNullOrWhiteSpace(settings.Audience))
            throw new InvalidOperationException("Jwt:Issuer and Jwt:Audience must be configured.");

        return settings;
    }
}
