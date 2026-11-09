using btgBank.Domain.ValueObjects;

namespace btgBank.UnitTests.Domain;

public class HolderNameTests
{
	[Fact]
	public void Constructor_ShouldCreateHolderName_WhenNameIsValid()
	{
		// Arrange
		var name = "Guilherme Krieger";

		// Act
		var holderName = new HolderName(name);

		// Assert
		Assert.Equal(name, holderName.Value);
	}

	[Theory]
	[InlineData("")]
	[InlineData(" ")]
	[InlineData("   ")]
	[InlineData(null)]
	public void Constructor_ShouldThrow_WhenNameIsEmpty(string? value)
	{
		// Act & Assert
		Assert.Throws<ArgumentException>(
			() => new HolderName(value!));
	}

	[Fact]
	public void Constructor_ShouldThrow_WhenNameExceedsMaxLength()
	{
		// Arrange
		var name = new string('A', 151);

		// Act & Assert
		Assert.Throws<ArgumentException>(
			() => new HolderName(name));
	}

	[Fact]
	public void Constructor_ShouldAcceptName_WithExactly150Characters()
	{
		// Arrange
		var name = new string('A', 150);

		// Act
		var holderName = new HolderName(name);

		// Assert
		Assert.Equal(150, holderName.Value.Length);
	}

	[Theory]
	[InlineData(" Guilherme", "Guilherme")]
	[InlineData("Guilherme ", "Guilherme")]
	[InlineData("  Guilherme Krieger  ", "Guilherme Krieger")]
	public void Constructor_ShouldTrimWhitespace(
		string input,
		string expected)
	{
		// Act
		var holderName = new HolderName(input);

		// Assert
		Assert.Equal(expected, holderName.Value);
	}

	[Fact]
	public void ToString_ShouldReturnHolderName()
	{
		// Arrange
		var holderName = new HolderName("Guilherme Krieger");

		// Act
		var result = holderName.ToString();

		// Assert
		Assert.Equal("Guilherme Krieger", result);
	}

	[Fact]
	public void Equals_ShouldReturnTrue_WhenValuesAreEqual()
	{
		// Arrange
		var name1 = new HolderName("Guilherme Krieger");
		var name2 = new HolderName("  Guilherme Krieger  ");

		// Act & Assert
		Assert.Equal(name1, name2);
	}
}