using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace Barcoder;

[AttributeUsage(AttributeTargets.Method)]
public class FileResultContentTypeAttribute(string contentType) : Attribute
{
    /// <summary>
    /// Content type of the file e.g. image/png
    /// </summary>
    public string ContentType { get; } = contentType;
}


public class FileResultContentTypeOperationFilter : IOpenApiOperationTransformer
{

    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        var requestAttribute = context.Description.ActionDescriptor.EndpointMetadata.OfType<FileResultContentTypeAttribute>()
            .Cast<FileResultContentTypeAttribute>()
            .FirstOrDefault();

        if (requestAttribute == null) return Task.CompletedTask;
        
        operation.Responses.Add(StatusCodes.Status200OK.ToString(), new OpenApiResponse
        {
            Content = new Dictionary<string, OpenApiMediaType>
            {
                {
                    requestAttribute.ContentType, new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "string",
                            Format = "binary"
                        }
                    }
                }
            }
        });
        return Task.CompletedTask;
    }
}