using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace AiToDu.Platform.Shared.Infrastructure.Interfaces.AspNetCore.OpenApi;

public sealed class BearerSecuritySchemeTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>
        {
            ["Bearer"] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JWT issued by IAM with sub, organization_id and role claims."
            }
        };
        foreach (var path in document.Paths.Where(x => x.Key != "/api/v1/health"))
            if (path.Value.Operations is { } operations)
                foreach (var operation in operations.Values)
                    operation.Security = [new OpenApiSecurityRequirement { [new OpenApiSecuritySchemeReference("Bearer", document)] = [] }];
        return Task.CompletedTask;
    }
}
