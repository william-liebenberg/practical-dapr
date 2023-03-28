using DaprShop.Products.API;

using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDaprClient();
builder.Services.AddScoped<ProductService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.MapGet("/get", async (string productId, [FromServices] ProductService productService) =>
{
    var result = await productService.GetProduct(productId);
    return Results.Ok(result);
})
.WithName("GetProduct")
.WithOpenApi();

app.MapGet("/catalogue", async ([FromServices] ProductService productService) =>
{
    var results = await productService.ListAll();
    return Results.Ok(results);
})
.WithName("GetCatalogue")
.WithOpenApi();

app.MapGet("/search", async (string field, string searchTerm, [FromServices] ProductService productService) =>
{
    var results = await productService.Search(field, searchTerm);
    return Results.Ok(results);
})
.WithName("Search")
.WithOpenApi();

app.MapPost("/seed", async ([FromServices] ProductService productService) =>
{
    await productService.Seed();
})
.WithName("Seed")
.WithOpenApi();

app.Run();

