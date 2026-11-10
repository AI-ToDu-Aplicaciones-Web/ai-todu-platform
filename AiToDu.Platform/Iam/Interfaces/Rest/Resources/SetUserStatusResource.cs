using System.ComponentModel.DataAnnotations;
namespace AiToDu.Platform.Iam.Interfaces.Rest.Resources;

public sealed record SetUserStatusResource([Required] bool? IsActive);
