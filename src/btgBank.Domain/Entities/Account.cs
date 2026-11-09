using btgBank.Domain.ValueObjects;

namespace btgBank.Domain.Entities;

public class Account
{
	public Guid Id { get; private set; }
	public string HolderName { get; private set; }
	public string Cpf { get; private set; }
	public bool IsActive { get; private set; }

	public Account(string holderName, string cpf)
	{
		var validatedName = new HolderName(holderName);
		var validatedCpf = new Cpf(cpf);

		Id = Guid.NewGuid();
		HolderName = validatedName.Value;
		Cpf = validatedCpf.Value;
		IsActive = true;
	}

	public void UpdateHolderName(string holderName)
	{
		var validatedName = new HolderName(holderName);

		HolderName = validatedName.Value;
	}

	public void Activate()
	{
		if (IsActive)
			throw new InvalidOperationException(
				"Conta já está ativa!");

		IsActive = true;
	}

	public void Deactivate()
	{
		if (!IsActive)
			throw new InvalidOperationException(
				"Conta já está inativa!");

		IsActive = false;
	}
}