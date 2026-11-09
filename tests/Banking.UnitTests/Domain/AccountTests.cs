using btgBank.Domain.Entities;

namespace btgBank.UnitTests.Domain;

public class AccountTests
{
	[Fact]
	public void Constructor_ShouldCreateActiveAccount_WhenDataIsValid()
	{
		// Arrange
		var holderName = "Guilherme";
		var cpf = "12345678901";

		// Act
		var account = new Account(holderName, cpf);

		// Assert
		Assert.NotEqual(Guid.Empty, account.Id);
		Assert.Equal(holderName, account.HolderName);
		Assert.Equal(cpf, account.Cpf);
		Assert.True(account.IsActive);
	}

	[Fact]
	public void Constructor_ShouldNormalizeHolderNameAndCpf()
	{
		// Arrange and Act
		var account = new Account(
			"  Guilherme  ",
			"123.456.789-01");

		// Assert
		Assert.Equal("Guilherme", account.HolderName);
		Assert.Equal("12345678901", account.Cpf);
	}

	[Theory]
	[InlineData("")]
	[InlineData(" ")]
	[InlineData(null)]
	public void Constructor_ShouldThrow_WhenHolderNameIsInvalid(
		string? holderName)
	{
		// Act and Assert
		Assert.Throws<ArgumentException>(
			() => new Account(holderName!, "12345678901"));
	}

	[Theory]
	[InlineData("")]
	[InlineData("123")]
	[InlineData("11111111111")]
	[InlineData(null)]
	public void Constructor_ShouldThrow_WhenCpfIsInvalid(string? cpf)
	{
		// Act and Assert
		Assert.Throws<ArgumentException>(
			() => new Account("Guilherme", cpf!));
	}

	[Fact]
	public void UpdateHolderName_ShouldUpdateName_WhenValid()
	{
		// Arrange
		var account = new Account(
			"Guilherme",
			"12345678901");

		// Act
		account.UpdateHolderName("João");

		// Assert
		Assert.Equal("João", account.HolderName);
	}

	[Fact]
	public void UpdateHolderName_ShouldNormalizeName()
	{
		// Arrange
		var account = new Account(
			"Guilherme",
			"12345678901");

		// Act
		account.UpdateHolderName("  João Silva  ");

		// Assert
		Assert.Equal("João Silva", account.HolderName);
	}

	[Fact]
	public void UpdateHolderName_ShouldThrow_WhenNameIsInvalid()
	{
		// Arrange
		var account = new Account(
			"Guilherme",
			"12345678901");

		// Act and Assert
		Assert.Throws<ArgumentException>(
			() => account.UpdateHolderName(""));

		Assert.Equal("Guilherme", account.HolderName);
	}

	[Fact]
	public void Deactivate_ShouldSetAccountInactive_WhenActive()
	{
		// Arrange
		var account = new Account(
			"Guilherme",
			"12345678901");

		// Act
		account.Deactivate();

		// Assert
		Assert.False(account.IsActive);
	}

	[Fact]
	public void Deactivate_ShouldThrow_WhenAlreadyInactive()
	{
		// Arrange
		var account = new Account(
			"Guilherme",
			"12345678901");

		account.Deactivate();

		// Act and Assert
		var exception = Assert.Throws<InvalidOperationException>(
			() => account.Deactivate());

		Assert.Equal("Conta já está inativa!", exception.Message);
		Assert.False(account.IsActive);
	}

	[Fact]
	public void Activate_ShouldSetAccountActive_WhenInactive()
	{
		// Arrange
		var account = new Account(
			"Guilherme",
			"12345678901");

		account.Deactivate();

		// Act
		account.Activate();

		// Assert
		Assert.True(account.IsActive);
	}

	[Fact]
	public void Activate_ShouldThrow_WhenAlreadyActive()
	{
		// Arrange
		var account = new Account(
			"Guilherme",
			"12345678901");

		// Act and Assert
		var exception = Assert.Throws<InvalidOperationException>(
			() => account.Activate());

		Assert.Equal("Conta já está ativa!", exception.Message);
		Assert.True(account.IsActive);
	}
}