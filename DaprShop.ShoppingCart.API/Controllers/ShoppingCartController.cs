using DaprShop.ShoppingCart.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace DaprShop.ShoppingCart.API.Controllers;

[ApiController]
[Route("[controller]")]
public class ShoppingCartController : ControllerBase
{
    private readonly IShoppingCartService _shoppingCartService;

    public ShoppingCartController(IShoppingCartService shoppingCartService)
    {
        _shoppingCartService = shoppingCartService;
    }

    [HttpGet("{userId}")]
    public async Task<ActionResult<Domain.ShoppingCart>> Get(string userId)
    {
        try
        {
            Domain.ShoppingCart shoppingCart = await _shoppingCartService.GetShoppingCart(userId);
            return Ok(shoppingCart);
        }
        catch(Exception ex)
        {
            return BadRequest(ex);
        }
    }

    [HttpPost("{userId}/items")]
    public async Task<ActionResult<Domain.ShoppingCart>> Post(string userId, [FromBody]Contracts.Requests.AddProductItemToCart item)
    {
        Domain.ShoppingCartItem mappedItem = new(
            item.ProductId,
            item.ProductName,
            item.Price,
            item.Quantity);

        try
        {
            await _shoppingCartService.AddItemToShoppingCart(userId, mappedItem);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }

    [HttpPost("{userId}/submit")]
    public async Task<ActionResult<Domain.SubmittedOrder>> Submit(string userId)
    {
        try
        {
            var order = await _shoppingCartService.Submit(userId);
            return Ok(order);
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }
}
