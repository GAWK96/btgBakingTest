using btgBank.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace btgBank.Infrastructure;

public static class DependencyInjection
{
	public static IServiceCollection AddInfrastructure(
		this IServiceCollection services,
		IConfiguration configuration)
	{
		services.AddDbContext<BtgBankDbContext>(options =>
			options.UseNpgsql(
				configuration.GetConnectionString("DefaultConnection")));

		return services;
	}
}