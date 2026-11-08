using btgBank.Application.DTOs;
using btgBank.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace btgBank.Api.Controllers;

[ApiController]
[Route("api/accounts")]
public class AccountsController : ControllerBase
{
	private readonly IAccountService _accountService;

	public AccountsController(IAccountService accountService)
	{
		_accountService = accountService;
	}

	[HttpPost]
	public async Task<ActionResult<AccountResponseDto>> Create(
		CreateAccountDto dto,
		CancellationToken cancellationToken)
	{
		var account = await _accountService.CreateAsync(
			dto,
			cancellationToken);

		return CreatedAtAction(
			nameof(GetById),
			new { id = account.Id },
			account);
	}

	[HttpGet("{id:guid}")]
	public async Task<ActionResult<AccountResponseDto>> GetById(
		Guid id,
		CancellationToken cancellationToken)
	{
		var account = await _accountService.GetByIdAsync(
			id,
			cancellationToken);

		if (account is null)
			return NotFound();

		return Ok(account);
	}

	[HttpGet]
	public async Task<ActionResult<IReadOnlyList<AccountResponseDto>>> GetAll(
		CancellationToken cancellationToken)
	{
		var accounts = await _accountService.GetAllAsync(
			cancellationToken);

		return Ok(accounts);
	}

	[HttpPut("{id:guid}")]
	public async Task<IActionResult> Update(
		Guid id,
		UpdateAccountDto dto,
		CancellationToken cancellationToken)
	{
		var updated = await _accountService.UpdateAsync(
			id,
			dto,
			cancellationToken);

		if (!updated)
			return NotFound();

		return NoContent();
	}

	[HttpDelete("{id:guid}")]
	public async Task<IActionResult> Delete(
		Guid id,
		CancellationToken cancellationToken)
	{
		var deleted = await _accountService.DeleteAsync(
			id,
			cancellationToken);

		if (!deleted)
			return NotFound();

		return NoContent();
	}
}