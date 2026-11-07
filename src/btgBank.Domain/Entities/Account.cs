namespace btgBank.Domain.Entities;

public class Account
{
	public Guid Id { get; private set; }
	public string HolderName { get; private set; }
	public string Cpf { get; private set; }
	public bool IsActive { get; private set; }

	public Account(string holderName, string cpf)
	{
		ValidateHolderName(holderName);
		ValidateCpf(cpf);

		Id = Guid.NewGuid();
		HolderName = holderName.Trim();
		Cpf = NormalizeCpf(cpf);
		IsActive = true;
	}

	public void UpdateHolderName(string holderName)
	{
		ValidateHolderName(holderName);

		HolderName = holderName.Trim();
	}

	public void Activate()
	{
		if (IsActive)
			throw new InvalidOperationException(
				"Conta já está ativa!.");

		IsActive = true;
	}

	public void Deactivate()
	{
		if (!IsActive)
			throw new InvalidOperationException(
				"Conta já está inativa!.");

		IsActive = false;
	}

	private static void ValidateHolderName(string holderName)
	{
		if (string.IsNullOrWhiteSpace(holderName))
			throw new ArgumentException(
				"Nome do titular não pode ser vazio.");

		if (holderName.Trim().Length > 150)
			throw new ArgumentException(
				"Nome do titular não pode exceder 150 caracteres.");
	}

	private static void ValidateCpf(string cpf)
	{
		if (string.IsNullOrWhiteSpace(cpf))
			throw new ArgumentException(
				"CPF não pode ser vazio.");

		var normalizedCpf = NormalizeCpf(cpf);

		if (normalizedCpf.Length != 11 ||
			!normalizedCpf.All(char.IsAsciiDigit))
		{
			throw new ArgumentException(
				"CPF deve conter exatamente 11 dígitos.");
		}
	}

	private static string NormalizeCpf(string cpf)
	{
		return cpf.Replace(".", "")
				  .Replace("-", "")
				  .Trim();
	}
}