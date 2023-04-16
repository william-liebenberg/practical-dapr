using DaprShop.Shopping.API.Services;

using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDaprClient();
builder.Services.AddScoped<CartService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Shopping Cart API",
        Description = "Shopping Cart Service"
    });
});

var app = builder.Build();

app.UseCloudEvents();
app.MapSubscribeHandler();

app.UseSwagger(c =>
{
    c.RouteTemplate = "cart/swagger/{documentName}/swagger.json";
});

app.UseSwaggerUI(o =>
{
    o.SwaggerEndpoint("v1/swagger.json", "v1");
    o.RoutePrefix = "cart/swagger";
});

app.MapShoppingCartRoutes();

app.Run();
