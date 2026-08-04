namespace InfiniteJourney.Application.Common.Interfaces;

public enum FileCategory
{
    Images,
    Pdfs,
    Documents
}

public sealed record StoredFileResult(
    string Path,
    string FileName,
    string ContentType,
    long SizeBytes);

public interface IFileStorageService
{
    Task<StoredFileResult> SaveAsync(
        Guid tenantId,
        FileCategory category,
        string fileName,
        string contentType,
        Stream content,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(string path, CancellationToken cancellationToken = default);
}
