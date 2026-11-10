namespace AiToDu.Platform.Shared.Application.Model;

public sealed record Error(string Code, ErrorKind Kind);
