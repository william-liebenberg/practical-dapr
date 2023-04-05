using Dapr;
using Dapr.Client;

using DaprShop.Contracts.Entities;

namespace DaprShop.UserManagement.API;

public class UserService
{
    private readonly ILogger<UserService> _logger;
    private readonly DaprClient _dapr;
    private readonly string _storeName = "daprshop-statestore";

    public UserService(ILogger<UserService> logger, DaprClient dapr)
    {
        _logger = logger;
        _dapr = dapr;
    }

    public async Task<bool> UserExists(string userId)
    {
        var existingUser = await GetUser(userId);
        return existingUser != null;
    }

    public async Task<User?> GetUser(string userId)
    {
        try
        {
            User? user = await _dapr.GetStateAsync<User?>(_storeName, userId);
            return user;
        }
        catch (DaprException dx)
        {
            _logger.LogError(dx, "Can't get state for user: {userId}", userId);
            throw;
        }
    }

    public async Task AddUser(User newUser)
    {
        if (await UserExists(newUser.UserId))
        {
            _logger.LogError("Cannot add duplicate user: {userId}", newUser.UserId);
            throw new UserAlreadyExistsException(newUser.UserId);
        }

        try
        {
            // save the user to the store
            await _dapr.SaveStateAsync(_storeName, newUser.UserId, newUser);
        }
        catch (DaprException dx)
        {
            _logger.LogError(dx, "Can't save state for user: {userId}", newUser.UserId);
            throw;
        }

        return;
    }
}