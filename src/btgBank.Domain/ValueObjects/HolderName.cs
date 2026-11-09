namespace btgBank.Domain.ValueObjects;

public sealed record HolderName
{
	public string Value { get; }

	public HolderName(string value)
	{
		if (string.IsNullOrWhiteSpace(value))
			throw new ArgumentException(
				"Nome do titular não pode ser vazio.");

		var normalizedName = value.Trim();

		if (normalizedName.Length > 150)
			throw new ArgumentException(
				"Nome do titular não pode exceder 150 caracteres.");

		Value = normalizedName;
	}

	public override string ToString() => Value;
}