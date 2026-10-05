namespace Test.Domain.Exceptions;

/// <summary>İş kuralı ihlali. API katmanında 422 olarak döner.</summary>
public class BusinessException(string message) : Exception(message);
