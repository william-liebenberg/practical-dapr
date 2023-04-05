using Dapr;
using Dapr.Client;

using DaprShop.Contracts.Entities;

namespace DaprShop.Shopping.API.Services;

public class CartService
{
    private readonly ILogger<CartService> _logger;
    private readonly DaprClient _dapr;

    private readonly string _storeName = "daprshop-statestore";
    private readonly string _pubsubName = "daprshop-pubsub";
    private readonly string _ordersQueueTopic = "daprshop.orders";

    public CartService(ILogger<CartService> logger, DaprClient dapr)
    {
        _logger = logger;
        _dapr = dapr;
    }

    public async Task AddItemToShoppingCart(string userId, string productId, int quantity)
    {
        // first check if product is valid (and in stock) by calling product service to get details of Product (via productId)
        HttpRequestMessage productRequest = _dapr.CreateInvokeMethodRequest(HttpMethod.Get, "products-api", $"/products/get?productId={productId}", string.Empty);
        var product = await _dapr.TryInvokeMethodAsync<Product>(productRequest);
        if (product == null)
        {
            _logger.LogInformation("Could not retrieve product with Id {productId} from Product Service", productId);
            throw new ProductNotFoundException(productId);
        }
        else
        {
            _logger.LogInformation("Product with Id {productId} is valid!", product.ProductId);
        }

        // check if the user is valid (registered) by calling the user service to get the details of the User (via userId)
        HttpRequestMessage userRequest = _dapr.CreateInvokeMethodRequest(HttpMethod.Get, "users-api", $"/users/get?userId={userId}", string.Empty);
        var user = await _dapr.TryInvokeMethodAsync<User>(userRequest);
        if (user == null)
        {
            _logger.LogInformation("Could not retrieve user with Id {userId} from User Service", userId);
            throw new ProductNotFoundException(productId);
        }
        else
        {
            _logger.LogInformation("User with Id {userId} is valid!", user.UserId);
        }

        Cart cart = await GetCart(userId);
        CartItem? existingItem = cart.Items.Where(x => x.ProductId == productId).FirstOrDefault();
        if(existingItem != null)
        {
            existingItem = existingItem with { Quantity = existingItem.Quantity + quantity };
        }
        else
        {
            cart.Items.Add(new CartItem(product.ProductId, product.Name, product.UnitPrice, quantity));
        }

        try
        {
            // save/update the cart
            await _dapr.SaveStateAsync(_storeName, userId, cart);
        }
        catch (DaprException dx)
        {
            _logger.LogError(dx, "Couldn't save cart state for user: {userid}", userId);
            throw;
        }

        var itemAddedToCartEvent = new Contracts.Events.ProductItemAddedToCart()
        {
            UserId = userId,
            ProductId = productId
        };

        try
        {
            // publish the event
            await _dapr.PublishEventAsync(_pubsubName, _ordersQueueTopic, itemAddedToCartEvent);
        }
        catch (DaprException dx)
        {
            _logger.LogError(dx, "Couldn't publish event for product: {productId}", itemAddedToCartEvent.ProductId);
            throw;
        }
    }

    public async Task<Cart> GetCart(string userId)
    {
        try
        {
            var cart = await _dapr.GetStateAsync<Cart>(_storeName, userId);
            if (cart == null)
            {
                cart = new Cart()
                {
                    UserId = userId
                };
            }
            return cart;
        }
        catch (DaprException dx)
        {
            _logger.LogError(dx, "Could not get cart state for user: {userid}", userId);
            throw;
        }
    }

    // sumbit an order by collecting everything in the basket, creating an order item, and publishing it onto the bus
    public async Task<Order> Submit(string userId)
    {
        var cart = await GetCart(userId);

        // TODO: Refine Order creation - too much in here
        var order = new Order
            (
            Status:         OrderStatus.OrderNew,
            OrderId:        Guid.NewGuid().ToString(),
            CustomerId:     userId,
            Title:          $"Order {DateTime.Now.ToShortDateString()}",
            TotalAmount:    cart.Items.Sum(i => i.Price),
            Items:          cart.Items.Where(i => i.ProductId != null && i.ProductName != null)
                                      .Select(i => new OrderItem(
                                          1, 
                                          new Product(
                                              i.ProductId!, 
                                              i.ProductName!, 
                                              "DESCRIPTION", 
                                              i.Price, 
                                              "IMAGEURL"))).ToArray()
            );

        await ClearCart(userId);

        await _dapr.PublishEventAsync<Order>(_pubsubName, _ordersQueueTopic, order);

        return order;
    }

    public async Task ClearCart(string userId)
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


