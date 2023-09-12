using DaprShop.Orders.API;

using Microsoft.ApplicationInsights.Extensibility;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDaprClient();
builder.Services.AddScoped<OrderService>();

builder.Services.AddHostedService<LongRunningService>();
builder.Services.AddSingleton<BackgroundWorkerQueue>();

builder.Services.AddApplicationInsightsTelemetry();
builder.Services.Configure<TelemetryConfiguration>((o) =>
{
	o.TelemetryInitializers.Add(new AppInsightsTelemetryInitializer("orders-api"));
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddOpenApiDocument(options =>
{
	options.DocumentName = "v1";
	options.Version = "v1";
	options.Title = "Orders API";
	options.Description = "Ordering Service";
});

var app = builder.Build();

app.UseCloudEvents();
app.MapSubscribeHandler();

app.UseStaticFiles(new StaticFileOptions()
{
	RequestPath = "/orders"
});

app.UseSwaggerUi3(c =>
{
	c.Path = "/orders/api";
	c.DocumentPath = "/orders/api/v1/specification.json";
});

app.MapOrderEndpoints();

app.Run();