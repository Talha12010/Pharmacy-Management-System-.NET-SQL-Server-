using Microsoft.Extensions.Configuration;
using PharmacyManagement.Application.Common.Interfaces;
using PharmacyManagement.Domain.Exceptions;

namespace PharmacyManagement.Infrastructure.Services;

public sealed class FileStorageService : IFileStorageService
{
    private readonly IConfiguration _configuration;

    public FileStorageService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<string> SaveAsync(
        Stream content,
        string originalFileName,
        string folder,
        CancellationToken cancellationToken = default)
    {
        var safeFolder = SanitizeFolder(folder);
        var extension = Path.GetExtension(originalFileName);
        if (extension.Length > 10)
        {
            extension = extension[..10];
        }

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var directory = Path.Combine(RootPath, safeFolder);
        Directory.CreateDirectory(directory);
        var fullPath = Path.Combine(directory, fileName);

        await using var fileStream = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        await content.CopyToAsync(fileStream, cancellationToken);
        await fileStream.FlushAsync(cancellationToken);

        return $"{safeFolder}/{fileName}";
    }

    public Task<Stream> OpenReadAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        var fullPath = ResolvePath(relativePath);
        if (!File.Exists(fullPath))
        {
            throw new NotFoundException("File", relativePath);
        }

        Stream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string relativePath, CancellationToken cancellationToken = default)
    {
        var fullPath = ResolvePath(relativePath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    private string RootPath
    {
        get
        {
            var configured = _configuration["Storage:RootPath"];
            var root = string.IsNullOrWhiteSpace(configured)
                ? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads")
                : configured;
            return Path.TrimEndingDirectorySeparator(Path.GetFullPath(root));
        }
    }

    private string ResolvePath(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            throw new BusinessRuleException("File path is required.");
        }

        var fullPath = Path.GetFullPath(Path.Combine(RootPath, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        if (!fullPath.StartsWith(RootPath + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            throw new ForbiddenException("Invalid file path.");
        }

        return fullPath;
    }

    private static string SanitizeFolder(string folder)
    {
        var safe = new string((folder ?? "uploads")
            .Select(c => char.IsLetterOrDigit(c) || c is '-' or '_' ? c : '-')
            .ToArray());
        return string.IsNullOrWhiteSpace(safe) ? "uploads" : safe;
    }
}
