using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services
	.AddReverseProxy()
	.LoadFromConfig(builder.Configuration.GetSection("DaprReverseProxy"));

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddOpenApiDocument(options =>
{
	options.DocumentName = "v1";
	options.Version = "v1";
	options.Title = "DaprShop Gateway API";
	options.Description = "All shop services";
});

var app = builder.Build();

app.MapReverseProxy();

app.UseStaticFiles();

app.UseSwaggerUi3(c =>
{
	// set the swagger ui prefix
	c.Path = "/api";

	// make sure we add the merged api gateway OpenApiSpec doc
	c.SwaggerRoutes.Add(new NSwag.AspNetCore.SwaggerUi3Route("DaprShop Gateway", "/api/v1/daprshop.json"));

	// add all the openapi spec routes for each of the configured downstream services
	ApiRouteConfig[]? apiRoutes = builder.Configuration.GetSection("ApiRoutes").Get<ApiRouteConfig[]>();
	if (apiRoutes != null)
	{
		foreach (ApiRouteConfig route in apiRoutes)
		{
			var swaggerJsonEndpoint = $"{route.RoutePrefix}/{route.OpenApiSpecUrl}";
			Console.WriteLine($"Adding Swagger Endpoint: {swaggerJsonEndpoint}");
			c.SwaggerRoutes.Add(new NSwag.AspNetCore.SwaggerUi3Route(route.RouteName, swaggerJsonEndpoint));
		}
	}

	c.DocExpansion = "list";
});

app.MapGet("info", ([FromServices] IConfiguration config) =>
{
	IConfigurationSection apiRoutesSection = config.GetSection("ApiRoutes");
	ApiRouteConfig[]? routes = apiRoutesSection.Get<ApiRouteConfig[]>();
	return Task.FromResult(Results.Ok(routes));
})
	//.WithOpenApi()
	.WithTags(new[] { "Gateway" })
	.WithName("Info");

app.Run();