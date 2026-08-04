using InfiniteJourney.Application.Common.Abstractions;
using InfiniteJourney.Application.Common.Interfaces;
using FluentValidation;

namespace InfiniteJourney.Application.Files.Commands;

public sealed record UploadFileCommand(
    string FileName,
    string ContentType,
    string Base64Data,
    FileCategory Category) : ICommand<UploadFileResultDto>;

public sealed record UploadFileResultDto(
    string Path,
    string FileName,
    string ContentType,
    long SizeBytes);

public sealed class UploadFileCommandHandler : ICommandHandler<UploadFileCommand, UploadFileResultDto>
{
    private readonly IFileStorageService _storage;
    private readonly ITenantContext _tenantContext;

    public UploadFileCommandHandler(IFileStorageService storage, ITenantContext tenantContext)
    {
        _storage = storage;
        _tenantContext = tenantContext;
    }

    public async Task<UploadFileResultDto> Handle(UploadFileCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.IsResolved)
            throw new InvalidOperationException("Tenant context is not resolved.");

        var base64 = request.Base64Data;
        var commaIndex = base64.IndexOf(',');
        if (commaIndex >= 0)
            base64 = base64[(commaIndex + 1)..];

        var bytes = Convert.FromBase64String(base64);
        await using var stream = new MemoryStream(bytes);

        var result = await _storage.SaveAsync(
            _tenantContext.TenantId,
            request.Category,
            request.FileName,
            request.ContentType,
            stream,
            cancellationToken);

        return new UploadFileResultDto(result.Path, result.FileName, result.ContentType, result.SizeBytes);
    }
}

public sealed class UploadFileCommandValidator : AbstractValidator<UploadFileCommand>
{
    private const int MaxImageBytes = 5 * 1024 * 1024;
    private const int MaxPdfBytes = 10 * 1024 * 1024;

    public UploadFileCommandValidator()
    {
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(255);
        RuleFor(x => x.ContentType).NotEmpty();
        RuleFor(x => x.Base64Data).NotEmpty();
        RuleFor(x => x).Must(BeWithinSizeLimit).WithMessage("File exceeds the maximum allowed size.");
    }

    private static bool BeWithinSizeLimit(UploadFileCommand command)
    {
        try
        {
            var base64 = command.Base64Data;
            var commaIndex = base64.IndexOf(',');
            if (commaIndex >= 0) base64 = base64[(commaIndex + 1)..];
            var bytes = Convert.FromBase64String(base64);
            var max = command.Category == FileCategory.Pdfs ? MaxPdfBytes : MaxImageBytes;
            return bytes.Length <= max;
        }
        catch
        {
            return false;
        }
    }
}
