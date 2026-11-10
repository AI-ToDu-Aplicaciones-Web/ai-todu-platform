using AiToDu.Platform.Iam.Application.CommandServices;
using AiToDu.Platform.Iam.Application.QueryServices;
using AiToDu.Platform.Iam.Domain.Model.Queries;
using AiToDu.Platform.Iam.Interfaces.Rest.Resources;
using AiToDu.Platform.Iam.Interfaces.Rest.Transform;
using AiToDu.Platform.Iam.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Localization;
using ProblemDetailsFactory = AiToDu.Platform.Shared.Interfaces.Rest.ProblemDetails.ProblemDetailsFactory;
namespace AiToDu.Platform.Iam.Interfaces.Rest;

[ApiController, Route("api/v1/authentication"), Tags("IAM Authentication")]
[EnableRateLimiting("authentication")]
public sealed class AuthenticationController(IUserCommandService commands, IUserQueryService queries,
    ProblemDetailsFactory problems, IStringLocalizer<IamErrorMessages> messages) : ControllerBase
{
    [AllowAnonymous, HttpPost("sign-up")]
    public async Task<IActionResult> SignUp(SignUpResource resource, CancellationToken ct)
    {
        Response.Headers.CacheControl = "no-store";
        var result = await commands.Handle(SignUpCommandFromResourceAssembler.ToCommandFromResource(resource), ct);
        return result.IsSuccess
            ? Created("/api/v1/authentication/me", UserResourceFromEntityAssembler.ToResourceFromEntity(result.Value!))
            : IamActionResultAssembler.ToProblem(this, result.DomainError, result.Error, problems, messages);
    }

    [AllowAnonymous, HttpPost("sign-in")]
    public async Task<IActionResult> SignIn(SignInResource resource, CancellationToken ct)
    {
        Response.Headers.CacheControl = "no-store";
        var result = await commands.Handle(SignInCommandFromResourceAssembler.ToCommandFromResource(resource), ct);
        return result.IsSuccess
            ? Ok(AuthenticatedUserResourceFromEntityAssembler.ToResourceFromEntity(result.Value!))
            : IamActionResultAssembler.ToProblem(this, result.DomainError, result.Error, problems, messages);
    }

    [Authorize, HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        Response.Headers.CacheControl = "no-store";
        var result = await queries.Handle(new GetCurrentUserQuery(), ct);
        return result.IsSuccess ? Ok(UserResourceFromEntityAssembler.ToResourceFromEntity(result.Value!))
            : IamActionResultAssembler.ToProblem(this, result.DomainError, result.Error, problems, messages);
    }
}
