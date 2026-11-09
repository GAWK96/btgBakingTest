using System.Text.Json;
using btgBank.Application.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace btgBank.Infrastructure.Cache;

public class RedisCacheService : ICacheService
{
	private readonly IDistributedCache _cache;

	public RedisCacheService(IDistributedCache cache)
	{
		_cache = cache;
	}

	public async Task<T?> GetAsync<T>(
		string key,
		CancellationToken cancellationToken = default)
	{
		var cachedData = await _cache.GetStringAsync(
			key,
			cancellationToken);

		if (cachedData is null)
			return default;

		return JsonSerializer.Deserialize<T>(cachedData);
	}

	public async Task SetAsync<T>(
		string key,
		T value,
		TimeSpan expiration,
		CancellationToken cancellationToken = default)
	{
		var serializedData = JsonSerializer.Serialize(value);

		var options = new DistributedCacheEntryOptions
		{
			AbsoluteExpirationRelativeToNow = expiration
		};

		await _cache.SetStringAsync(
			key,
			serializedData,
			options,
			cancellationToken);
	}

	public async Task RemoveAsync(
		string key,
		CancellationToken cancellationToken = default)
	{
		await _cache.RemoveAsync(
			key,
			cancellationToken);
	}
}