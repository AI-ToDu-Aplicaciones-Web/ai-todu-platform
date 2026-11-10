namespace AiToDu.Platform.Shared.Application.Exceptions;

/// <summary>Persistence adapter translates provider-specific uniqueness/concurrency failures.</summary>
public sealed class PersistenceConflictException(string code, Exception inner) : Exception(code, inner)
{ public string Code { get; } = code; }
