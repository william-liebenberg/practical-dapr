using System.Runtime.Serialization;

namespace DaprShop.Shopping.API.Services
{
	[Serializable]
	internal class ProductNotFoundException : Exception
	{
		public ProductNotFoundException()
		{
		}

		public ProductNotFoundException(string? message) : base(message)
		{
		}

		public ProductNotFoundException(string? message, Exception? innerException) : base(message, innerException)
		{
		}
	}
}