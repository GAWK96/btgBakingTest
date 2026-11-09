using btgBank.Application.Interfaces;
using MassTransit;

namespace btgBank.Infrastructure.Messaging;

public class MassTransitEventPublisher : IEventPublisher
{
	private readonly IPublishEndpoint _publishEndpoint;

	public MassTransitEventPublisher(
		IPublishEndpoint publishEndpoint)
	{
		_publishEndpoint = publishEndpoint;
	}

	public Task PublishAsync<T>(
		T message,
		CancellationToken cancellationToken = default)
		where T : class
	{
		return _publishEndpoint.Publish(
			message,
			cancellationToken);
	}
}