using btgBank.Domain.Entities;

namespace btgBank.Application.Interfaces;

public interface IAccountRepository
{
	Task<Account?> GetByIdAsync(
		Guid id,
		CancellationToken cancellationToken = default);

	Task<IReadOnlyList<Account>> GetAllAsync(
		CancellationToken cancellationToken = default);

	Task<bool> ExistsByCpfAsync(
		string cpf,
		CancellationToken cancellationToken = default);

	Task AddAsync(
		Account account,
		CancellationToken cancellationToken = default);

	void Remove(Account account);
}