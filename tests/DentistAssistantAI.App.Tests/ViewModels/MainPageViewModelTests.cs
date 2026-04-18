using DentistAssistantAI.App.ViewModels;
using DentistAssistantAI.App.Services;
using DentistAssistantAI.Core.Configuration;
using DentistAssistantAI.Core.Interfaces;
using Xunit;

namespace DentistAssistantAI.App.Tests.ViewModels;

public sealed class MainPageViewModelTests
{
    [Fact]
    public void Constructor_SeedsWelcomeMessage()
    {
        var viewModel = CreateViewModel(new FakeAIManager());

        Assert.Single(viewModel.Messages);
        Assert.False(viewModel.Messages[0].IsFromUser);
        Assert.Contains("DentAI", viewModel.Messages[0].Text);
    }

    [Fact]
    public void SendMessageCommand_NoTextAndNoImage_CannotExecute()
    {
        var viewModel = CreateViewModel(new FakeAIManager());

        Assert.False(viewModel.SendMessageCommand.CanExecute(null));
    }

    [Fact]
    public void SendMessageCommand_TextPresent_CanExecute()
    {
        var viewModel = CreateViewModel(new FakeAIManager());
        viewModel.UserInput = "Question";

        Assert.True(viewModel.SendMessageCommand.CanExecute(null));
    }

    [Fact]
    public void SendMessageCommand_PendingImagePresent_CanExecute()
    {
        var viewModel = CreateViewModel(new FakeAIManager());
        viewModel.PendingImagePath = "C:\\temp\\scan.jpg";

        Assert.True(viewModel.SendMessageCommand.CanExecute(null));
    }

    [Fact]
    public async Task SendMessageAsync_TextInput_TrimmedAndMessagesAppendedInOrder()
    {
        var aiManager = new FakeAIManager { Response = "AI answer" };
        var viewModel = CreateViewModel(aiManager);
        viewModel.UserInput = "  Need diagnosis  ";
        viewModel.PendingImagePath = "C:\\temp\\scan.jpg";

        await viewModel.SendMessageCommand.ExecuteAsync(null);

        Assert.Equal("Need diagnosis", aiManager.LastQuestion);
        Assert.Equal("C:\\temp\\scan.jpg", aiManager.LastImagePath);
        Assert.Equal(string.Empty, viewModel.UserInput);
        Assert.Null(viewModel.PendingImagePath);
        Assert.False(viewModel.IsBusy);

        Assert.Equal(3, viewModel.Messages.Count);
        Assert.False(viewModel.Messages[0].IsFromUser);
        Assert.True(viewModel.Messages[1].IsFromUser);
        Assert.Equal("Need diagnosis", viewModel.Messages[1].Text);
        Assert.Equal("C:\\temp\\scan.jpg", viewModel.Messages[1].ImagePath);
        Assert.False(viewModel.Messages[2].IsFromUser);
        Assert.Equal("AI answer", viewModel.Messages[2].Text);
    }

    [Fact]
    public async Task SendMessageAsync_ImageOnly_UsesDefaultImagePrompt()
    {
        var aiManager = new FakeAIManager { Response = "AI image answer" };
        var viewModel = CreateViewModel(aiManager);
        viewModel.PendingImagePath = "C:\\temp\\scan.jpg";

        await viewModel.SendMessageCommand.ExecuteAsync(null);

        Assert.Equal(DentalAIConfig.DefaultImagePrompt, aiManager.LastQuestion);
        Assert.Equal("C:\\temp\\scan.jpg", aiManager.LastImagePath);
        Assert.Equal(string.Empty, viewModel.Messages[1].Text);
        Assert.Equal("AI image answer", viewModel.Messages[2].Text);
        Assert.False(viewModel.IsBusy);
    }

    [Fact]
    public async Task SendMessageAsync_ServiceThrows_AppendsErrorMessageAndResetsBusyState()
    {
        var aiManager = new FakeAIManager
        {
            ExceptionToThrow = new InvalidOperationException("send failed")
        };
        var viewModel = CreateViewModel(aiManager);
        viewModel.UserInput = "Question";

        await viewModel.SendMessageCommand.ExecuteAsync(null);

        Assert.False(viewModel.IsBusy);
        Assert.Equal(3, viewModel.Messages.Count);
        Assert.True(viewModel.Messages[1].IsFromUser);
        Assert.False(viewModel.Messages[2].IsFromUser);
        Assert.Equal("Error: send failed", viewModel.Messages[2].Text);
    }

