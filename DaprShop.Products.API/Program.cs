using DaprShop.Products.API;

using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDaprClient();
builder.Services.AddScoped<ProductService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Products API",
        Description = "Products Service"
    });
});

var app = builder.Build();

app.UseCloudEvents();
app.MapSubscribeHandler();

app.UseSwagger(c =>
{
    c.RouteTemplate = "products/swagger/{documentName}/swagger.json";
});

app.UseSwaggerUI(o =>
{
    o.SwaggerEndpoint("v1/swagger.json", "v1");
    o.RoutePrefix = "products/swagger";
});

app.MapProductEndpoints();
 
app.Run();