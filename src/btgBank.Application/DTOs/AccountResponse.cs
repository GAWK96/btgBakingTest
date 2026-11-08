namespace btgBank.Application.DTOs;

public record AccountResponseDto(
	Guid Id,
	string HolderName,
	string Cpf,
	bool IsActive
);