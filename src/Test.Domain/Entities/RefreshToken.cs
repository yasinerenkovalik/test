namespace Test.Domain.Entities;

/// <summary>
/// Ham token asla saklanmaz; sadece SHA-256 hash'i tutulur.
/// Rotation: kullanılan token iptal edilir ve yerine geçen token'ın hash'i işaretlenir.
/// </summary>
public sealed class RefreshToken
{
    private RefreshToken() { } // EF Core

    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;
    public string TokenHash { get; private set; } = null!;
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? RevokedAtUtc { get; private set; }
    public string? ReplacedByTokenHash { get; private set; }

    public bool IsActive(DateTime nowUtc) => RevokedAtUtc is null && ExpiresAtUtc > nowUtc;

    public static RefreshToken Create(Guid userId, string tokenHash, DateTime expiresAtUtc)
        => new()
        {
            UserId = userId,
            TokenHash = tokenHash,
            ExpiresAtUtc = expiresAtUtc,
            CreatedAtUtc = DateTime.UtcNow
        };

    public void Revoke(DateTime nowUtc, string? replacedByTokenHash = null)
    {
        RevokedAtUtc ??= nowUtc;
        ReplacedByTokenHash = replacedByTokenHash ?? ReplacedByTokenHash;
    }
}
