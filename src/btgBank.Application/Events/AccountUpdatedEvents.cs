namespace btgBank.Application.Events;

public record AccountUpdatedEvent(
	Guid AccountId,
	string HolderName,
	bool IsActive
);