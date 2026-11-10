using btgBank.Application.Events;
using btgBank.Worker.Consumers;
using btgBank.Worker.Services;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace btgBank.UnitTests.Worker.Consumers;

public class AccountEventConsumerTests
{
	private readonly Mock<IProcessedMessageService> _processedMessageServiceMock;
	private readonly Mock<ILogger<AccountEventConsumer>> _loggerMock;
	private readonly AccountEventConsumer _consumer;

	public AccountEventConsumerTests()
	{
		_processedMessageServiceMock = new Mock<IProcessedMessageService>();
		_loggerMock = new Mock<ILogger<AccountEventConsumer>>();

		_consumer = new AccountEventConsumer(
			_loggerMock.Object,
			_processedMessageServiceMock.Object);
	}

	private static Mock<ConsumeContext<T>> CreateContext<T>(
		Guid? messageId,
		T message) where T : class
	{
		var contextMock = new Mock<ConsumeContext<T>>();

		contextMock
			.Setup(x => x.MessageId)
			.Returns(messageId);

		contextMock
			.Setup(x => x.Message)
			.Returns(message);

		contextMock
			.Setup(x => x.CancellationToken)
			.Returns(CancellationToken.None);

		return contextMock;
	}

	private void SetupRegistration(Guid messageId, bool registered)
	{
		_processedMessageServiceMock
			.Setup(x => x.TryRegisterAsync(
				messageId,
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(registered);
	}

	private void SetupRegistrationFailure(Guid messageId)
	{
		_processedMessageServiceMock
			.Setup(x => x.TryRegisterAsync(
				messageId,
				It.IsAny<CancellationToken>()))
			.ThrowsAsync(
				new InvalidOperationException("Database unavailable"));
	}

	private void VerifyRegistration(Guid messageId, Times times)
	{
		_processedMessageServiceMock.Verify(
			x => x.TryRegisterAsync(
				messageId,
				It.IsAny<CancellationToken>()),
			times);
	}

	private void VerifyNoRegistration()
	{
		_processedMessageServiceMock.Verify(
			x => x.TryRegisterAsync(
				It.IsAny<Guid>(),
				It.IsAny<CancellationToken>()),
			Times.Never);
	}

	//ACCOUNT CREATED EVENT

	[Fact]
	public async Task Consume_CreatedEvent_ShouldRegisterMessage()
	{
		// Arrange
		var messageId = Guid.NewGuid();
		var accountId = Guid.NewGuid();

		var message = new AccountCreatedEvent(
			accountId,
			"John Doe");

		var context = CreateContext(messageId, message);

		SetupRegistration(messageId, true);

		// Act
		await _consumer.Consume(context.Object);

		// Assert
		VerifyRegistration(messageId, Times.Once());
	}

	[Fact]
	public async Task Consume_CreatedEvent_WhenDuplicate_ShouldNotThrow()
	{
		// Arrange
		var messageId = Guid.NewGuid();

		var message = new AccountCreatedEvent(
			Guid.NewGuid(),
			"John Doe");

		var context = CreateContext(messageId, message);

		SetupRegistration(messageId, false);

		// Act
		var exception = await Record.ExceptionAsync(
			() => _consumer.Consume(context.Object));

		// Assert
		Assert.Null(exception);

		VerifyRegistration(messageId, Times.Once());
	}

	[Fact]
	public async Task Consume_CreatedEvent_WithoutMessageId_ShouldThrow()
	{
		// Arrange
		var message = new AccountCreatedEvent(
			Guid.NewGuid(),
			"John Doe");

		var context = CreateContext(
			(Guid?)null,
			message);

		// Act
		var exception = await Assert.ThrowsAsync<InvalidOperationException>(
			() => _consumer.Consume(context.Object));

		// Assert
		Assert.Equal("MessageId is required.", exception.Message);

		VerifyNoRegistration();
	}

	[Fact]
	public async Task Consume_CreatedEvent_WhenRegistrationFails_ShouldThrow()
	{
		// Arrange
		var messageId = Guid.NewGuid();

		var message = new AccountCreatedEvent(
			Guid.NewGuid(),
			"John Doe");

		var context = CreateContext(messageId, message);

		SetupRegistrationFailure(messageId);

		// Act
		var exception = await Assert.ThrowsAsync<InvalidOperationException>(
			() => _consumer.Consume(context.Object));

		// Assert
		Assert.Equal("Database unavailable", exception.Message);

		VerifyRegistration(messageId, Times.Once());
	}

	//ACCOUNT UPDATED EVENT

	[Fact]
	public async Task Consume_UpdatedEvent_ShouldRegisterMessage()
	{
		// Arrange
		var messageId = Guid.NewGuid();

		var message = new AccountUpdatedEvent(
			Guid.NewGuid(),
			"Jane Doe",
			true);

		var context = CreateContext(messageId, message);

		SetupRegistration(messageId, true);

		// Act
		await _consumer.Consume(context.Object);

		// Assert
		VerifyRegistration(messageId, Times.Once());
	}

	[Fact]
	public async Task Consume_UpdatedEvent_WhenDuplicate_ShouldNotThrow()
	{
		// Arrange
		var messageId = Guid.NewGuid();

		var message = new AccountUpdatedEvent(
			Guid.NewGuid(),
			"Jane Doe",
			true);

		var context = CreateContext(messageId, message);

		SetupRegistration(messageId, false);

		// Act
		var exception = await Record.ExceptionAsync(
			() => _consumer.Consume(context.Object));

		// Assert
		Assert.Null(exception);

		VerifyRegistration(messageId, Times.Once());
	}

	[Fact]
	public async Task Consume_UpdatedEvent_WithoutMessageId_ShouldThrow()
	{
		// Arrange
		var message = new AccountUpdatedEvent(
			Guid.NewGuid(),
			"Jane Doe",
			true);

		var context = CreateContext(
			(Guid?)null,
			message);

		// Act
		var exception = await Assert.ThrowsAsync<InvalidOperationException>(
			() => _consumer.Consume(context.Object));

		// Assert
		Assert.Equal("MessageId is required.", exception.Message);

		VerifyNoRegistration();
	}

	[Fact]
	public async Task Consume_UpdatedEvent_WhenRegistrationFails_ShouldThrow()
	{
		// Arrange
		var messageId = Guid.NewGuid();

		var message = new AccountUpdatedEvent(
			Guid.NewGuid(),
			"Jane Doe",
			true);

		var context = CreateContext(messageId, message);

		SetupRegistrationFailure(messageId);

		// Act
		var exception = await Assert.ThrowsAsync<InvalidOperationException>(
			() => _consumer.Consume(context.Object));

		// Assert
		Assert.Equal("Database unavailable", exception.Message);

		VerifyRegistration(messageId, Times.Once());
	}

	//ACCOUNT DELETED EVENT

	[Fact]
	public async Task Consume_DeletedEvent_ShouldRegisterMessage()
	{
		// Arrange
		var messageId = Guid.NewGuid();

		var message = new AccountDeletedEvent(
			Guid.NewGuid());

		var context = CreateContext(messageId, message);

		SetupRegistration(messageId, true);

		// Act
		await _consumer.Consume(context.Object);

		// Assert
		VerifyRegistration(messageId, Times.Once());
	}

	[Fact]
	public async Task Consume_DeletedEvent_WhenDuplicate_ShouldNotThrow()
	{
		// Arrange
		var messageId = Guid.NewGuid();

		var message = new AccountDeletedEvent(
			Guid.NewGuid());

		var context = CreateContext(messageId, message);

		SetupRegistration(messageId, false);

		// Act
		var exception = await Record.ExceptionAsync(
			() => _consumer.Consume(context.Object));

		// Assert
		Assert.Null(exception);

		VerifyRegistration(messageId, Times.Once());
	}

	[Fact]
	public async Task Consume_DeletedEvent_WithoutMessageId_ShouldThrow()
	{
		// Arrange
		var message = new AccountDeletedEvent(
			Guid.NewGuid());

		var context = CreateContext(
			(Guid?)null,
			message);

		// Act
		var exception = await Assert.ThrowsAsync<InvalidOperationException>(
			() => _consumer.Consume(context.Object));

		// Assert
		Assert.Equal("MessageId is required.", exception.Message);

		VerifyNoRegistration();
	}

	[Fact]
	public async Task Consume_DeletedEvent_WhenRegistrationFails_ShouldThrow()
	{
		// Arrange
		var messageId = Guid.NewGuid();

		var message = new AccountDeletedEvent(
			Guid.NewGuid());

		var context = CreateContext(messageId, message);

		SetupRegistrationFailure(messageId);

		// Act
		var exception = await Assert.ThrowsAsync<InvalidOperationException>(
			() => _consumer.Consume(context.Object));

		// Assert
		Assert.Equal("Database unavailable", exception.Message);

		VerifyRegistration(messageId, Times.Once());
	}
}