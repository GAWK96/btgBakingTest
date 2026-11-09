using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace btgBank.Api.Exceptions;

public class GlobalExceptionHandler : IExceptionHandler
{
	private readonly ILogger<GlobalExceptionHandler> _logger;

	public GlobalExceptionHandler(
		ILogger<GlobalExceptionHandler> logger)
	{
		_logger = logger;
	}

	public async ValueTask<bool> TryHandleAsync(
		HttpContext httpContext,
		Exception exception,
		CancellationToken cancellationToken)
	{
		_logger.LogError(
			exception,
			"An exception occurred: {Message}",
			exception.Message);

		var statusCode = exception switch
		{
			ArgumentException =>
				StatusCodes.Status400BadRequest,

			InvalidOperationException =>
				StatusCodes.Status409Conflict,

			_ =>
				StatusCodes.Status500InternalServerError
		};

		var problemDetails = new ProblemDetails
		{
			Status = statusCode,
			Title = statusCode switch
			{
				400 => "Bad Request",
				409 => "Conflict",
				_ => "Internal Server Error"
			},
			Detail = statusCode == 500
				? "An unexpected error occurred."
				: exception.Message,
			Instance = httpContext.Request.Path
		};

		httpContext.Response.StatusCode = statusCode;

		await httpContext.Response.WriteAsJsonAsync(
			problemDetails,
			cancellationToken);

		return true;
	}
}