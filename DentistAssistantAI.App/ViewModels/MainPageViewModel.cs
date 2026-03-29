using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DentistAssistantAI.App.Models;
using DentistAssistantAI.Application.Services;
using DentistAssistantAI.Core.Configuration;
using System.Collections.ObjectModel;

namespace DentistAssistantAI.App.ViewModels
{
    public partial class MainPageViewModel : ObservableObject
    {
        private readonly AIManager _aiManager;

        public MainPageViewModel(AIManager aiManager)
        {
            _aiManager = aiManager;
            Messages.Add(new ChatMessageItem(
                """
                Salom! Men DentAI — professional stomatologiya bo'yicha klinik sun'iy intellektingizman. 🦷

                Quyidagilarda yordam bera olaman:
                • 📷 Rentgen va klinik rasmlarni tahlil qilish
                • 🔍 Tashxis va davolash rejalashtirishda yordam
                • 💊 Farmakologiya va klinik protokollar
                • 📋 Parodontologiya, endodontiya, jarrohlik va boshqalar

                Rentgen yoki foto yuboring yoki savolingizni yozing.
                """,
                isFromUser: false));
        }

        public ObservableCollection<ChatMessageItem> Messages { get; } = [];

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SendMessageCommand))]
        private string _userInput = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SendMessageCommand))]
        private bool _isBusy;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasPendingImage))]
        [NotifyPropertyChangedFor(nameof(PendingImageSource))]
        [NotifyCanExecuteChangedFor(nameof(SendMessageCommand))]
        private string? _pendingImagePath;

        public bool HasPendingImage => !string.IsNullOrEmpty(PendingImagePath);
        public ImageSource? PendingImageSource =>
            PendingImagePath != null ? ImageSource.FromFile(PendingImagePath) : null;

        private bool CanSendMessage() =>
            !IsBusy && (!string.IsNullOrWhiteSpace(UserInput) || HasPendingImage);

        [RelayCommand(CanExecute = nameof(CanSendMessage))]
        private async Task SendMessageAsync()
        {
            var text = UserInput.Trim();
            var imagePath = PendingImagePath;

            UserInput = string.Empty;
            PendingImagePath = null;

            Messages.Add(new ChatMessageItem(text, isFromUser: true, imagePath: imagePath));
            IsBusy = true;

            try
            {
                // When only an image is sent, fall back to the structured analysis default prompt
                var prompt = string.IsNullOrEmpty(text)
                    ? DentalAIConfig.DefaultImagePrompt
                    : text;

                var answer = await _aiManager.AskDentistAI(prompt, imagePath);
                Messages.Add(new ChatMessageItem(answer, isFromUser: false));
            }
            catch (Exception ex)
            {
                Messages.Add(new ChatMessageItem($"Error: {ex.Message}", isFromUser: false));
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task PickPhotoAsync()
        {
            try
            {
                var result = await MediaPicker.Default.PickPhotoAsync();
                if (result is not null)
                    PendingImagePath = await CopyToLocalCacheAsync(result);
            }
            catch (Exception)
            {
                // User cancelled or permission denied
            }
        }

        [RelayCommand]
        private async Task TakePhotoAsync()
        {
            try
            {
                if (!MediaPicker.Default.IsCaptureSupported)
                    return;

                var result = await MediaPicker.Default.CapturePhotoAsync();
                if (result is not null)
                    PendingImagePath = await CopyToLocalCacheAsync(result);
            }
            catch (Exception)
            {
                // User cancelled or permission denied
            }
        }

        [RelayCommand]
        private void ClearPendingImage() => PendingImagePath = null;

        /// <summary>
        /// Copies the picked/captured file into the app's cache directory using
        /// OpenReadAsync — the only cross-platform safe way to read MediaPicker results
        /// on Android (content URIs) and iOS (sandboxed paths).
        /// </summary>
        private static async Task<string?> CopyToLocalCacheAsync(FileResult file)
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