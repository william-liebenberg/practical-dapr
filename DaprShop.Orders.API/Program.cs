using DaprShop.Orders.API;

using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDaprClient();
builder.Services.AddScoped<OrderService>();

builder.Services.AddHostedService<LongRunningService>();
builder.Services.AddSingleton<BackgroundWorkerQueue>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Orders API",
        Description = "Ordering Service"
    });
});

var app = builder.Build();

app.UseCloudEvents();
app.MapSubscribeHandler();

app.UseSwagger(c =>
{
    c.RouteTemplate = "orders/swagger/{documentName}/swagger.json";
});

app.UseSwaggerUI(o =>
{
    o.SwaggerEndpoint("v1/swagger.json", "v1");
    o.RoutePrefix = "orders/swagger";
});

app.MapOrderEndpoints();

app.Run();
