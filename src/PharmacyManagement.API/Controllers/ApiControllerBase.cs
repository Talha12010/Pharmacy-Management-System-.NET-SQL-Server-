using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagement.Domain.Exceptions;

namespace PharmacyManagement.API.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected CancellationToken Ct => HttpContext.RequestAborted;

    protected Guid CurrentUserId
    {
        get
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }

    protected async Task ValidateAsync<T>(T request)
    {
        if (request is null)
        {
            throw new ValidationFailedException("Request", "Request body is required.");
        }

        var validator = HttpContext.RequestServices.GetService(typeof(IValidator<T>)) as IValidator<T>;
        if (validator is null)
        {
            return;
        }

        var result = await validator.ValidateAsync(request, Ct);
        if (result.IsValid)
        {
            return;
        }

        var errors = result.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray(), StringComparer.OrdinalIgnoreCase);
        throw new ValidationFailedException("One or more validation errors occurred.", errors);
    }

    protected static async Task<(Stream Stream, string FileName)> ReadUploadAsync(IFormFile file)
    {
        if (file is null || file.Length == 0)
        {
            throw new ValidationFailedException("File", "A non-empty file is required.");
        }

        var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        stream.Position = 0;
        return (stream, Path.GetFileName(file.FileName));
    }
}

public sealed class ReasonRequest
{
    public string Reason { get; set; } = string.Empty;
}
