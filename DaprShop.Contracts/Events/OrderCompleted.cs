namespace DaprShop.Contracts.Events;

public record OrderCompleted(string? Username, string? OrderId);