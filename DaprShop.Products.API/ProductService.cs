using Dapr;
using Dapr.Client;

using DaprShop.Contracts.Entities;

namespace DaprShop.Products.API;

public class ProductService
{
	private readonly ILogger<ProductService> _logger;
	private readonly DaprClient _dapr;
	private readonly string _storeName = "daprshop-statestore";

	public ProductService(ILogger<ProductService> logger, DaprClient dapr)
	{
		_logger = logger;
		_dapr = dapr;
	}

	public async Task<Product> GetProduct(string productId)
	{
		try
		{
			return await _dapr.GetStateAsync<Product>(_storeName, productId);
		}
		catch (DaprException dx)
		{
			_logger.LogError(dx, "Could not perform product search");
			throw;
		}
	}

	// Search
	public async Task<IEnumerable<Product>> Search(string field, string searchTerm)
	{
		//if(!string.Equals(field, "unitprice",  StringComparison.OrdinalIgnoreCase))
		//{
		//	searchTerm = $"\"{searchTerm}\"";
		//}

		string query = $$"""
			{
				"filter": {
					"EQ": { "{{field}}": {{searchTerm}} }
				}
			}
			""";

		try
		{
			StateQueryResponse<Product> results = await _dapr.QueryStateAsync<Product>(_storeName, query, cancellationToken: CancellationToken.None);
			return results.Results.Select(i => i.Data);
		}
		catch (DaprException dx)
		{
			_logger.LogError(dx, "Could not perform product search");
			throw;
		}
	}

	// Query (just check name / description)
	public async Task<IEnumerable<Product>> Query(string searchTerm)
	{
		string query = $$"""
			{
				"filter": {
					"OR": [
						{
							"EQ": { "name": "{{searchTerm}}" }
						},
						{
							"EQ": { "description": "{{searchTerm}}" }
						}
					]
				}
			}
			""";

		try
		{
			StateQueryResponse<Product> results = await _dapr.QueryStateAsync<Product>(_storeName, query, cancellationToken: CancellationToken.None);
			return results.Results.Select(i => i.Data);
		}
		catch (DaprException dx)
		{
			_logger.LogError(dx, "Could not perform product search");
			throw;
		}
	}

	// All
	public async Task<IEnumerable<Product>> ListAll()
	{
		string query = "{}";
		
		try
		{
			StateQueryResponse<Product> results = await _dapr.QueryStateAsync<Product>(_storeName, query, cancellationToken: CancellationToken.None);
			if (!string.IsNullOrWhiteSpace(results.Token))
			{
			}

			return results.Results.Select(i => i.Data);
		}
		catch (DaprException dx)
		{
			_logger.LogError(dx, "Could not perform product search");
			throw;
		}
	}

	public async Task Seed()
	{
		try
		{
			var p1 = new Product("1", "Bread", "Fluffy freshly baked bread", 5, "");
			var p2 = new Product("2", "Cheese", "Kraft Tastey Cheese block", 10, "");
			var p3 = new Product("3", "Soy Milk", "Soy milk", 5, "");
			var p4 = new Product("4", "Coffee", "Nescafe Freeze dried coffee", 5, "");

			await _dapr.SaveStateAsync(_storeName, p1.ProductId, p1);
			await _dapr.SaveStateAsync(_storeName, p2.ProductId, p2);
			await _dapr.SaveStateAsync(_storeName, p3.ProductId, p3);
			await _dapr.SaveStateAsync(_storeName, p4.ProductId, p4);
		}
		catch (DaprException dx)
		{
			_logger.LogError(dx, "Could not perform product seed");
			throw;
		}
	}
}