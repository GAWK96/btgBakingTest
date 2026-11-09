using btgBank.Application.Interfaces;
using btgBank.Infrastructure.Persistence;
using btgBank.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using btgBank.Infrastructure.Cache;
using MassTransit;
using btgBank.Infrastructure.Messaging;
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

		services.AddScoped<IAccountRepository, AccountRepository>();
		services.AddScoped<IUnitOfWork, UnitOfWork>();
		services.AddScoped<IEventPublisher, MassTransitEventPublisher>();
		services.AddMassTransit(x =>
		{
			x.AddEntityFrameworkOutbox<BtgBankDbContext>(options =>
			{
				options.UsePostgres();
				options.UseBusOutbox();
			});

			x.UsingRabbitMq((context, cfg) =>
			{
				cfg.Host(
					configuration["RabbitMq:Host"],
					"/",
					h =>
					{
						h.Username(configuration["RabbitMq:Username"]!);
						h.Password(configuration["RabbitMq:Password"]!);
					});	

				cfg.ConfigureEndpoints(context);
			});
		});
		services.AddStackExchangeRedisCache(options =>
		{
			options.Configuration =
				configuration.GetConnectionString("Redis");
		});

		services.AddScoped<ICacheService, RedisCacheService>();

		return services;
	}
}