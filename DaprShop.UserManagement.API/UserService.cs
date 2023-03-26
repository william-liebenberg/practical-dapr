using Dapr;
using Dapr.Client;

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

    public async Task<bool> UserExists(string username)
    {
        var existingUser = await GetUser(username);
        return existingUser != null;
    }

    public async Task<User?> GetUser(string username)
    {
        try
        {
            User? user = await _dapr.GetStateAsync<User?>(_storeName, username);
            return user;
        }
        catch (DaprException dx)
        {
            return null;
        }
    }

    public async Task AddUser(User newUser)
    {
        if (await UserExists(newUser.Username))
        {
            _logger.LogError("Cannot add duplicate user: {username}", newUser.Username);
            throw new UserAlreadyExistsException(newUser.Username);
        }

        // save the user to the store
        await _dapr.SaveStateAsync(_storeName, newUser.Username, newUser);

        return;
    }
}