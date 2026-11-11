using AiToDu.Platform.Iam.Application.CommandServices;
using AiToDu.Platform.Iam.Application.QueryServices;
using AiToDu.Platform.Iam.Domain.Model.Commands;
using AiToDu.Platform.Iam.Domain.Model.Queries;
using AiToDu.Platform.Iam.Domain.Model.ValueObjects;
using AiToDu.Platform.Iam.Interfaces.Rest.Resources;
using AiToDu.Platform.Iam.Interfaces.Rest.Transform;
using AiToDu.Platform.Iam.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Localization;
using ProblemDetailsFactory = AiToDu.Platform.Shared.Interfaces.Rest.ProblemDetails.ProblemDetailsFactory;
namespace AiToDu.Platform.Iam.Interfaces.Rest;

[ApiController, Route("api/v1/users"), Tags("IAM Organization Users")]
[Authorize(Roles = UserRole.Owner)]
[EnableRateLimiting("authentication")]
public sealed class UsersController(IUserCommandService commands, IUserQueryService queries,
    ProblemDetailsFactory problems, IStringLocalizer<IamErrorMessages> messages) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        Response.Headers.CacheControl = "no-store";
        var result = await queries.Handle(new GetOrganizationUsersQuery(), ct);
        return result.IsSuccess ? Ok(result.Value!.Select(UserResourceFromEntityAssembler.ToResourceFromEntity))
            : IamActionResultAssembler.ToProblem(this, result.DomainError, result.Error, problems, messages);
    }

    [AiToDu.Platform.Subscriptions.Interfaces.Rest.PlanAllocation("users")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateOrganizationUserResource resource, CancellationToken ct)
    {
        Response.Headers.CacheControl = "no-store";
        var result = await commands.Handle(CreateOrganizationUserCommandFromResourceAssembler.ToCommandFromResource(resource), ct);
        return result.IsSuccess ? StatusCode(201, UserResourceFromEntityAssembler.ToResourceFromEntity(result.Value!))
            : IamActionResultAssembler.ToProblem(this, result.DomainError, result.Error, problems, messages);
    }

    public sealed record AccessResource(string Role, bool? Inventory, bool? Sales);
    [HttpPatch("{id:guid}/access")]
    public async Task<IActionResult> SetAccess(Guid id, AccessResource resource, CancellationToken ct)
    {
        var result = await commands.SetAccess(id, resource.Role, resource.Inventory, resource.Sales, ct);
        return result.IsSuccess ? Ok(UserResourceFromEntityAssembler.ToResourceFromEntity(result.Value!))
            : IamActionResultAssembler.ToProblem(this, result.DomainError, result.Error, problems, messages);
    }
    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> SetStatus(Guid id, SetUserStatusResource resource, CancellationToken ct)
    {
        var result = await commands.Handle(new SetUserStatusCommand(id, resource.IsActive!.Value), ct);
        return result.IsSuccess ? Ok(UserResourceFromEntityAssembler.ToResourceFromEntity(result.Value!))
            : IamActionResultAssembler.ToProblem(this, result.DomainError, result.Error, problems, messages);
    }
}
