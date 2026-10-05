namespace Test.Application.Common.Exceptions;

public sealed class NotFoundException(string entity, object key)
    : Exception($"{entity} ({key}) was not found.");
