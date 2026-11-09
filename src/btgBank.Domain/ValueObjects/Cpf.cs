namespace btgBank.Domain.ValueObjects;

public sealed record Cpf
{
	public string Value { get; }

	public Cpf(string value)
	{
		var normalizedCpf = Normalize(value);

		if (!IsValid(normalizedCpf))
			throw new ArgumentException("CPF inválido.");

		Value = normalizedCpf;
	}

	private static string Normalize(string cpf)
	{
		if (string.IsNullOrWhiteSpace(cpf))
			throw new ArgumentException("CPF não pode ser vazio.");

		return cpf.Replace(".", "")
				  .Replace("-", "")
				  .Trim();
	}

	private static bool IsValid(string cpf)
	{
		if (cpf.Length != 11)
			return false;

		if (!cpf.All(char.IsAsciiDigit))
			return false;

		if (cpf.All(c => c == cpf[0]))
			return false;

		return true;
	}

	public override string ToString() => Value;
}