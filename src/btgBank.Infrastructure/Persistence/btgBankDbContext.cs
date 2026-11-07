using btgBank.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace btgBank.Infrastructure.Persistence;

public class BtgBankDbContext : DbContext
{
	public BtgBankDbContext(
		DbContextOptions<BtgBankDbContext> options)
		: base(options)
	{
	}

	public DbSet<Account> Accounts => Set<Account>();

	protected override void OnModelCreating(
		ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		modelBuilder.ApplyConfigurationsFromAssembly(
			typeof(BtgBankDbContext).Assembly);
	}
}