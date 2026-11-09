using System.Timers;
using btgBank.Application.DTOs;
using btgBank.Application.Events;
using btgBank.Application.Interfaces;
using btgBank.Application.Services;
using btgBank.Domain.Entities;
using Moq;

namespace btgBank.UnitTests.Application;

public class AccountServiceTests
{
	private readonly Mock<IAccountRepository> _repositoryMock;
	private readonly Mock<IUnitOfWork> _unitOfWorkMock;
	private readonly Mock<ICacheService> _cacheMock;
	private readonly Mock<IEventPublisher> _eventPublisherMock;

	private readonly AccountService _service;

	public AccountServiceTests()
	{
		_repositoryMock = new Mock<IAccountRepository>();
		_unitOfWorkMock = new Mock<IUnitOfWork>();
		_cacheMock = new Mock<ICacheService>();
		_eventPublisherMock = new Mock<IEventPublisher>();

		_service = new AccountService(
			_repositoryMock.Object,
			_unitOfWorkMock.Object,
			_cacheMock.Object,
			_eventPublisherMock.Object);
	}

	// =========================================================
	// CREATE
	// =========================================================

	[Fact]
	public async Task CreateAsync_ShouldCreateAccount_WhenCpfDoesNotExist()
	{
		// Arrange
		var dto = new CreateAccountDto(
			"Guilherme",
			"12345678901");

		_repositoryMock
			.Setup(r => r.ExistsByCpfAsync(
				dto.Cpf,
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(false);

		// Act
		var result = await _service.CreateAsync(dto);

		// Assert
		Assert.NotNull(result);
		Assert.NotEqual(Guid.Empty, result.Id);
		Assert.Equal("Guilherme", result.HolderName);
		Assert.Equal("12345678901", result.Cpf);
		Assert.True(result.IsActive);

		_repositoryMock.Verify(
			r => r.AddAsync(
				It.Is<Account>(a =>
					a.HolderName == "Guilherme" &&
					a.Cpf == "12345678901"),
				It.IsAny<CancellationToken>()),
			Times.Once);

		_eventPublisherMock.Verify(
			p => p.PublishAsync(
				It.Is<AccountCreatedEvent>(e =>
					e.AccountId == result.Id &&
					e.HolderName == result.HolderName),
				It.IsAny<CancellationToken>()),
			Times.Once);

		_unitOfWorkMock.Verify(
			u => u.SaveChangesAsync(
				It.IsAny<CancellationToken>()),
			Times.Once);

		_cacheMock.Verify(
			c => c.RemoveAsync(
				"allAccounts",
				It.IsAny<CancellationToken>()),
			Times.Once);
	}

	[Fact]
	public async Task CreateAsync_ShouldThrow_WhenCpfAlreadyExists()
	{
		// Arrange
		var dto = new CreateAccountDto(
			"Guilherme",
			"12345678901");

		_repositoryMock
			.Setup(r => r.ExistsByCpfAsync(
				dto.Cpf,
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(true);

		// Act
		var exception =
			await Assert.ThrowsAsync<InvalidOperationException>(
				() => _service.CreateAsync(dto));

		// Assert
		Assert.Equal(
			"An account with this CPF already exists.",
			exception.Message);

		_repositoryMock.Verify(
			r => r.AddAsync(
				It.IsAny<Account>(),
				It.IsAny<CancellationToken>()),
			Times.Never);

		_eventPublisherMock.Verify(
			p => p.PublishAsync(
				It.IsAny<AccountCreatedEvent>(),
				It.IsAny<CancellationToken>()),
			Times.Never);

		_unitOfWorkMock.Verify(
			u => u.SaveChangesAsync(
				It.IsAny<CancellationToken>()),
			Times.Never);
	}

	[Fact]
	public async Task CreateAsync_ShouldThrow_WhenCpfIsInvalid()
	{
		// Arrange
		var dto = new CreateAccountDto(
			"Guilherme",
			"11111111111");

		// Act
		await Assert.ThrowsAsync<ArgumentException>(
			() => _service.CreateAsync(dto));

		// Assert
		_repositoryMock.Verify(
			r => r.AddAsync(
				It.IsAny<Account>(),
				It.IsAny<CancellationToken>()),
			Times.Never);

		_unitOfWorkMock.Verify(
			u => u.SaveChangesAsync(
				It.IsAny<CancellationToken>()),
			Times.Never);
	}

	// =========================================================
	// GET BY ID
	// =========================================================

	[Fact]
	public async Task GetByIdAsync_ShouldReturnCachedAccount_WhenCacheHit()
	{
		// Arrange
		var id = Guid.NewGuid();

		var cachedAccount = new AccountResponseDto(
			id,
			"Guilherme",
			"12345678901",
			true);

		_cacheMock
			.Setup(c => c.GetAsync<AccountResponseDto>(
				$"account:{id}",
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(cachedAccount);

		// Act
		var result = await _service.GetByIdAsync(id);

		// Assert
		Assert.NotNull(result);
		Assert.Equal(id, result.Id);
		Assert.Equal("Guilherme", result.HolderName);

		_repositoryMock.Verify(
			r => r.GetByIdAsync(
				It.IsAny<Guid>(),
				It.IsAny<CancellationToken>()),
			Times.Never);
	}

	[Fact]
	public async Task GetByIdAsync_ShouldGetFromRepository_WhenCacheMiss()
	{
		// Arrange
		var account = new Account(
			"Guilherme",
			"12345678901");

		_cacheMock
			.Setup(c => c.GetAsync<AccountResponseDto>(
				$"account:{account.Id}",
				It.IsAny<CancellationToken>()))
			.ReturnsAsync((AccountResponseDto?)null);

		_repositoryMock
			.Setup(r => r.GetByIdAsync(
				account.Id,
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(account);

		// Act
		var result = await _service.GetByIdAsync(account.Id);

		// Assert
		Assert.NotNull(result);
		Assert.Equal(account.Id, result.Id);
		Assert.Equal(account.HolderName, result.HolderName);

		_repositoryMock.Verify(
			r => r.GetByIdAsync(
				account.Id,
				It.IsAny<CancellationToken>()),
			Times.Once);

		_cacheMock.Verify(
			c => c.SetAsync(
				$"account:{account.Id}",
				It.Is<AccountResponseDto>(a =>
					a.Id == account.Id),
				It.IsAny<TimeSpan>(),
				It.IsAny<CancellationToken>()),
			Times.Once);
	}

	[Fact]
	public async Task GetByIdAsync_ShouldReturnNull_WhenAccountDoesNotExist()
	{
		// Arrange
		var id = Guid.NewGuid();

		_cacheMock
			.Setup(c => c.GetAsync<AccountResponseDto>(
				$"account:{id}",
				It.IsAny<CancellationToken>()))
			.ReturnsAsync((AccountResponseDto?)null);

		_repositoryMock
			.Setup(r => r.GetByIdAsync(
				id,
				It.IsAny<CancellationToken>()))
			.ReturnsAsync((Account?)null);

		// Act
		var result = await _service.GetByIdAsync(id);

		// Assert
		Assert.Null(result);

		_cacheMock.Verify(
			c => c.SetAsync(
				It.IsAny<string>(),
				It.IsAny<AccountResponseDto>(),
				It.IsAny<TimeSpan>(),
				It.IsAny<CancellationToken>()),
			Times.Never);
	}

	// =========================================================
	// GET ALL
	// =========================================================

	[Fact]
	public async Task GetAllAsync_ShouldReturnCachedAccounts_WhenCacheHit()
	{
		// Arrange
		IReadOnlyList<AccountResponseDto> cachedAccounts =
			new List<AccountResponseDto>
			{
				new(
					Guid.NewGuid(),
					"Guilherme",
					"12345678901",
					true)
			};

		_cacheMock
			.Setup(c => c.GetAsync<IReadOnlyList<AccountResponseDto>>(
				"allAccounts",
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(cachedAccounts);

		// Act
		var result = await _service.GetAllAsync();

		// Assert
		Assert.Single(result);
		Assert.Equal("Guilherme", result[0].HolderName);

		_repositoryMock.Verify(
			r => r.GetAllAsync(
				It.IsAny<CancellationToken>()),
			Times.Never);
	}

	[Fact]
	public async Task GetAllAsync_ShouldGetFromRepository_WhenCacheMiss()
	{
		// Arrange
		var accounts = new List<Account>
		{
			new("Guilherme", "12345678901"),
			new("João", "98765432100")
		};

		_cacheMock
			.Setup(c => c.GetAsync<IReadOnlyList<AccountResponseDto>>(
				"allAccounts",
				It.IsAny<CancellationToken>()))
			.ReturnsAsync((IReadOnlyList<AccountResponseDto>?)null);

		_repositoryMock
			.Setup(r => r.GetAllAsync(
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(accounts);

		// Act
		var result = await _service.GetAllAsync();

		// Assert
		Assert.Equal(2, result.Count);

		_repositoryMock.Verify(
			r => r.GetAllAsync(
				It.IsAny<CancellationToken>()),
			Times.Once);

		_cacheMock.Verify(
			c => c.SetAsync(
				"allAccounts",
				It.IsAny<IReadOnlyList<AccountResponseDto>>(),
				It.IsAny<TimeSpan>(),
				It.IsAny<CancellationToken>()),
			Times.Once);
	}

	[Fact]
	public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoAccountsExist()
	{
		// Arrange
		_repositoryMock
			.Setup(r => r.GetAllAsync(
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(new List<Account>());

		// Act
		var result = await _service.GetAllAsync();

		// Assert
		Assert.NotNull(result);
		Assert.Empty(result);
	}

	// =========================================================
	// UPDATE
	// =========================================================

	[Fact]
	public async Task UpdateAsync_ShouldUpdateAccount_WhenAccountExists()
	{
		// Arrange
		var account = new Account(
			"Guilherme",
			"12345678901");

		var dto = new UpdateAccountDto(
			"João",
			false);

		_repositoryMock
			.Setup(r => r.GetByIdAsync(
				account.Id,
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(account);

		// Act
		var result = await _service.UpdateAsync(
			account.Id,
			dto);

		// Assert
		Assert.True(result);
		Assert.Equal("João", account.HolderName);
		Assert.False(account.IsActive);

		_eventPublisherMock.Verify(
			p => p.PublishAsync(
				It.Is<AccountUpdatedEvent>(e =>
					e.AccountId == account.Id &&
					e.HolderName == "João" &&
					!e.IsActive),
				It.IsAny<CancellationToken>()),
			Times.Once);

		_unitOfWorkMock.Verify(
			u => u.SaveChangesAsync(
				It.IsAny<CancellationToken>()),
			Times.Once);

		_cacheMock.Verify(
			c => c.RemoveAsync(
				$"account:{account.Id}",
				It.IsAny<CancellationToken>()),
			Times.Once);

		_cacheMock.Verify(
			c => c.RemoveAsync(
				"allAccounts",
				It.IsAny<CancellationToken>()),
			Times.Once);
	}

	[Fact]
	public async Task UpdateAsync_ShouldReturnFalse_WhenAccountDoesNotExist()
	{
		// Arrange
		var id = Guid.NewGuid();

		var dto = new UpdateAccountDto(
			"João",
			true);

		_repositoryMock
			.Setup(r => r.GetByIdAsync(
				id,
				It.IsAny<CancellationToken>()))
			.ReturnsAsync((Account?)null);

		// Act
		var result = await _service.UpdateAsync(id, dto);

		// Assert
		Assert.False(result);

		_unitOfWorkMock.Verify(
			u => u.SaveChangesAsync(
				It.IsAny<CancellationToken>()),
			Times.Never);

		_eventPublisherMock.Verify(
			p => p.PublishAsync(
				It.IsAny<AccountUpdatedEvent>(),
				It.IsAny<CancellationToken>()),
			Times.Never);
	}

	// =========================================================
	// DELETE
	// =========================================================

	[Fact]
	public async Task DeleteAsync_ShouldDeleteAccount_WhenAccountExists()
	{
		// Arrange
		var account = new Account(
			"Guilherme",
			"12345678901");

		_repositoryMock
			.Setup(r => r.GetByIdAsync(
				account.Id,
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(account);

		// Act
		var result = await _service.DeleteAsync(account.Id);

		// Assert
		Assert.True(result);

		_repositoryMock.Verify(
			r => r.Remove(account),
			Times.Once);

		_eventPublisherMock.Verify(
			p => p.PublishAsync(
				It.Is<AccountDeletedEvent>(e =>
					e.AccountId == account.Id),
				It.IsAny<CancellationToken>()),
			Times.Once);

		_unitOfWorkMock.Verify(
			u => u.SaveChangesAsync(
				It.IsAny<CancellationToken>()),
			Times.Once);

		_cacheMock.Verify(
			c => c.RemoveAsync(
				$"account:{account.Id}",
				It.IsAny<CancellationToken>()),
			Times.Once);

		_cacheMock.Verify(
			c => c.RemoveAsync(
				"allAccounts",
				It.IsAny<CancellationToken>()),
			Times.Once);
	}

	[Fact]
	public async Task DeleteAsync_ShouldReturnFalse_WhenAccountDoesNotExist()
	{
		// Arrange
		var id = Guid.NewGuid();

		_repositoryMock
			.Setup(r => r.GetByIdAsync(
				id,
				It.IsAny<CancellationToken>()))
			.ReturnsAsync((Account?)null);

		// Act
		var result = await _service.DeleteAsync(id);

		// Assert
		Assert.False(result);

		_repositoryMock.Verify(
			r => r.Remove(It.IsAny<Account>()),
			Times.Never);

		_unitOfWorkMock.Verify(
			u => u.SaveChangesAsync(
				It.IsAny<CancellationToken>()),
			Times.Never);

		_eventPublisherMock.Verify(
			p => p.PublishAsync(
				It.IsAny<AccountDeletedEvent>(),
				It.IsAny<CancellationToken>()),
			Times.Never);
	}
}