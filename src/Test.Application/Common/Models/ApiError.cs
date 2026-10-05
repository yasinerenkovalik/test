namespace Test.Application.Common.Models;

public sealed record ApiError(string? Field, string Message);

/// <summary>Tüm API cevaplarının ortak zarfı.</summary>
