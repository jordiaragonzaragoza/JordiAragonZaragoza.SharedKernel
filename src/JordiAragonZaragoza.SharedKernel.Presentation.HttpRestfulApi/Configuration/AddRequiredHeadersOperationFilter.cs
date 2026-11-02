namespace JordiAragonZaragoza.SharedKernel.Presentation.HttpRestfulApi.Configuration
{
    using System;
    using System.Collections.Generic;
    using Microsoft.OpenApi;
    using Swashbuckle.AspNetCore.SwaggerGen;

    /// <summary>
    /// Swagger operation filter to add the execution context headers read by ExecutionContextMiddleware.
    /// </summary>
    public class AddRequiredHeadersOperationFilter : IOperationFilter
    {
        /// <summary>
        /// Apply the operation filter to add custom headers.
        /// </summary>
        /// <param name="operation">The operation to modify.</param>
        /// <param name="context">The operation filter context.</param>
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            ArgumentNullException.ThrowIfNull(operation);

            operation.Parameters ??= new List<IOpenApiParameter>();

            operation.Parameters.Add(
                new OpenApiParameter
                {
                    Name = "x-tenant-id",
                    In = ParameterLocation.Header,
                    Description = "The ID of the tenant. This header is required and must be a valid non-empty Guid.",
                    Required = true,
                    Schema = new OpenApiSchema { Type = JsonSchemaType.String, Format = "uuid", },
                });

            operation.Parameters.Add(
                new OpenApiParameter
                {
                    Name = "x-partition-id",
                    In = ParameterLocation.Header,
                    Description = "The ID of the partition. This header is optional and must be a valid non-empty Guid if provided. It is used for data partitioning in multi-tenant scenarios.",
                    Required = false,
                    Schema = new OpenApiSchema { Type = JsonSchemaType.String, Format = "uuid", },
                });

            operation.Parameters.Add(
                new OpenApiParameter
                {
                    Name = "x-domain-id",
                    In = ParameterLocation.Header,
                    Description = "The ID of the domain (for example, a cinema). This header is optional and must be a valid non-empty Guid if provided. It identifies the specific domain or context of the request.",
                    Required = false,
                    Schema = new OpenApiSchema { Type = JsonSchemaType.String, Format = "uuid", },
                });

            operation.Parameters.Add(
                new OpenApiParameter
                {
                    Name = "x-correlation-id",
                    In = ParameterLocation.Header,
                    Description = "Correlation ID for tracking related requests (optional). If not provided, a new one will be generated.",
                    Required = false,
                    Schema = new OpenApiSchema { Type = JsonSchemaType.String, Format = "uuid", },
                });

            operation.Parameters.Add(
                new OpenApiParameter
                {
                    Name = "x-causation-id",
                    In = ParameterLocation.Header,
                    Description = "Causation ID for event-driven scenarios (optional). Links this request to a prior event.",
                    Required = false,
                    Schema = new OpenApiSchema { Type = JsonSchemaType.String, Format = "uuid", },
                });
        }
    }
}
