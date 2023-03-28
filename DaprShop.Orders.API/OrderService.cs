using Dapr;
using Dapr.Client;

namespace DaprShop.Orders.API;

public class OrderService
{
    private readonly ILogger<OrderService> _logger;
    private readonly DaprClient _dapr;
    private readonly BackgroundWorkerQueue _backgroundWorkerQueue;
    private readonly string _storeName = "daprshop-statestore";

    public OrderService(
        ILogger<OrderService> logger,
        DaprClient dapr,
        BackgroundWorkerQueue backgroundWorkerQueue)
    {
        _logger = logger;
        _dapr = dapr;
        _backgroundWorkerQueue = backgroundWorkerQueue;
    }

    //// Validate checks an order is correct
    //pubilc bool Validate(Order o)
    //{
    //    if (o.Amount <= 0 || len(o.LineItems) == 0 || o.Title == "" || o.ForUserID == "")
    //    {
    //        return errors.New("order failed validation")
    //        }
    //}

    public async Task<bool> OrderExists(string orderId)
    {
        return await GetOrder(orderId) != null;
    }

    public async Task<Order> GetOrder(string orderId)
    {
        try
        {
            return await _dapr.GetStateAsync<Order>(_storeName, orderId);
        }
        catch (DaprException dx)
        {
            _logger.LogError(dx, "Could not get order: {orderId}", orderId);
            throw;
        }
    }

    public async Task AddOrder(Order newOrder)
    {
        // TODO: check if order already exists...

        // save the order by order id
        try
        {
            await _dapr.SaveStateAsync(_storeName, newOrder.OrderId, newOrder);
        }
        catch (DaprException dx)
        {
            _logger.LogError(dx, "Could not save new order: {orderId}", newOrder.OrderId);
            throw;
        }

        try
        {
            // get all the orders for an existing user
            List<Order> ordersForCustomer = (await _dapr.GetStateAsync<Order[]>(_storeName, newOrder.CustomerId))?.ToList() ?? new List<Order>();

            // check for duplicates
            var duplicateOrder = ordersForCustomer.FirstOrDefault(o => o.OrderId == newOrder.OrderId);
            if (duplicateOrder is not null)
            {
                _logger.LogWarning("Order {orderId} already exists for Customer {customerId}", duplicateOrder.OrderId, newOrder.CustomerId);
            }
            else
            {
                // update the list of orders for the user
                ordersForCustomer.Add(newOrder);
            }

            // save the user orders
            await _dapr.SaveStateAsync(_storeName, newOrder.CustomerId, ordersForCustomer);
        }
        catch (DaprException dx)
        {
            _logger.LogError(dx, "Could not update orders for customer: {customerId}", newOrder.CustomerId);
            throw;
        }
    }

    public async Task<IEnumerable<Order>> GetOrdersForCustomer(string customerId)
    {
        try
        {
            return await _dapr.GetStateAsync<Order[]>(_storeName, customerId);
        }
        catch (DaprException dx)
        {
            _logger.LogError(dx, "Could not get orders for customer: {customerId}", customerId);
            throw;
        }
    }

    public async Task SetStatus(Order order, OrderStatus newStatus)
    {
        _logger.LogInformation("Progressing order {orderId} from {currentOrderStatus} to {newOrderStatus}", order.OrderId, order.Status, newStatus);

        var updatedOrder = order with { Status = newStatus };

        try
        {
            await _dapr.SaveStateAsync(_storeName, updatedOrder.OrderId, updatedOrder);
        }
        catch (DaprException dx)
        {
            _logger.LogError(dx, "Could not save new order: {orderId}", updatedOrder.OrderId);
            throw;
        }
    }

    public async Task ProcessOrder(Order order)
    {
        _logger.LogInformation("=== Processing order: {orderId}", order.OrderId);

        // check we have a new order, if not fail!
        if (order.Status != OrderStatus.OrderNew)
        {
            throw new InvalidOperationException("Can't reprocess non-new orders!");
        }

        _logger.LogInformation("Processing order: {orderId} - Saving new Order", order.OrderId);
        // save the new order
        await AddOrder(order);

        _logger.LogInformation("Processing order: {orderId} - Setting Status to {newStatus}", order.OrderId, OrderStatus.OrderReceived);
        // set status to OrderReceived
        await SetStatus(order, OrderStatus.OrderReceived);

        // save Order slip...
        //_logger.LogInformation("Processing order: {orderId} - Saving order slip", order.OrderId);
        //await SaveOrderSlip(order);

        // do some fake background processing
        var originalOrderId = order.OrderId;

        // after 30 seconds, set status to OrderProcessing
        _backgroundWorkerQueue.QueueBackgroundWorkItem(async token =>
        {
            await Task.Delay(TimeSpan.FromSeconds(10), token);
            var originalOrder = await GetOrder(originalOrderId);
            await SetStatus(originalOrder, OrderStatus.OrderProcessing);
        });

        // after 60 seconds, set status to OrderComplete
        _backgroundWorkerQueue.QueueBackgroundWorkItem(async token =>
        {
            await Task.Delay(TimeSpan.FromSeconds(10), token);
            var originalOrder = await GetOrder(originalOrderId);
            await SetStatus(originalOrder, OrderStatus.OrderComplete);
        });

        // publish the OrderComplete event
        // then, delivery/notification service can pick it up and take it to customer
    }

    //public async Task EmailNotify(Order order) { }

    //public async Task SaveReport(Order order) { } 
}