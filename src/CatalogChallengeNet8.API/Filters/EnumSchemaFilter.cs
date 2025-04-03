using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CatalogChallengeNet8.API.Filters
{
    public class EnumSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type.IsEnum)
            {
                schema.Enum = Enum.GetNames(context.Type)
                    .Select(name => (IOpenApiAny)new OpenApiString(name.ToLower()))
                    .ToList();
                schema.Type = "string"; // Ensure Swagger treats it as a string
            }
        }
    }
}
