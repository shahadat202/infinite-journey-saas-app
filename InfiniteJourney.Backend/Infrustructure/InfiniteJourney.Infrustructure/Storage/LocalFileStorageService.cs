using InfiniteJourney.Application.Common.Interfaces;
using InfiniteJourney.Infrustructure.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InfiniteJourney.Infrustructure.Storage;

public sealed class LocalFileStorageService : IFileStorageService
{
    private readonly string _rootPath;
    private readonly ILogger<LocalFileStorageService> _logger;

    public LocalFileStorageService(IOptions<StorageOptions> options, ILogger<LocalFileStorageService> logger)
    {
        _logger = logger;
        _rootPath = Path.GetFullPath(options.Value.RootPath);
        Directory.CreateDirectory(_rootPath);
    }

    public string RootPath => _rootPath;

    public async Task<StoredFileResult> SaveAsync(
        Guid tenantId,
        FileCategory category,
        string fileName,
        string contentType,
        Stream content,
        CancellationToken cancellationToken = default)
    {
        var folder = category switch
        {
            FileCategory.Images => "images",
            FileCategory.Pdfs => "pdfs",
            _ => "documents"
        };

        var extension = Path.GetExtension(Path.GetFileName(fileName));
        var storedName = $"{Guid.NewGuid():N}{extension}";
        var relativeDir = Path.Combine(tenantId.ToString("N"), folder);
        var absoluteDir = Path.Combine(_rootPath, relativeDir);

        Directory.CreateDirectory(absoluteDir);

        var absolutePath = Path.Combine(absoluteDir, storedName);
        await using var fileStream = new FileStream(absolutePath, FileMode.CreateNew, FileAccess.Write);
        await content.CopyToAsync(fileStream, cancellationToken);

        var publicPath = $"/uploads/{tenantId:N}/{folder}/{storedName}";
        var size = new FileInfo(absolutePath).Length;

        _logger.LogInformation("Stored file {Path} ({Size} bytes)", publicPath, size);

        return new StoredFileResult(publicPath, storedName, contentType, size);
    }

    public Task DeleteAsync(string path, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(path) || !path.StartsWith("/uploads/", StringComparison.Ordinal))
            return Task.CompletedTask;

        var relative = path["/uploads/".Length..].Replace('/', Path.DirectorySeparatorChar);
        var absolute = Path.Combine(_rootPath, relative);

        if (File.Exists(absolute))
            File.Delete(absolute);

        return Task.CompletedTask;
    }
}
