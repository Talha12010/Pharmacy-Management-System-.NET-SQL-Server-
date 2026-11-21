using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Domain.Exceptions;

namespace PharmacyManagement.API.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleAsync(context, exception);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception exception)
    {
        var (status, title) = exception switch
        {
            ValidationFailedException => (HttpStatusCode.BadRequest, "Validation failed"),
            BusinessRuleException => (HttpStatusCode.BadRequest, "Business rule violated"),
            UnauthorizedException => (HttpStatusCode.Unauthorized, "Unauthorized"),
            ForbiddenException => (HttpStatusCode.Forbidden, "Forbidden"),
            NotFoundException => (HttpStatusCode.NotFound, "Not found"),
            ConflictException or DbUpdateConcurrencyException => (HttpStatusCode.Conflict, "Conflict"),
            DbUpdateException => (HttpStatusCode.BadRequest, "Database update failed"),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred")
        };

        if (status == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Unhandled API exception");
        }
        else
        {
            _logger.LogWarning(exception, "API request failed with status {Status}", (int)status);
        }

        var problem = new ProblemDetails
        {
            Status = (int)status,
            Title = title,
            Detail = status == HttpStatusCode.InternalServerError ? "Please contact support." : exception.Message,
            Instance = context.Request.Path,
            Type = $"https://httpstatuses.com/{(int)status}"
        };
        problem.Extensions["traceId"] = context.TraceIdentifier;

        if (exception is ValidationFailedException validationException)
        {
            problem.Extensions["errors"] = validationException.Errors;
        }

        context.Response.StatusCode = (int)status;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(problem, JsonOptions);
    }
}
