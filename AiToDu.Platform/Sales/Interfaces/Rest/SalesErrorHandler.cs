using AiToDu.Platform.Sales.Application;
using Microsoft.AspNetCore.Diagnostics;

namespace AiToDu.Platform.Sales.Interfaces.Rest;
public sealed class SalesErrorHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        if (exception is not SalesError error) return false;
        await Results.Problem(detail:error.Message, statusCode:error.Status).ExecuteAsync(context);
        return true;
    }
}
