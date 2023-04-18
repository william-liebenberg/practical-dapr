using DaprShop.Shopping.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDaprClient();
builder.Services.AddScoped<CartService>();

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

app.UseSwaggerUi3(c =>
{
	c.Path = "/cart/api";
	c.DocumentPath = "/cart/api/v1/specification.json";
});

app.MapShoppingCartRoutes();

app.Run();
