using btgBank.Application.Interfaces;
using btgBank.Infrastructure.Persistence;

namespace btgBank.Infrastructure;

public class UnitOfWork : IUnitOfWork
{
	private readonly BtgBankDbContext _context;

	public UnitOfWork(BtgBankDbContext context)
	{
		_context = context;
	}

	public Task<int> SaveChangesAsync(
		CancellationToken cancellationToken = default)
	{
		return _context.SaveChangesAsync(cancellationToken);
	}
}