using Dapr;
using Dapr.Client;

using DaprShop.ShoppingCart.API.Domain;

namespace DaprShop.ShoppingCart.API.Services;

public interface IShoppingCartService
{
    Task<Domain.ShoppingCart> GetShoppingCart(string userId);
    Task AddItemToShoppingCart(string userId, ShoppingCartItem item);
}

public class ShoppingCartService : IShoppingCartService
{
    private readonly ILogger<ShoppingCartService> _logger;
    private readonly DaprClient _dapr;

    private readonly string _storeName = "daprshop-statestore";
    private readonly string _pubsubName = "daprshop-pubsub";
    private readonly string _ordersQueueTopic = "daprshop.orders";

    public ShoppingCartService(ILogger<ShoppingCartService> logger, DaprClient dapr)
    {
        _logger = logger;
        _dapr = dapr;
    }

    public async Task AddItemToShoppingCart(string userId, ShoppingCartItem item)
    {
        Domain.ShoppingCart shoppingCart = await GetShoppingCart(userId);
        ShoppingCartItem? existingItem = shoppingCart.Items.Where(x => x.ProductId == item.ProductId).FirstOrDefault();
        if(existingItem != null)
        {
            existingItem = existingItem with { Quantity = existingItem.Quantity + item.Quantity };
        }
        else
        {
            shoppingCart.Items.Add(item);
        }

        try
        {
            // save/update the shopping cart
            await _dapr.SaveStateAsync(_storeName, userId, shoppingCart, new StateOptions()
            {
                Concurrency = ConcurrencyMode.LastWrite
            });
        }
        catch (DaprException dx)
        {
            _logger.LogError(dx, "Couldn't save cart state for user: {userid}", userId);
            throw;
        }

        var itemAddedToShoppingCartEvent = new DaprShop.Contracts.Events.ProductItemAddedToShoppingCart()
        {
            UserId = userId,
            ProductId = item.ProductId ?? string.Empty
        };

        try
        {
            // publish the event
            await _dapr.PublishEventAsync(_pubsubName, _ordersQueueTopic, itemAddedToShoppingCartEvent);
        }
        catch (DaprException dx)
        {
            _logger.LogError(dx, "Couldn't publish event for product: {productId}", itemAddedToShoppingCartEvent.ProductId);
            throw;
        }
    }

    public async Task<Domain.ShoppingCart> GetShoppingCart(string userId)
    {
        try
        {
            var shoppingCart = await _dapr.GetStateAsync<Domain.ShoppingCart>(_storeName, userId);
            if (shoppingCart == null)
            {
                shoppingCart = new Domain.ShoppingCart()
                {
                    UserId = userId
                };
            }
            return shoppingCart;
        }
        catch (DaprException dx)
        {
            _logger.LogError(dx, "Could not get cart state for user: {userid}", userId);
            throw;
        }
    }

    // sumbit an order by collecting everything in the basket, creating an order item, and publishing it onto the bus
    public async Task<Domain.Order> Submit(string userId)
    {
        var cart = await GetShoppingCart(userId);

        // TODO: Refine Order creation - too much in here
        var order = new Order
            (
            Guid.NewGuid().ToString(),
            userId,
            "my order",
            cart.Items.Sum(i => i.Price),
            cart.Items.Where(i => i.ProductId != null && i.ProductName != null)
                .Select(i => new OrderItem(1, new Product(i.ProductId!, i.ProductName!, "DESCRIPTION", i.Price, "IMAGEURL"))).ToArray(),
            OrderStatus.OrderNew
            );

        await ClearShoppingCart(userId);

        await _dapr.PublishEventAsync<Order>(_pubsubName, _ordersQueueTopic, order);

        return order;
    }

    public async Task ClearShoppingCart(string userId)
    {
        try
        {
            await _dapr.DeleteStateAsync(_storeName, userId);
        }
        catch (DaprException dx)
        {
            _logger.LogError(dx, "Could clear the cart for user: {userid}", userId);
            throw;
        }
    }
}


