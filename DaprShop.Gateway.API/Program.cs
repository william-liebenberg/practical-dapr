using Microsoft.AspNetCore.Mvc;

using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddReverseProxy()
    //.AddTransforms(builderContext =>
    //{
    //    builderContext.CopyRequestHeaders = true;
    //    builderContext.AddOriginalHost(useOriginal: true);
    //    builderContext.UseDefaultForwarders = true;
    //})
    //.AddApiGatewayConfiguration(builder.Configuration);    
    .LoadFromConfig(builder.Configuration.GetSection("DaprReverseProxy"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.MapReverseProxy();

app.UseSwagger(options =>
{
    options.RouteTemplate = "swagger/{documentName}/swagger.json";
});

app.UseSwaggerUI(setup =>
{
    setup.RoutePrefix = string.Empty;
    setup.SwaggerEndpoint("swagger/v1/swagger.json", "Api Gateway");
    
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

app.MapGet("info", ([FromServices] IConfiguration config) =>
{
    IConfigurationSection apiRoutesSection = config.GetSection("ApiRoutes");
    ApiRouteConfig[]? routes = apiRoutesSection.Get<ApiRouteConfig[]>();
    return Task.FromResult(Results.Ok(routes));
})
            .WithOpenApi()
            .WithName("Info");


//app.UseRouting();
//app.UseCors();

app.Run();