    [Fact]
    public void ClearPendingImageCommand_ClearsPendingImage()
    {
        var viewModel = CreateViewModel(new FakeAIManager());
        viewModel.PendingImagePath = "C:\\temp\\scan.jpg";

        viewModel.ClearPendingImageCommand.Execute(null);

        Assert.Null(viewModel.PendingImagePath);
    }

    [Fact]
    public async Task PickPhotoAsync_FileReturned_SetsPendingImagePath()
    {
        var mediaPickerService = new FakeMediaPickerService
        {
            PickPhotoResult = new FileResult("picked.jpg")
        };
        var mediaFileCache = new FakeMediaFileCache
        {
            ResultPath = "C:\\cache\\picked.jpg"
        };
        var viewModel = CreateViewModel(new FakeAIManager(), mediaPickerService, mediaFileCache);

        await viewModel.PickPhotoCommand.ExecuteAsync(null);

        Assert.Equal("C:\\cache\\picked.jpg", viewModel.PendingImagePath);
        Assert.Equal(1, mediaPickerService.PickPhotoCallCount);
        Assert.Equal(1, mediaFileCache.CopyCallCount);
        Assert.Equal("picked.jpg", mediaFileCache.LastFile?.FileName);
    }

    [Fact]
    public async Task TakePhotoAsync_CaptureSupportedAndFileReturned_SetsPendingImagePath()
    {
        var mediaPickerService = new FakeMediaPickerService
        {
            IsCaptureSupported = true,
            CapturePhotoResult = new FileResult("captured.png")
        };
        var mediaFileCache = new FakeMediaFileCache
        {
            ResultPath = "C:\\cache\\captured.png"
        };
        var viewModel = CreateViewModel(new FakeAIManager(), mediaPickerService, mediaFileCache);

        await viewModel.TakePhotoCommand.ExecuteAsync(null);

        Assert.Equal("C:\\cache\\captured.png", viewModel.PendingImagePath);
        Assert.Equal(1, mediaPickerService.CapturePhotoCallCount);
        Assert.Equal(1, mediaFileCache.CopyCallCount);
    }

    [Fact]
    public async Task TakePhotoAsync_CaptureNotSupported_DoesNothing()
    {
        var mediaPickerService = new FakeMediaPickerService { IsCaptureSupported = false };
        var mediaFileCache = new FakeMediaFileCache();
        var viewModel = CreateViewModel(new FakeAIManager(), mediaPickerService, mediaFileCache);
        viewModel.PendingImagePath = "C:\\existing\\image.jpg";

        await viewModel.TakePhotoCommand.ExecuteAsync(null);

        Assert.Equal("C:\\existing\\image.jpg", viewModel.PendingImagePath);
        Assert.Equal(0, mediaPickerService.CapturePhotoCallCount);
        Assert.Equal(0, mediaFileCache.CopyCallCount);
    }

    [Fact]
    public async Task PickPhotoAsync_NullResult_DoesNotChangePendingImagePath()
    {
        var mediaPickerService = new FakeMediaPickerService();
        var mediaFileCache = new FakeMediaFileCache();
        var viewModel = CreateViewModel(new FakeAIManager(), mediaPickerService, mediaFileCache);
        viewModel.PendingImagePath = "C:\\existing\\image.jpg";

        await viewModel.PickPhotoCommand.ExecuteAsync(null);

        Assert.Equal("C:\\existing\\image.jpg", viewModel.PendingImagePath);
        Assert.Equal(1, mediaPickerService.PickPhotoCallCount);
        Assert.Equal(0, mediaFileCache.CopyCallCount);
    }

