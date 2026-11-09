namespace btgBank.Application.Events;

public record AccountDeletedEvent(
	Guid AccountId
);