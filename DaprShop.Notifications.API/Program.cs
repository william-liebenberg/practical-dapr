using DaprShop.DaprExtensions;
using DaprShop.Notifications.API;

using Microsoft.ApplicationInsights.Extensibility;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDaprClient();

// TODO: Add Email service

builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddSingleton<ITelemetryInitializer>(new AppInsightsTelemetryInitializer("notifications-api"));

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddOpenApiDocument(options =>
{
	options.DocumentName = "v1";
	options.Version = "v1";
	options.Title = "Notifications API";
	options.Description = "Notifications Service";
});

var app = builder.Build();

app.UseCloudEvents();
app.MapSubscribeHandler();

app.UseStaticFiles(new StaticFileOptions()
{
	RequestPath = "/notifications"
});

app.UseSwaggerUi(c =>
{
	c.Path = "/notifications/api";
	c.DocumentPath = "/notifications/api/v1/specification.json";
	c.CustomStylesheetPath = "/notifications/api/SwaggerDark.css";
});

app.MapNotificationsEndpoints();

app.Run();