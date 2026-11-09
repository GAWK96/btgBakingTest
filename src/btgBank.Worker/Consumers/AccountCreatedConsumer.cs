using btgBank.Application.Events;
using btgBank.Worker.Services;
using MassTransit;

namespace btgBank.Worker.Consumers;

public class AccountEventConsumer :
	IConsumer<AccountCreatedEvent>,
	IConsumer<AccountUpdatedEvent>,
	IConsumer<AccountDeletedEvent>
{
	private readonly ILogger<AccountEventConsumer> _logger;
	private readonly IProcessedMessageService _processedMessageService;

	public AccountEventConsumer(
		ILogger<AccountEventConsumer> logger,
		IProcessedMessageService processedMessageService)
	{
		_logger = logger;
		_processedMessageService = processedMessageService;
	}

	public async Task Consume(
		ConsumeContext<AccountCreatedEvent> context)
	{
		if (!await RegisterMessageAsync(context))
			return;

		_logger.LogInformation(
			"Account created event processed. AccountId: {AccountId}",
			context.Message.AccountId);
	}

	public async Task Consume(
		ConsumeContext<AccountUpdatedEvent> context)
	{
		if (!await RegisterMessageAsync(context))
			return;

		_logger.LogInformation(
			"Account updated event processed. AccountId: {AccountId}",
			context.Message.AccountId);
	}

	public async Task Consume(
		ConsumeContext<AccountDeletedEvent> context)
	{
		if (!await RegisterMessageAsync(context))
			return;

		_logger.LogInformation(
			"Account deleted event processed. AccountId: {AccountId}",
			context.Message.AccountId);
	}

	private async Task<bool> RegisterMessageAsync<T>(
		ConsumeContext<T> context) where T : class
	{
		if (!context.MessageId.HasValue)
		{
			throw new InvalidOperationException(
				"MessageId is required.");
		}

		var messageId = context.MessageId.Value;

		var registered = await _processedMessageService.TryRegisterAsync(
			messageId,
			context.CancellationToken);

		if (!registered)
		{
			_logger.LogInformation(
				"Message {MessageId} already processed. Skipping.",
				messageId);
		}

		return registered;
	}
}