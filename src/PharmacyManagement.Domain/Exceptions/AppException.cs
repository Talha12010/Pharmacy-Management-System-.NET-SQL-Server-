namespace PharmacyManagement.Domain.Exceptions;

public abstract class AppException : Exception
{
    protected AppException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}

public sealed class NotFoundException : AppException
{
    public NotFoundException(string message) : base(message) { }

    public NotFoundException(string entityName, object key)
        : base($"{entityName} with identifier '{key}' was not found.") { }
}

public sealed class BusinessRuleException : AppException
{
    public BusinessRuleException(string message) : base(message) { }
}

public sealed class ConflictException : AppException
{
    public ConflictException(string message, Exception? innerException = null) : base(message, innerException) { }
}

public sealed class ForbiddenException : AppException
{
    public ForbiddenException(string message = "You do not have permission to perform this action.") : base(message) { }
}

public sealed class UnauthorizedException : AppException
{
    public UnauthorizedException(string message = "Invalid credentials.") : base(message) { }
}

public sealed class ValidationFailedException : AppException
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public ValidationFailedException(string message, IReadOnlyDictionary<string, string[]> errors)
        : base(message)
    {
        Errors = errors;
    }

    public ValidationFailedException(string field, string error)
        : base(error)
    {
        Errors = new Dictionary<string, string[]> { [field] = new[] { error } };
    }
}
