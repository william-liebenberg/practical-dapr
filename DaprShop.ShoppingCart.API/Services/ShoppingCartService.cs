using System.Data.SqlTypes;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;

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

    private readonly string _storeName = "shopStore";
    private readonly string _pubsubName = "daprshop-pubsub";
    private readonly string _shoppingCartItemsTopic = "daprshop.shoppingcart.items";

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
            existingItem = existingItem with { Quantity = existingItem.Quantity + item.Quantity };
        }
        else
        {
            shoppingCart.Items.Add(item);
        }

        // save/update the shopping cart
        await _dapr.SaveStateAsync(_storeName, userId, shoppingCart, new StateOptions(){
            Concurrency = ConcurrencyMode.LastWrite
        });

        // publish the event
        var itemAddedToShoppingCartEvent = new DaprShop.Contracts.Events.ProductItemAddedToShoppingCart()
        {
            UserId = userId,
            ProductId = item.ProductId ?? string.Empty
        };

        await _dapr.PublishEventAsync(_pubsubName, _shoppingCartItemsTopic, itemAddedToShoppingCartEvent);
    }

    public async Task<Domain.ShoppingCart> GetShoppingCart(string userId)
    {
        //var ss = await _dapr.GetStateEntryAsync<Domain.ShoppingCart>(_storeName, userId);
        //await ss.SaveAsync();

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


