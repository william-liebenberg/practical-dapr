namespace DaprShop.Contracts.Entities;

public class Cart
{
	public string Username { get; set; } = string.Empty;
	public List<CartItem> Items { get; set; } = new();
}

public static class CartExtensions
{
	public static CartItem? FindCartItem(this Cart cart, string productId)
	{
		return cart.Items?.Where(x => x.ProductId == productId).FirstOrDefault();
	}

	public static void AddItem(this Cart cart, CartItem newItem)
	{
		cart.Items.Add(newItem);
	}

	public static bool RemoveCartItem(this Cart cart, string productId)
	{
		CartItem? existingItem = cart.FindCartItem(productId);
		return existingItem != null && cart.Items.Remove(existingItem);
	}

	public static bool AdjustCartItem(this Cart cart, Product product, int quantity)
	{
		CartItem? existingItem = cart.FindCartItem(product.ProductId);
		if (existingItem is null)
		{
			cart.AddItem(new CartItem(product.ProductId, product.Name, product.UnitPrice, quantity));
			return true;
		}

		var adjustedItem = existingItem with
		{
			Quantity = existingItem.Quantity + quantity
		};

		if (adjustedItem.Quantity <= 0)
		{
			cart.Items.Remove(existingItem);
		}
		else
		{
			cart.Items.Remove(existingItem);
			cart.Items.Add(adjustedItem);
		}
		
		return true;
	}

	public static bool IsEmpty(this Cart cart)
	{
		if (cart.Items is not null)
		{
			if (cart.Items.Count > 0)
			{
				return false;
			}
		}
		return true;
	}
}