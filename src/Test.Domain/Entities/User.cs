using Test.Domain.Common;
using Test.Domain.Enums;

namespace Test.Domain.Entities;

public sealed class User : BaseEntity
{
    private User() { } // EF Core

    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public string? PhoneNumber { get; private set; }
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; }

    public ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();

    public static User Create(string firstName, string lastName, string email, string passwordHash,
        string? phoneNumber, UserRole role = UserRole.User)
        => new()
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email.Trim().ToLowerInvariant(), // e-posta her zaman küçük harfle saklanır
            PasswordHash = passwordHash,
            PhoneNumber = phoneNumber,
            Role = role,
            IsActive = true
        };

    public void UpdateProfile(string firstName, string lastName, string? phoneNumber)
    {
        FirstName = firstName;
        LastName = lastName;
        PhoneNumber = phoneNumber;
    }

    public void SetPasswordHash(string passwordHash) => PasswordHash = passwordHash;
}