    [Fact]
    public async Task PickPhotoAsync_Exception_IsSwallowed()
    {
        var mediaPickerService = new FakeMediaPickerService
        {
            PickPhotoException = new InvalidOperationException("pick failed")
        };
        var mediaFileCache = new FakeMediaFileCache();
        var viewModel = CreateViewModel(new FakeAIManager(), mediaPickerService, mediaFileCache);
        viewModel.PendingImagePath = "C:\\existing\\image.jpg";

        await viewModel.PickPhotoCommand.ExecuteAsync(null);

        Assert.Equal("C:\\existing\\image.jpg", viewModel.PendingImagePath);
        Assert.Equal(1, mediaPickerService.PickPhotoCallCount);
        Assert.Equal(0, mediaFileCache.CopyCallCount);
    }

    [Fact]
    public async Task TakePhotoAsync_Exception_IsSwallowed()
    {
        var mediaPickerService = new FakeMediaPickerService
        {
            IsCaptureSupported = true,
            CapturePhotoException = new InvalidOperationException("capture failed")
        };
        var mediaFileCache = new FakeMediaFileCache();
        var viewModel = CreateViewModel(new FakeAIManager(), mediaPickerService, mediaFileCache);
        viewModel.PendingImagePath = "C:\\existing\\image.jpg";

        await viewModel.TakePhotoCommand.ExecuteAsync(null);

        Assert.Equal("C:\\existing\\image.jpg", viewModel.PendingImagePath);
        Assert.Equal(1, mediaPickerService.CapturePhotoCallCount);
        Assert.Equal(0, mediaFileCache.CopyCallCount);
    }

    private static MainPageViewModel CreateViewModel(
        FakeAIManager aiManager,
        FakeMediaPickerService? mediaPickerService = null,
        FakeMediaFileCache? mediaFileCache = null)
    {
        return new MainPageViewModel(
            aiManager,
            mediaPickerService ?? new FakeMediaPickerService(),
            mediaFileCache ?? new FakeMediaFileCache());
    }

    private sealed class FakeAIManager : IAIManager
    {
        public string Response { get; set; } = string.Empty;
        public Exception? ExceptionToThrow { get; set; }
        public string? LastQuestion { get; private set; }
        public string? LastImagePath { get; private set; }

        public Task<string> AskDentistAI(string question, string? imagePath = null)
        {
            LastQuestion = question;
            LastImagePath = imagePath;

            if (ExceptionToThrow is not null)
                throw ExceptionToThrow;

            return Task.FromResult(Response);
        }

        public Task<string> GenerateLecture(string topic, int courseYear) => Task.FromResult(string.Empty);
        public Task<string> GenerateTest(string topic, int courseYear, int questionCount = 10) => Task.FromResult(string.Empty);
        public Task<string> GenerateTeacherCase(string topic, int courseYear) => Task.FromResult(string.Empty);
        public Task<string> GenerateStudentCase(string topic, int courseYear) => Task.FromResult(string.Empty);
        public Task<string> AskStudent(string question, int courseYear = 2) => Task.FromResult(string.Empty);
        public Task<string> EvaluateStudentAnswer(string caseText, string diagnosis, string treatment) => Task.FromResult(string.Empty);
    }

    private sealed class FakeMediaPickerService : IMediaPickerService
    {
        public bool IsCaptureSupported { get; set; }
        public FileResult? PickPhotoResult { get; set; }
        public FileResult? CapturePhotoResult { get; set; }
        public Exception? PickPhotoException { get; set; }
        public Exception? CapturePhotoException { get; set; }
        public int PickPhotoCallCount { get; private set; }
        public int CapturePhotoCallCount { get; private set; }

        public Task<FileResult?> PickPhotoAsync()
        {
            PickPhotoCallCount++;
            if (PickPhotoException is not null) throw PickPhotoException;
            return Task.FromResult(PickPhotoResult);
        }

        public Task<FileResult?> CapturePhotoAsync()
        {
            CapturePhotoCallCount++;
            if (CapturePhotoException is not null) throw CapturePhotoException;
            return Task.FromResult(CapturePhotoResult);
        }
    }

    private sealed class FakeMediaFileCache : IMediaFileCache
    {
        public string? ResultPath { get; set; }
        public FileResult? LastFile { get; private set; }
        public int CopyCallCount { get; private set; }

        public Task<string?> CopyToLocalCacheAsync(FileResult file)
        {
            CopyCallCount++;
            LastFile = file;
            return Task.FromResult(ResultPath);
        }
    }
}
