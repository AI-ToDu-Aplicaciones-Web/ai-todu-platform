using AiToDu.Platform.Iam.Resources;
using AiToDu.Platform.Shared.Domain.Model.Errors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using ProblemDetailsFactory = AiToDu.Platform.Shared.Interfaces.Rest.ProblemDetails.ProblemDetailsFactory;
namespace AiToDu.Platform.Iam.Interfaces.Rest.Transform;

public static class IamActionResultAssembler
{
    public static IActionResult ToProblem(ControllerBase controller, Error error, Enum? errorEnum,
        ProblemDetailsFactory factory, IStringLocalizer<IamErrorMessages> messages)
    {
        var status = error.Type switch
        {
            EErrorType.Conflict => 409, EErrorType.Unauthorized => 401,
            EErrorType.Forbidden => 403, EErrorType.NotFound => 404, _ => 400
        };
        var localized = messages[errorEnum?.ToString() ?? error.Code];
        var result = factory.CreateProblemDetails(controller, status, errorEnum,
            localized.ResourceNotFound ? error.Message : localized.Value);
        if (result is ObjectResult { Value: ProblemDetails details }) details.Extensions["code"] = error.Code;
        return result;
    }
}
