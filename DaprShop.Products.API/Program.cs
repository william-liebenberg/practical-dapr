using DaprShop.Products.API;

using Microsoft.ApplicationInsights.Extensibility;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDaprClient();
builder.Services.AddScoped<ProductService>();

builder.Services.AddApplicationInsightsTelemetry();
builder.Services.Configure<TelemetryConfiguration>((o) =>
{
	o.TelemetryInitializers.Add(new AppInsightsTelemetryInitializer("products-api"));
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddOpenApiDocument(options =>
{
	options.DocumentName = "v1";
	options.Version = "v1";
	options.Title = "Products API";
	options.Description = "Product Service";
});

var app = builder.Build();

app.UseCloudEvents();
app.MapSubscribeHandler();

app.UseStaticFiles(new StaticFileOptions()
{
	RequestPath = "/products"
});

app.UseSwaggerUi3(c =>
{
	c.Path = "/products/api";
	c.DocumentPath = "/products/api/v1/specification.json";
});

app.MapProductEndpoints();

app.Run();