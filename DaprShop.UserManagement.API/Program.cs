using DaprShop.DaprExtensions;
using DaprShop.UserManagement.API;

using Microsoft.ApplicationInsights.Extensibility;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDaprClient();
builder.Services.AddScoped<UserService>();

builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddSingleton<ITelemetryInitializer>(new AppInsightsTelemetryInitializer("users-api"));

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddOpenApiDocument(options =>
{
	options.DocumentName = "v1";
	options.Version = "v1";
	options.Title = "Users API";
	options.Description = "User Management Service";
});

var app = builder.Build();

app.UseCloudEvents();
app.MapSubscribeHandler();

app.UseStaticFiles(new StaticFileOptions()
{
	RequestPath = "/users"
});

app.UseSwaggerUi(c =>
{
	c.Path = "/users/api";
	c.DocumentPath = "/users/api/v1/specification.json";
});

app.MapUserManagementEndpoints();

app.Run();