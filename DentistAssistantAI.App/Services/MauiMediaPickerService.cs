namespace DentistAssistantAI.App.Services
{
    public sealed class MauiMediaPickerService : IMediaPickerService
    {
        public bool IsCaptureSupported => MediaPicker.Default.IsCaptureSupported;

        public Task<FileResult?> PickPhotoAsync() => MediaPicker.Default.PickPhotoAsync();

        public Task<FileResult?> CapturePhotoAsync() => MediaPicker.Default.CapturePhotoAsync();
    }
}
