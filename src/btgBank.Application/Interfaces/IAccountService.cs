using btgBank.Application.DTOs;

namespace btgBank.Application.Interfaces;

public interface IAccountService
{
	Task<AccountResponseDto> CreateAsync(
		CreateAccountDto dto,
		CancellationToken cancellationToken = default);

	Task<AccountResponseDto?> GetByIdAsync(
		Guid id,
		CancellationToken cancellationToken = default);

	Task<IReadOnlyList<AccountResponseDto>> GetAllAsync(
		CancellationToken cancellationToken = default);

	Task<bool> UpdateAsync(
		Guid id,
		UpdateAccountDto dto,
		CancellationToken cancellationToken = default);

	Task<bool> DeleteAsync(
		Guid id,
		CancellationToken cancellationToken = default);
}