using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace DaprShop.Notifications.API;

public class AppInsightsTelemetryInitializer : ITelemetryInitializer
{
	private readonly string _preferredRolename;

	public AppInsightsTelemetryInitializer(string preferredRolename)
	{
		_preferredRolename = preferredRolename;
	}

	public void Initialize(ITelemetry telemetry)
	{
		if (string.IsNullOrEmpty(telemetry.Context.Cloud.RoleName))
		{
			telemetry.Context.Cloud.RoleName = _preferredRolename;
		}
	}
}