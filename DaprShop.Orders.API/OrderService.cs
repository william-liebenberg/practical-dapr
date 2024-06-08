using Dapr;
using Dapr.Client;

using DaprShop.Contracts.Entities;
using DaprShop.Contracts.Events;

namespace DaprShop.Orders.API;

public class OrderService
{
	private readonly ILogger<OrderService> _logger;
	private readonly DaprClient _dapr;
	private readonly BackgroundWorkerQueue _backgroundWorkerQueue;
	private readonly string _storeName = "daprshop-statestore";
	private readonly string _pubsubName = "daprshop-pubsub";
	private readonly string _orderStatusTopic = "daprshop.orders.status";
	private readonly string _orderCompletedTopic = "daprshop.orders.completed";

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
		return await GetOrder(orderId) is not null;
	}

	public async Task<Order?> GetOrder(string orderId)
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
			IEnumerable<Order> ordersForUser = await GetOrdersForUser(newOrder.Username);

			// check for duplicates
			var duplicateOrder = ordersForUser.FirstOrDefault(o => o.OrderId == newOrder.OrderId);
			if (duplicateOrder is not null)
			{
				_logger.LogWarning("Order {orderId} already exists for User {username}", duplicateOrder.OrderId, newOrder.Username);
			}
			else
			{
				// update the list of orders for the user
				var userOrders = new UserOrders()
				{
					Username = newOrder.Username,
					Orders = new(ordersForUser.Select(o => o.OrderId))
				};
				userOrders.Orders.Add(newOrder.OrderId);

				await SaveOrdersForUser(userOrders);
			}
		}
		catch (DaprException dx)
		{
			_logger.LogError(dx, "Could not update orders for user: {username}", newOrder.Username);
			throw;
		}
	}

	public async Task<IEnumerable<Order>> GetOrdersForUser(string username)
	{
		try
		{
			var userOrders = await _dapr.GetStateAsync<UserOrders>(_storeName, username);
			userOrders ??= new UserOrders
			{
				Username = username
			};

			List<Order> orders = new();
			foreach (string orderId in userOrders.Orders)
			{
				var order = await _dapr.GetStateAsync<Order>(_storeName, orderId);
				orders.Add(order);
			}
			return orders;
		}
		catch (DaprException dx)
		{
			_logger.LogError(dx, "Could not get orders for user: {username}", username);
			throw;
		}
	}

	private async Task SaveOrdersForUser(UserOrders ordersForUser)
	{
		try
		{
			// save the user orders
			await _dapr.SaveStateAsync(_storeName, ordersForUser.Username, ordersForUser);
		}
		catch (DaprException dx)
		{
			_logger.LogError(dx, "Could not save orders for user: {username}", ordersForUser.Username);
			throw;
		}
	}

	public async Task SetStatus(Order? order, OrderStatus newStatus, CancellationToken cancellationToken)
	{
		if (order is null)
		{
			_logger.LogDebug("Can't set status of null order to {newOrderStatus}", newStatus);
			return;
		}

		_logger.LogInformation("Progressing order {orderId} from {currentOrderStatus} to {newOrderStatus}", order.OrderId, order.Status, newStatus);

		var updatedOrder = order with { Status = newStatus };

		try
		{
			await _dapr.SaveStateAsync(_storeName, updatedOrder.OrderId, updatedOrder, cancellationToken: cancellationToken);

			OrderStatusChanged statusChangedEvent = new()
			{
				OrderId = updatedOrder.OrderId,
				CurrentStatus = updatedOrder.Status,
				PreviousStatus = order.Status
			};

			await _dapr.PublishEventAsync(_pubsubName, _orderStatusTopic, statusChangedEvent, cancellationToken: cancellationToken);
		}
		catch (DaprException dx)
		{
			_logger.LogError(dx, "Could not save new order: {orderId}", updatedOrder.OrderId);
			throw;
		}
	}

	public async Task ProcessOrder(Order order, CancellationToken cancellationToken)
	{
		_logger.LogInformation("=== Processing order: {orderId}", order.OrderId);

		// check we have a new order, if not fail!
		if (order.Status != OrderStatus.OrderNew)
		{
			// instead of throwing...we could check the current status and respond nicely...
			return;
		}

		_logger.LogInformation("Processing order: {orderId} - Saving new Order", order.OrderId);

		// save the new order
		await AddOrder(order);

		// set status to OrderReceived
		await SetStatus(order, OrderStatus.OrderReceived, cancellationToken);

		// do some fake background processing
		var originalOrderId = order.OrderId;

		// after 30 seconds, set status to OrderProcessing
		_backgroundWorkerQueue.QueueBackgroundWorkItem(async token =>
		{
			await Task.Delay(TimeSpan.FromSeconds(10), token);
			var originalOrder = await GetOrder(originalOrderId);
			await SetStatus(originalOrder, OrderStatus.OrderProcessing, token);
		});

		// after 60 seconds, set status to OrderComplete
		_backgroundWorkerQueue.QueueBackgroundWorkItem(async token =>
		{
			await Task.Delay(TimeSpan.FromSeconds(10), token);
			var originalOrder = await GetOrder(originalOrderId);
			await SetStatus(originalOrder, OrderStatus.OrderComplete, token);
		});

		// publish the OrderComplete event
		// then, delivery/notification service can pick it up and take it to user
		_backgroundWorkerQueue.QueueBackgroundWorkItem(async token =>
		{
			var originalOrder = await GetOrder(originalOrderId);

			if (originalOrder?.Status == OrderStatus.OrderComplete)
			{
				OrderCompleted orderCompletedEvent = new(order.Username, originalOrder.OrderId);
				await _dapr.PublishEventAsync(_pubsubName, _orderCompletedTopic, orderCompletedEvent, token);
			}
		});
	}
}