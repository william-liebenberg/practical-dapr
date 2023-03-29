using DaprShop.Products.API;

using Google.Protobuf.WellKnownTypes;

using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDaprClient();
builder.Services.AddScoped<ProductService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger(c =>
{
    c.RouteTemplate = "products/swagger/{documentName}/swagger.json";
    c.PreSerializeFilters.Add((swaggerDoc, httpRequest) =>
    {
        if (!httpRequest.Headers.ContainsKey("X-Forwarded-Host")) return;
        var basePath = "";
        var serverUrl = $"{httpRequest.Scheme}://{httpRequest.Headers["X-Forwarded-Host"]}/{basePath}";
        swaggerDoc.Servers = new List<OpenApiServer> { new OpenApiServer { Url = serverUrl } };
    });
});
app.UseSwaggerUI(o =>
{
    o.SwaggerEndpoint("v1/swagger.json", "v1");
    o.RoutePrefix = "products/swagger";
});

app.UseForwardedHeaders();
app.UseHttpsRedirection();

app.MapGet("/products/get", async (string productId, [FromServices] ProductService productService) =>
{
    var result = await productService.GetProduct(productId);
    return Results.Ok(result);
})
.WithName("GetProduct")
.WithOpenApi();

app.MapGet("/products/catalogue", async ([FromServices] ProductService productService) =>
{
    var results = await productService.ListAll();
    return Results.Ok(results);
})
.WithName("GetCatalogue")
.WithOpenApi();

app.MapGet("/products/search", async (string field, string searchTerm, [FromServices] ProductService productService) =>
{
    var results = await productService.Search(field, searchTerm);
    return Results.Ok(results);
})
.WithName("Search")
.WithOpenApi();

app.MapPost("/products/seed", async ([FromServices] ProductService productService) =>
{
    await productService.Seed();
})
.WithName("Seed")
.WithOpenApi();

app.Run();

