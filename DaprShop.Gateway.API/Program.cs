using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddReverseProxy()
    .AddApiGatewayConfiguration(builder.Configuration)
    .AddTransforms(builderContext =>
    {
        builderContext.CopyRequestHeaders = true;
        builderContext.AddOriginalHost(useOriginal: true);
        builderContext.UseDefaultForwarders = true;
    })    
    .LoadFromConfig(builder.Configuration.GetSection("DaprReverseProxy"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

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
            setup.SwaggerEndpoint(swaggerJsonEndpoint, route.RouteName);
        }
    }
});

app.UseRouting();
app.UseCors();
app.MapReverseProxy();

app.Run();
