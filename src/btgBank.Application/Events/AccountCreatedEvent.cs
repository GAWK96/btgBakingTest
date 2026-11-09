namespace btgBank.Application.Events;

public record AccountCreatedEvent(
	Guid AccountId,
	string HolderName
);