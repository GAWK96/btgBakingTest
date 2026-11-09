using btgBank.Worker.Consumers;
using MassTransit;
using btgBank.Worker.WorkerPersistence;
using Microsoft.EntityFrameworkCore;
using btgBank.Worker.Services;
var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddDbContext<WorkerDbContext>(options =>
{
	options.UseNpgsql(
		builder.Configuration.GetConnectionString("DefaultConnection"),
		npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory_Worker"));
});
builder.Services.AddScoped<
	IProcessedMessageService,
	ProcessedMessageService>();
builder.Services.AddMassTransit(x =>
{
	x.AddConsumer<AccountEventConsumer>();
	x.UsingRabbitMq((context, cfg) =>
	{
		cfg.Host(
			builder.Configuration["RabbitMq:Host"]!,
			"/",
			h =>
			{
				h.Username(
					builder.Configuration["RabbitMq:Username"]!);

				h.Password(
					builder.Configuration["RabbitMq:Password"]!);
			});

		cfg.UseMessageRetry(retry =>
		{
			retry.Interval(
				retryCount: 3,
				interval: TimeSpan.FromSeconds(5));
		});

		cfg.ConfigureEndpoints(context);
	});
});

var host = builder.Build();

host.Run();