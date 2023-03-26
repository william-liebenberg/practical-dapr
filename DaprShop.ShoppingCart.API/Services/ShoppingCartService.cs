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

    private readonly string _storeName = "shopStore";
    private readonly string _pubsubName = "daprshop-pubsub";
    private readonly string _shoppingCartItemsTopic = "daprshop.shoppingcart.items";

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
            await _dapr.PublishEventAsync(_pubsubName, _shoppingCartItemsTopic, itemAddedToShoppingCartEvent);
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
}


