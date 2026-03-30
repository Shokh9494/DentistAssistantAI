namespace DentistAssistantAI.App.Services
{
    public sealed class MediaFileCache : IMediaFileCache
    {
        public async Task<string?> CopyToLocalCacheAsync(FileResult file)
        {
            try
            {
                var ext = Path.GetExtension(file.FileName).ToLowerInvariant() is ".png" ? ".png" : ".jpg";
                var localPath = Path.Combine(FileSystem.CacheDirectory, $"{Guid.NewGuid()}{ext}");

                using var source = await file.OpenReadAsync();
                using var destination = File.OpenWrite(localPath);
                await source.CopyToAsync(destination);

                return localPath;
            }
            catch
            {
                return null;
            }
        }
    }
}
