using btgBank.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace btgBank.Infrastructure.Persistence.Configurations;

public class AccountConfiguration
	: IEntityTypeConfiguration<Account>
{
	public void Configure(
		EntityTypeBuilder<Account> builder)
	{
		builder.ToTable("Accounts");

		builder.HasKey(a => a.Id);

		builder.Property(a => a.HolderName)
			.IsRequired()
			.HasMaxLength(150);

		builder.Property(a => a.Cpf)
			.IsRequired()
			.HasMaxLength(11);

		builder.Property(a => a.IsActive)
			.IsRequired();

		builder.HasIndex(a => a.Cpf)
			.IsUnique();
	}
}