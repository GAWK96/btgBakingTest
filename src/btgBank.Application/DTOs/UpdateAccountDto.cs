namespace btgBank.Application.DTOs;

public record UpdateAccountDto(
	string HolderName,
	bool IsActive
);