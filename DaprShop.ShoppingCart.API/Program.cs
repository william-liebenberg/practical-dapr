using Dapr.Client;

using DaprShop.Shopping.API;
using DaprShop.Shopping.API.Services;

using Microsoft.ApplicationInsights.Extensibility;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDaprClient();

builder.Services.AddKeyedScoped("products-api", (sp, _) => DaprClient.CreateInvokeHttpClient("products-api"));
builder.Services.AddKeyedScoped("users-api", (sp, _) => DaprClient.CreateInvokeHttpClient("users-api"));

builder.Services.AddScoped<CartService>();

builder.Services.AddApplicationInsightsTelemetry();
builder.Services.Configure<TelemetryConfiguration>((o) =>
{
	o.TelemetryInitializers.Add(new AppInsightsTelemetryInitializer("cart-api"));
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddOpenApiDocument(options =>
{
	options.DocumentName = "v1";
	options.Version = "v1";
	options.Title = "Shopping Cart API";
	options.Description = "Shopping Cart Service";
});

var app = builder.Build();

app.UseCloudEvents();
app.MapSubscribeHandler();

app.UseStaticFiles(new StaticFileOptions()
{
	RequestPath = "/cart"
});

app.UseSwaggerUi(c =>
{
	c.Path = "/cart/api";
	c.DocumentPath = "/cart/api/v1/specification.json";
});

app.MapShoppingCartRoutes();

app.Run();