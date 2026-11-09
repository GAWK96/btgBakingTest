using btgBank.Application.DTOs;
using btgBank.Application.Interfaces;
using btgBank.Domain.Entities;
using btgBank.Application.Events;
namespace btgBank.Application.Services;

public class AccountService : IAccountService
{
	private readonly IAccountRepository _accountRepository;
	private readonly IUnitOfWork _unitOfWork;

	private readonly ICacheService _cacheService;

	private readonly IEventPublisher _eventPublisher;

	public AccountService(
		IAccountRepository accountRepository,
		IUnitOfWork unitOfWork,
		ICacheService cacheService,
		IEventPublisher eventPublisher)
	{
		_accountRepository = accountRepository;
		_unitOfWork = unitOfWork;
		_cacheService = cacheService;
		_eventPublisher = eventPublisher;
	}

	public async Task<AccountResponseDto> CreateAsync(
		CreateAccountDto dto,
		CancellationToken cancellationToken = default)
	{
		var account = new Account(
			dto.HolderName,
			dto.Cpf);

		var exists = await _accountRepository.ExistsByCpfAsync(
			account.Cpf,
			cancellationToken);

		if (exists)
			throw new InvalidOperationException(
				"An account with this CPF already exists.");

		await _accountRepository.AddAsync(
			account,
			cancellationToken);
		await _eventPublisher.PublishAsync(
		new AccountCreatedEvent(
			account.Id,
			account.HolderName),
		cancellationToken);
		await _unitOfWork.SaveChangesAsync(
			cancellationToken);
		await _cacheService.RemoveAsync($"allAccounts",cancellationToken);
		return MapToResponse(account);
	}

	public async Task<AccountResponseDto?> GetByIdAsync(
	Guid id,
	CancellationToken cancellationToken = default)
	{
		var cacheKey = $"account:{id}";

		var cachedAccount =
			await _cacheService.GetAsync<AccountResponseDto>(
				cacheKey,
				cancellationToken);

		if (cachedAccount is not null)
			return cachedAccount;

		var account = await _accountRepository.GetByIdAsync(
			id,
			cancellationToken);

		if (account is null)
			return null;

		var response = MapToResponse(account);

		var now = DateTimeOffset.Now; //Para producao, definir qual fuso representa o dia de negocio

		var midnight = now.Date.AddDays(1);

		var expiration = midnight - now;

		await _cacheService.SetAsync(
			cacheKey,
			response,
			expiration,
			cancellationToken);

		return response;
	}

	public async Task<IReadOnlyList<AccountResponseDto>> GetAllAsync(
		CancellationToken cancellationToken = default)
	{
		var cacheKey = $"allAccounts";
		var cachedAccount =
			await _cacheService.GetAsync<IReadOnlyList<AccountResponseDto>>(
				cacheKey,
				cancellationToken);

		if (cachedAccount is not null)
			return cachedAccount;
		var accounts = await _accountRepository.GetAllAsync(
			cancellationToken);

		var response = accounts
			.Select(MapToResponse)
			.ToList();
		var now = DateTimeOffset.Now; //Para producao, definir qual fuso representa o dia de negocio

		var midnight = now.Date.AddDays(1);

		var expiration = midnight - now;
		
		await _cacheService.SetAsync(
			cacheKey,
			response,
			expiration,
			cancellationToken);
		return response;
	}

	public async Task<bool> UpdateAsync(
		Guid id,
		UpdateAccountDto dto,
		CancellationToken cancellationToken = default)
	{
		var account = await _accountRepository.GetByIdAsync(
			id,
			cancellationToken);

		if (account is null)
			return false;

		account.UpdateHolderName(dto.HolderName);

		if (dto.IsActive && !account.IsActive)
			account.Activate();
		else if (!dto.IsActive && account.IsActive)
			account.Deactivate();
		await _eventPublisher.PublishAsync(
				new AccountUpdatedEvent(
					account.Id,
					account.HolderName,
					account.IsActive),
				cancellationToken);
		await _unitOfWork.SaveChangesAsync(
			cancellationToken);
		await _cacheService.RemoveAsync(
		    $"account:{id}",		
		    cancellationToken);
		Console.WriteLine($"account:{id} removed from cache");
		await _cacheService.RemoveAsync(
		    $"allAccounts",
		    cancellationToken);
		Console.WriteLine($"All accounts removed from cache");
		return true;
	}

	public async Task<bool> DeleteAsync(
		Guid id,
		CancellationToken cancellationToken = default)
	{
		var account = await _accountRepository.GetByIdAsync(
			id,
			cancellationToken);

		if (account is null)
			return false;

		_accountRepository.Remove(account);
		await _eventPublisher.PublishAsync(
				new AccountDeletedEvent(account.Id),
				cancellationToken);
		await _unitOfWork.SaveChangesAsync(
			cancellationToken);
		await _cacheService.RemoveAsync(
			$"account:{id}",
			cancellationToken);
		Console.WriteLine($"account:{id} removed from cache");
		await _cacheService.RemoveAsync(
		    $"allAccounts",
		    cancellationToken);
		Console.WriteLine($"All accounts removed from cache");
		return true;
	}

	private static AccountResponseDto MapToResponse(Account account)
	{
		return new AccountResponseDto(
			account.Id,
			account.HolderName,
			account.Cpf,
			account.IsActive);
	}
}