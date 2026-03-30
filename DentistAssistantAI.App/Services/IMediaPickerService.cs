namespace DentistAssistantAI.App.Services
{
    public interface IMediaPickerService
    {
        bool IsCaptureSupported { get; }

        Task<FileResult?> PickPhotoAsync();

        Task<FileResult?> CapturePhotoAsync();
    }
}
