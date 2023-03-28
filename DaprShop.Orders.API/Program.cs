using System;

using Dapr.Client.Autogen.Grpc.v1;

using DaprShop.Orders.API;

using Google.Api;

using Microsoft.AspNetCore.Mvc;

using static System.Runtime.InteropServices.JavaScript.JSType;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDaprClient();
builder.Services.AddScoped<OrderService>();

builder.Services.AddHostedService<LongRunningService>();
builder.Services.AddSingleton<BackgroundWorkerQueue>();

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

// add order

// get order

// get orders for customer



app.MapGet("/get", async (string orderId, [FromServices] OrderService orderService) =>
{
    var result = await orderService.GetOrder(orderId);
    return Results.Ok(result);
})
    .WithName("GetOrder")
    .WithOpenApi();

app.MapGet("/customer", async (string customerId, [FromServices] OrderService orderService) =>
{
    var result = await orderService.GetOrdersForCustomer(customerId);
    return Results.Ok(result);
})
    .WithName("GetOrdersForCustomer")
    .WithOpenApi();

app.MapPost("/sumbit", async ([FromBody] Order order, [FromServices] OrderService orderService) =>
{
    // await orderService.AddOrder(order);
    await orderService.ProcessOrder(order);
    return Results.Ok();
})
    .WithName("ReceiveOrder")
    .WithOpenApi()
    .WithTopic("daprshop-pubsub", "daprshop.orders");

app.Run();
