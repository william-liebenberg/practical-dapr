using DaprShop.Orders.API;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDaprClient();
builder.Services.AddScoped<OrderService>();

builder.Services.AddHostedService<LongRunningService>();
builder.Services.AddSingleton<BackgroundWorkerQueue>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger(c =>
{
    c.RouteTemplate = "orders/swagger/{documentName}/swagger.json";
});

app.UseSwaggerUI(o =>
{
    o.SwaggerEndpoint("v1/swagger.json", "v1");
    o.RoutePrefix = "orders/swagger";
});

app.UseHttpsRedirection();
app.MapOrderEndpoints();

app.Run();
