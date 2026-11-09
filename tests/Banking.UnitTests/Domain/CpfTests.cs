using btgBank.Domain.ValueObjects;

namespace btgBank.UnitTests.Domain;

public class CpfTests
{
	[Theory]
	[InlineData("12345678901")]
	[InlineData("98765432100")]
	public void Constructor_ShouldCreateCpf_WhenFormatIsValid(string value)
	{
		// Act
		var cpf = new Cpf(value);

		// Assert
		Assert.Equal(value, cpf.Value);
	}

	[Theory]
	[InlineData("123.456.789-01", "12345678901")]
	[InlineData("987.654.321-00", "98765432100")]
	[InlineData(" 123.456.789-01 ", "12345678901")]
	public void Constructor_ShouldNormalizeCpf_WhenFormatted(
		string input,
		string expected)
	{
		// Act
		var cpf = new Cpf(input);

		// Assert
		Assert.Equal(expected, cpf.Value);
	}

	[Theory]
	[InlineData("")]
	[InlineData(" ")]
	[InlineData("   ")]
	[InlineData(null)]
	public void Constructor_ShouldThrow_WhenCpfIsEmpty(string? value)
	{
		// Act & Assert
		Assert.Throws<ArgumentException>(() => new Cpf(value!));
	}

	[Theory]
	[InlineData("1234567890")]
	[InlineData("123456789012")]
	public void Constructor_ShouldThrow_WhenCpfLengthIsInvalid(string value)
	{
		// Act & Assert
		Assert.Throws<ArgumentException>(() => new Cpf(value));
	}

	[Theory]
	[InlineData("1234567890A")]
	[InlineData("1234567890!")]
	[InlineData("12345 78901")]
	public void Constructor_ShouldThrow_WhenCpfContainsInvalidCharacters(
		string value)
	{
		// Act & Assert
		Assert.Throws<ArgumentException>(() => new Cpf(value));
	}

	[Theory]
	[InlineData("11111111111")]
	[InlineData("00000000000")]
	[InlineData("99999999999")]
	public void Constructor_ShouldThrow_WhenAllDigitsAreEqual(string value)
	{
		// Act & Assert
		Assert.Throws<ArgumentException>(() => new Cpf(value));
	}

	[Fact]
	public void ToString_ShouldReturnNormalizedCpf()
	{
		// Arrange
		var cpf = new Cpf("123.456.789-01");

		// Act
		var result = cpf.ToString();

		// Assert
		Assert.Equal("12345678901", result);
	}
}