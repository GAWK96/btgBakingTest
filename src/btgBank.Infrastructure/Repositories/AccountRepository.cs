using btgBank.Application.Interfaces;
using btgBank.Domain.Entities;
using btgBank.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace btgBank.Infrastructure.Repositories;

public class AccountRepository : IAccountRepository
{
	private readonly BtgBankDbContext _context;

	public AccountRepository(BtgBankDbContext context)
	{
		_context = context;
	}

	public async Task<Account?> GetByIdAsync(
		Guid id,
		CancellationToken cancellationToken = default)
	{
		return await _context.Accounts
			.FirstOrDefaultAsync(
				a => a.Id == id,
				cancellationToken);
	}

	public async Task<IReadOnlyList<Account>> GetAllAsync(
		CancellationToken cancellationToken = default)
	{
		return await _context.Accounts
			.AsNoTracking()
			.ToListAsync(cancellationToken);
	}

	public async Task<bool> ExistsByCpfAsync(
		string cpf,
		CancellationToken cancellationToken = default)
	{
		return await _context.Accounts
			.AnyAsync(
				a => a.Cpf == cpf,
				cancellationToken);
	}

	public async Task AddAsync(
		Account account,
		CancellationToken cancellationToken = default)
	{
		await _context.Accounts.AddAsync(
			account,
			cancellationToken);
	}

	public void Remove(Account account)
	{
		_context.Accounts.Remove(account);
	}
}