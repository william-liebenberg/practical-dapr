namespace DaprShop.Notifications.API;

public record EmailModel(string From, string To, string Subject, string CC, string BCC, string Body);

public class EmailService
{
	private readonly string _sendMailBinding = "sendgrid";
	private readonly string _createBindingOperation = "create";
	private readonly Dapr.Client.DaprClient _dapr;

	public EmailService(Dapr.Client.DaprClient dapr)
	{
		_dapr = dapr;
	}

	public async Task SendEmail(EmailModel email)
	{
		var emailMetadata = new Dictionary<string, string>
		{
			["emailFrom"] = email.From,
			["emailTo"] = email.To,
			["subject"] = email.Subject
		};

		if (!string.IsNullOrEmpty(email.CC))
		{
			emailMetadata["emailCc"] = email.CC;
		}

		if (!string.IsNullOrEmpty(email.BCC))
		{
			emailMetadata["emailBcc"] = email.BCC;
		}

		await _dapr.InvokeBindingAsync(
		 _sendMailBinding,
		 _createBindingOperation,
		 email.Body,
		 emailMetadata
		);
	}
}
