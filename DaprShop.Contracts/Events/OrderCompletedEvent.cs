namespace DaprShop.Contracts.Events;

public record OrderCompletedEvent(string? Username, string? OrderId);