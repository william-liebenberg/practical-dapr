namespace DaprShop.XsafeprojectnameX.API;

public class MyService
{
	private readonly ILogger<MyService> _logger;

	public MyService(ILogger<MyService> logger)
    {
		_logger = logger;
	}

    public async Task<string?> Execute(string? message)
    {
        var arr = message?.ToCharArray() ?? null;
        if(arr is not null)
        {
            Array.Reverse(arr);
        }
        string? result = new string(arr);
        _logger.LogInformation("Transformed message \"{input}\" into \"{output}\"", message, result);
        return await Task.FromResult<string?>(result);
    }
}
