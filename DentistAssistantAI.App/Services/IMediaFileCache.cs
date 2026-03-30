namespace DentistAssistantAI.App.Services
{
    public interface IMediaFileCache
    {
        Task<string?> CopyToLocalCacheAsync(FileResult file);
    }
}
