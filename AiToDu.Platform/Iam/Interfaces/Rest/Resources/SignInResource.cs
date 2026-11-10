using System.ComponentModel.DataAnnotations;
namespace AiToDu.Platform.Iam.Interfaces.Rest.Resources;

public sealed record SignInResource([Required, EmailAddress, StringLength(254)] string Email, [Required, StringLength(72)] string Password);
