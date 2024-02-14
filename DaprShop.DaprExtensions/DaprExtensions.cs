using Dapr;
using Dapr.Client;

namespace DaprShop.DaprExtensions;

public static class DaprExtensions
{
	public static async Task<TResponse?> TryInvokeMethodAsync<TResponse>(this DaprClient dapr, HttpRequestMessage request, CancellationToken cancellationToken = default)
	{
		try
		{
			TResponse resp = await dapr.InvokeMethodAsync<TResponse>(request, cancellationToken);
			return resp;
		}
		catch (DaprException dx)
		{

			Console.WriteLine(dx.Message);
		}

		return default;
	}
}
