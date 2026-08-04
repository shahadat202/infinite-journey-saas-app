using System.Diagnostics;
using FluentValidation;
using InfiniteJourney.Application.Common.Exceptions;
using InfiniteJourney.Application.Common.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace InfiniteJourney.Web.Middleware;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, errorCode, message, fieldErrors) = MapException(exception);

        if (statusCode >= 500)
            _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
        else
            _logger.LogWarning(exception, "Handled exception: {Message}", exception.Message);

        var response = new ApiErrorResponse
        {
            StatusCode = statusCode,
            Message = message,
            ErrorCode = errorCode,
            TraceId = Activity.Current?.Id ?? httpContext.TraceIdentifier,
            Errors = fieldErrors
        };

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";
        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

        return true;
    }

    private (int StatusCode, string ErrorCode, string Message, IReadOnlyList<ApiFieldError>? Errors) MapException(Exception exception)
    {
        return exception switch
        {
            AppException app => (app.StatusCode, app.ErrorCode, app.Message, null),
            ValidationException validation => (
                StatusCodes.Status400BadRequest,
                "VALIDATION_FAILED",
                "One or more validation errors occurred.",
                validation.Errors
                    .Select(e => new ApiFieldError(e.PropertyName, e.ErrorMessage))
                    .ToList()),
            KeyNotFoundException keyNotFound => (
                StatusCodes.Status404NotFound,
                "NOT_FOUND",
                keyNotFound.Message,
                null),
            UnauthorizedAccessException => (
                StatusCodes.Status401Unauthorized,
                "UNAUTHORIZED",
                "Authentication is required.",
                null),
            _ => (
                StatusCodes.Status500InternalServerError,
                "INTERNAL_ERROR",
                _environment.IsDevelopment() ? exception.Message : "An unexpected error occurred.",
                null)
        };
    }
}
