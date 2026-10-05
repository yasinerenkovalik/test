namespace Test.Application.Common.Interfaces;

public sealed record AccessToken(string Token, DateTime ExpiresAtUtc);
