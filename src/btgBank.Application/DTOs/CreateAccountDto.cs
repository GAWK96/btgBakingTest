namespace btgBank.Application.DTOs;

public record CreateAccountDto(
	string HolderName,
	string Cpf
);