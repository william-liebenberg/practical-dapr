namespace DaprShop.UserManagement.API; 

public record RegisterUserRequest(string Username, string Email, string DisplayName, string ProfileImageUrl);