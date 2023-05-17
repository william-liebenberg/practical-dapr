using Dapr.Client;

using DaprShop.Contracts.Entities;
using DaprShop.Contracts.Events;

using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace DaprShop.Shopping.API.Services;

public class CartService
{
	private readonly ILogger<CartService> _logger;
	private readonly DaprClient _dapr;

	private readonly string _storeName = "daprshop-statestore";
	private readonly string _pubsubName = "daprshop-pubsub";
	private readonly string _cartTopic = "daprshop.cart.items";
	private readonly string _ordersQueueTopic = "daprshop.orders.queue";

	public CartService(ILogger<CartService> logger, DaprClient dapr)
	{
		_logger = logger;
		_dapr = dapr;
	}

	public async Task AddItemToShoppingCart(string username, string productId, int quantity)
	{
		// first check if product is valid (and in stock) by calling product service to get details of Product (via productId)
		var productsHttpClient = DaprClient.CreateInvokeHttpClient("products-api");
		var product = await productsHttpClient.GetFromJsonAsync<Product>($"products/get?productId={productId}");
		if (product == null)
		{
			_logger.LogInformation("Could not retrieve product with Id {productId} from Product Service", productId);
			throw new ProductNotFoundException(productId);
		}
		else
		{
			_logger.LogInformation("Product with Id {productId} is valid!", product.ProductId);
		}

		// check if the user is valid (registered) by calling the user service to get the details of the User (via username)
		var usersHttpClient = DaprClient.CreateInvokeHttpClient("users-api");
		var user = await usersHttpClient.GetFromJsonAsync<User>($"users/get?username={username}");
		if (user == null)
		{
			_logger.LogInformation("Could not retrieve user {username} from User Service", username);
			throw new Exception($"User not found: {username}");
		}
		else
		{
			_logger.LogInformation("User {username} is valid!", user.Username);
		}

		Cart cart = await GetCart(username);
		CartItem? existingItem = cart.FindCartItem(productId);
		if (existingItem != null)
		{
			var newItem = existingItem with { Quantity = existingItem.Quantity + quantity };
			if (cart.RemoveCartItem(productId))
			{
				cart.AddItem(newItem);
			}
		}
		else
		{
			cart.AddItem(new CartItem(product.ProductId, product.Name, product.UnitPrice, quantity));
		}

		
		// save/update the cart
		await _dapr.SaveStateAsync(_storeName, username, cart);

		// publish the event
		ProductItemAddedToCart itemAddedToCartEvent = new()
		{
			Username = username,
			ProductId = productId
		};

		await _dapr.PublishEventAsync(_pubsubName, _cartTopic, itemAddedToCartEvent);
	}

	public async Task<bool> RemoveItemFromShoppingCart(string username, string productId, int quantity)
	{
		Cart cart = await GetCart(username);
		if (cart.RemoveCartItem(productId))
		{
			var itemRemoved = new ProductItemRemovedFromCart() { ProductId = productId };
			await _dapr.PublishEventAsync(_pubsubName, _cartTopic, itemRemoved);
			return true;
		}
		return false;
	}

	public async Task<Cart> GetCart(string username)
	{
		var cart = await _dapr.GetStateAsync<Cart>(_storeName, username);
		cart ??= new Cart()
		{
			Username = username
		};
		return cart;
	}

	// submit an order by collecting everything in the basket, creating an order item, and publishing it onto the bus
	public async Task<Order?> SubmitNewOrder(string username)
	{
		Cart? cart = await GetCart(username);
		if (cart is null || cart.IsEmpty())
		{
			_logger.LogInformation("User {username} tried to submit an empty cart!", username);
			return null;
		}

		var order = new Order
			(
				Status: OrderStatus.OrderNew,
				OrderId: Guid.NewGuid().ToString(),
				Username: username,
				Title: $"Order {DateTime.Now.ToShortDateString()}",
				TotalAmount: cart.Items.Sum(i => i.Price),
				Items: cart.Items.Select(i => new OrderItem(1, i.ProductId))
										  .ToArray()
			);

		// put the new order into the queue
		await _dapr.PublishEventAsync(_pubsubName, _ordersQueueTopic, order);

		// clear the users cart (so they can fill it up again)
		await ClearCart(username);

		return order;
	}

	public async Task ClearCart(string username)
	{
		await _dapr.DeleteStateAsync(_storeName, username);
		await _dapr.PublishEventAsync(_pubsubName, _cartTopic, new CartCleared() { Username = username });
	}
}