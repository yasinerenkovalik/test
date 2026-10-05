namespace Test.Application.Common.Exceptions;

public sealed class UnauthorizedException(string message = "Authentication failed.")
    : Exception(message);
