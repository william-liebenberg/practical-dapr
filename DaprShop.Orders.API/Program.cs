using System;

using Dapr.Client.Autogen.Grpc.v1;

using DaprShop.Orders.API;

using Google.Api;
using Google.Protobuf.WellKnownTypes;

using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

using static System.Runtime.InteropServices.JavaScript.JSType;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDaprClient();
builder.Services.AddScoped<OrderService>();

builder.Services.AddHostedService<LongRunningService>();
builder.Services.AddSingleton<BackgroundWorkerQueue>();

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
    c.RouteTemplate = "orders/swagger/{documentName}/swagger.json";

    //c.PreSerializeFilters.Add((swagger, httpReq) =>
    //{
    //    //Clear servers -element in swagger.json because it got the wrong port when hosted behind reverse proxy
    //    swagger.Servers.Clear();
    //});

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
    o.RoutePrefix = "orders/swagger";
});

app.UseForwardedHeaders(); app.UseHttpsRedirection();

app.MapGet("/orders/get", async (string orderId, [FromServices] OrderService orderService) =>
{
    var result = await orderService.GetOrder(orderId);
    return Results.Ok(result);
})
    .WithName("GetOrder")
    .WithOpenApi();

app.MapGet("/orders/customer", async (string customerId, [FromServices] OrderService orderService) =>
{
    var result = await orderService.GetOrdersForCustomer(customerId);
    return Results.Ok(result);
})
    .WithName("GetOrdersForCustomer")
    .WithOpenApi();

app.MapPost("/orders/sumbit", async ([FromBody] Order order, [FromServices] OrderService orderService) =>
{
    // await orderService.AddOrder(order);
    await orderService.ProcessOrder(order);
    return Results.Ok();
})
    .WithName("ReceiveOrder")
    .WithOpenApi()
    .WithTopic("daprshop-pubsub", "daprshop.orders");

app.Run();
