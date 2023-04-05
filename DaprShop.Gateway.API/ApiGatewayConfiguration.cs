using Yarp.ReverseProxy.Configuration;

public record ApiRouteConfig(string RouteName, string ClusterName, string RoutePrefix, string HostUrl, string SwaggerJsonUrl);

public static class ApiGatewayConfiguration
{
    public static IReverseProxyBuilder AddApiGatewayConfiguration(this IReverseProxyBuilder proxyBuilder, IConfiguration config)
    {
        IConfigurationSection apiRoutesSection = config.GetSection("ApiRoutes");
        ApiRouteConfig[]? routes = apiRoutesSection.Get<ApiRouteConfig[]>();

        List<RouteConfig> apiRoutes = new();
        List<ClusterConfig> apiClusters = new();

        routes?.ToList().ForEach(r =>
        {
            Console.WriteLine($"Adding Api Route: {r.RouteName} -> {r.ClusterName} -> {r.RoutePrefix} -> {r.HostUrl}");

            apiRoutes.Add(new RouteConfig()
            {
                RouteId = r.RouteName,
                ClusterId = r.ClusterName,
                Match = new RouteMatch()
                {
                    Path = $"/{r.RoutePrefix}/{{*any}}"
                }
            });
            apiClusters.Add(new ClusterConfig()
            {
                ClusterId = r.ClusterName,
                Destinations = new Dictionary<string, DestinationConfig>
                {
                    { "service", new DestinationConfig { Address = r.HostUrl } }
                }
            });
        });
        
        proxyBuilder.LoadFromMemory(
            apiRoutes.ToList(),
            apiClusters.ToList());

        return proxyBuilder;
    }
}