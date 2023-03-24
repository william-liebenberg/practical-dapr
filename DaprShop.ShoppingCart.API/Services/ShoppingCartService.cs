using Dapr.Client;

using DaprShop.Contracts;
using DaprShop.ShoppingCart.API.Domain;

namespace DaprShop.ShoppingCart.API.Services;

public interface IShoppingCartService
{
    Task<Domain.ShoppingCart> GetShoppingCart(string userId);
    Task AddItemToShoppingCart(string userId, ShoppingCartItem item);
}

public class ShoppingCartService : IShoppingCartService
{
    private readonly DaprClient _dapr;
    private readonly string _storeName = "shoppingCartStore";

    public ShoppingCartService(DaprClient dapr)
    {
        _dapr = dapr;
    }

    public async Task AddItemToShoppingCart(string userId, ShoppingCartItem item)
    {
        Domain.ShoppingCart shoppingCart = await GetShoppingCart(userId);
        ShoppingCartItem? existingItem = shoppingCart.Items.Where(x => x.ProductId == item.ProductId).FirstOrDefault();
        if(existingItem != null)
        {
            existingItem.Quantity += item.Quantity;
        }
        else
        {
            shoppingCart.Items.Add(item);
        }

        // save/update the shopping cart
        await _dapr.SaveStateAsync(_storeName, userId, shoppingCart);

        // publish the event
        var itemAddedToShoppingCartEvent = new ProductItemAddedToShoppingCartEvent()
        {
            UserId = userId,
            ProductId = item.ProductId ?? string.Empty
        };

        const string pubsubName = "pubsub";
        const string topicNameOfShoppingCartItems = "daprshop.shoppingart.items";
        await _dapr.PublishEventAsync(pubsubName, topicNameOfShoppingCartItems, itemAddedToShoppingCartEvent);
    }

    public async Task<Domain.ShoppingCart> GetShoppingCart(string userId)
    {
        var shoppingCart = await _dapr.GetStateAsync<Domain.ShoppingCart>(_storeName, userId);
        if(shoppingCart == null)
        {
            shoppingCart = new Domain.ShoppingCart()
            {
                UserId = userId
            };
        }
        return shoppingCart;
    }
}