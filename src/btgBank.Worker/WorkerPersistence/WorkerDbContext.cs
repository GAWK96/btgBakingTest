using btgBank.Worker.WorkerPersistence;
using Microsoft.EntityFrameworkCore;

namespace btgBank.Worker.WorkerPersistence;

public class WorkerDbContext : DbContext
{
	public WorkerDbContext(
		DbContextOptions<WorkerDbContext> options)
		: base(options)
	{
	}

	public DbSet<ProcessedMessage> ProcessedMessages
		=> Set<ProcessedMessage>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<ProcessedMessage>(entity =>
		{
			entity.ToTable("ProcessedMessages");

			entity.HasKey(x => x.MessageId);

			entity.Property(x => x.ProcessedAt)
				.IsRequired();
		});
	}
}