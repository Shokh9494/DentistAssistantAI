using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DentistAssistantAI.App.Models;
using DentistAssistantAI.App.Services;
using DentistAssistantAI.Core.Configuration;
using DentistAssistantAI.Core.Interfaces;
using System.Collections.ObjectModel;

namespace DentistAssistantAI.App.ViewModels
{
    public partial class MainPageViewModel : ObservableObject
    {
        private readonly IAIManager _aiManager;
        private readonly IMediaPickerService _mediaPickerService;
        private readonly IMediaFileCache _mediaFileCache;

        public MainPageViewModel(IAIManager aiManager)
            : this(aiManager, new MauiMediaPickerService(), new MediaFileCache())
        {
        }

        public MainPageViewModel(
            IAIManager aiManager,
            IMediaPickerService mediaPickerService,
            IMediaFileCache mediaFileCache)
        {
            _aiManager = aiManager;
            _mediaPickerService = mediaPickerService;
            _mediaFileCache = mediaFileCache;
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
        public partial string UserInput { get; set; } = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SendMessageCommand))]
        public partial bool IsBusy { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasPendingImage))]
        [NotifyPropertyChangedFor(nameof(PendingImageSource))]
        [NotifyCanExecuteChangedFor(nameof(SendMessageCommand))]
        public partial string? PendingImagePath { get; set; }

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
                var result = await _mediaPickerService.PickPhotoAsync();
                if (result is not null)
                    PendingImagePath = await _mediaFileCache.CopyToLocalCacheAsync(result);
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
                if (!_mediaPickerService.IsCaptureSupported)
                    return;

                var result = await _mediaPickerService.CapturePhotoAsync();
                if (result is not null)
                    PendingImagePath = await _mediaFileCache.CopyToLocalCacheAsync(result);
            }
            catch (Exception)
            {
                // User cancelled or permission denied
            }
        }

        [RelayCommand]
        private void ClearPendingImage() => PendingImagePath = null;
    }
}