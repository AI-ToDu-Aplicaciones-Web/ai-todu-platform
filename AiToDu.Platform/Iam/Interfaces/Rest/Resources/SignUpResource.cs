using System.ComponentModel.DataAnnotations;
namespace AiToDu.Platform.Iam.Interfaces.Rest.Resources;

public sealed record SignUpResource([Required, StringLength(160)] string OrganizationName, [Required, StringLength(160)] string FullName, [Required, EmailAddress, StringLength(254)] string Email, [Required, StringLength(72, MinimumLength = 12)] string Password);
