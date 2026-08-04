namespace InfiniteJourney.Infrustructure.Storage;

public sealed class StorageOptions
{
    public const string SectionName = "Storage";
    public string RootPath { get; set; } = "UPLOADED_DATA";
}
