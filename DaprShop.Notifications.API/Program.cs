using DaprShop.Notifications.API;

using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDaprClient();

// TODO: Add Email service

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

app.UseSwaggerUi3(c =>
{
	c.Path = "/notifications/api";
	c.DocumentPath = "/notifications/api/v1/specification.json";
});

app.MapNotificationsEndpoints();

app.Run();