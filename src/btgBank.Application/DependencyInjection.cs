using btgBank.Application.Interfaces;
using btgBank.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace btgBank.Application;

public static class DependencyInjection
{
	public static IServiceCollection AddApplication(
		this IServiceCollection services)
	{
		services.AddScoped<IAccountService, AccountService>();

		return services;
	}
}