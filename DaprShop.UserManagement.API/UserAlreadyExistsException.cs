using System.Runtime.Serialization;

namespace DaprShop.UserManagement.API;

[Serializable]
class UserAlreadyExistsException : Exception
{
	public UserAlreadyExistsException()
	{
	}

	public UserAlreadyExistsException(string? message) : base(message)
	{
	}

	public UserAlreadyExistsException(string? message, Exception? innerException) : base(message, innerException)
	{
	}
}