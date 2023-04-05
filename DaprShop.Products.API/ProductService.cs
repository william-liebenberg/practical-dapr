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
        string query = "";
        try
        {
            StateQueryResponse<Product> results = await _dapr.QueryStateAsync<Product>(_storeName, query);
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

    // Query (just check name / description)
    public async Task<IEnumerable<Product>> Query(string searchTerm)
    {
        string query = "";
        try
        {
            StateQueryResponse<Product> results = await _dapr.QueryStateAsync<Product>(_storeName, query);
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

    // All
    public async Task<IEnumerable<Product>> ListAll()
    {
        string query = "";
        try
        {
            StateQueryResponse<Product> results = await _dapr.QueryStateAsync<Product>(_storeName, query);
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
            var p1 = new Product("1", "", "", 10, "");
            var p2 = new Product("2", "", "", 10, "");

            await _dapr.SaveStateAsync(_storeName, "product_1", p1);
            await _dapr.SaveStateAsync(_storeName, "product_2", p2);
        }
        catch (DaprException dx)
        {
            _logger.LogError(dx, "Could not perform product seed");
            throw;
        }
    }
}