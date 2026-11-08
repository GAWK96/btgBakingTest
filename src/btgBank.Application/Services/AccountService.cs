using btgBank.Application.DTOs;
using btgBank.Application.Interfaces;
using btgBank.Domain.Entities;

namespace btgBank.Application.Services;

public class AccountService : IAccountService
{
	private readonly IAccountRepository _accountRepository;
	private readonly IUnitOfWork _unitOfWork;

	public AccountService(
		IAccountRepository accountRepository,
		IUnitOfWork unitOfWork)
	{
		_accountRepository = accountRepository;
		_unitOfWork = unitOfWork;
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

		await _unitOfWork.SaveChangesAsync(
			cancellationToken);

		return MapToResponse(account);
	}

	public async Task<AccountResponseDto?> GetByIdAsync(
		Guid id,
		CancellationToken cancellationToken = default)
	{
		var account = await _accountRepository.GetByIdAsync(
			id,
			cancellationToken);

		return account is null
			? null
			: MapToResponse(account);
	}

	public async Task<IReadOnlyList<AccountResponseDto>> GetAllAsync(
		CancellationToken cancellationToken = default)
	{
		var accounts = await _accountRepository.GetAllAsync(
			cancellationToken);

		return accounts
			.Select(MapToResponse)
			.ToList();
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

		await _unitOfWork.SaveChangesAsync(
			cancellationToken);

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

		await _unitOfWork.SaveChangesAsync(
			cancellationToken);

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