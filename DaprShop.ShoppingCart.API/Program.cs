using DaprShop.Shopping.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDaprClient();
builder.Services.AddScoped<CartService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger(c =>
{
    c.RouteTemplate = "cart/swagger/{documentName}/swagger.json";
});

app.UseSwaggerUI(o =>
{
    o.SwaggerEndpoint("v1/swagger.json", "v1");
    o.RoutePrefix = "cart/swagger";
});

app.UseHttpsRedirection();
app.MapShoppingCartRoutes();

app.Run();
