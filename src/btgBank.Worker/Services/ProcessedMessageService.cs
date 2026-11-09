using btgBank.Worker.Persistence;
using btgBank.Worker.WorkerPersistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace btgBank.Worker.Services;

public class ProcessedMessageService : IProcessedMessageService
{
	private readonly WorkerDbContext _dbContext;

	public ProcessedMessageService(WorkerDbContext dbContext)
	{
		_dbContext = dbContext;
	}

	public async Task<bool> TryRegisterAsync(
		Guid messageId,
		CancellationToken cancellationToken = default)
	{
		var processedAt = DateTime.UtcNow;

		var affectedRows = await _dbContext.Database.ExecuteSqlInterpolatedAsync(
			$"""
            INSERT INTO "ProcessedMessages" ("MessageId", "ProcessedAt")
            VALUES ({messageId}, {processedAt})
            ON CONFLICT ("MessageId") DO NOTHING
            """,
			cancellationToken);

		return affectedRows > 0;
	}
}