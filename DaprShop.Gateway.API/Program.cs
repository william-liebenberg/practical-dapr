using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("DaprReverseProxy"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => 
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "DaprShop Gateway API",
        Description = "All shop services"
    });
});

var app = builder.Build();

app.MapReverseProxy();

app.UseSwagger(options =>
{
    options.RouteTemplate = "swagger/{documentName}/swagger.json";
});

app.UseSwaggerUI(setup =>
{
    setup.RoutePrefix = string.Empty;
    setup.SwaggerEndpoint("swagger/v1/daprshop.json", "DaprShop API Gateway");
    setup.ConfigObject.DisplayRequestDuration = true;

    // both flags work to enable the `url.primaryName` query parameter to be respected
    // see: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/2516
    setup.ConfigObject.DeepLinking = true;
    setup.ConfigObject.AdditionalItems["queryConfigEnabled"] = true;

    IConfigurationSection apiRoutesSection = builder.Configuration.GetSection("ApiRoutes");
    ApiRouteConfig[]? apiRoutes = apiRoutesSection.Get<ApiRouteConfig[]>();
    if(apiRoutes != null)
    {
        foreach (ApiRouteConfig route in apiRoutes)
        {
            var swaggerJsonEndpoint = $"{route.RoutePrefix}/{route.SwaggerJsonUrl}";
            Console.WriteLine($"Adding Swagger Endpoint: {swaggerJsonEndpoint}");
            setup.SwaggerEndpoint(swaggerJsonEndpoint, route.RouteName);
        }
    }
});

app.UseStaticFiles();

app.MapGet("info", ([FromServices] IConfiguration config) =>
{
    IConfigurationSection apiRoutesSection = config.GetSection("ApiRoutes");
    ApiRouteConfig[]? routes = apiRoutesSection.Get<ApiRouteConfig[]>();
    return Task.FromResult(Results.Ok(routes));
})
    .WithOpenApi()
    .WithTags(new []{"Gateway"})
    .WithName("Info");

app.Run();
