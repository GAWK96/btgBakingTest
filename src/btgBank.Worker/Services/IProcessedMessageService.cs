namespace btgBank.Worker.Services;

public interface IProcessedMessageService
{
	Task<bool> TryRegisterAsync(
		Guid messageId,
		CancellationToken cancellationToken = default);
}