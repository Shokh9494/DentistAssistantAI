using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DentistAssistantAI.Core.Interfaces;

namespace DentistAssistantAI.App.ViewModels;

public partial class TeacherPageViewModel : ObservableObject
{
    private readonly IAIManager _aiManager;

    public TeacherPageViewModel(IAIManager aiManager)
    {
        _aiManager = aiManager;
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GenerateLectureCommand))]
    [NotifyCanExecuteChangedFor(nameof(GenerateTestCommand))]
    [NotifyCanExecuteChangedFor(nameof(GenerateTeacherCaseCommand))]
    public partial string Topic { get; set; } = string.Empty;

    [ObservableProperty] public partial int CourseYear { get; set; } = 3;
    [ObservableProperty] public partial int QuestionCount { get; set; } = 10;

    [ObservableProperty] public partial string GeneratedContent { get; set; } = string.Empty;
    [ObservableProperty] public partial string OutputTitle { get; set; } = string.Empty;
    [ObservableProperty] public partial bool HasContent { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GenerateLectureCommand))]
    [NotifyCanExecuteChangedFor(nameof(GenerateTestCommand))]
    [NotifyCanExecuteChangedFor(nameof(GenerateTeacherCaseCommand))]
    public partial bool IsBusy { get; set; }

    public List<int> Years { get; } = [1, 2, 3, 4, 5];

    private bool CanGenerate() => !IsBusy && !string.IsNullOrWhiteSpace(Topic);

    [RelayCommand(CanExecute = nameof(CanGenerate))]
    private async Task GenerateLectureAsync()
    {
        await RunGenerationAsync("📖 Lecture", () => _aiManager.GenerateLecture(Topic, CourseYear));
    }

    [RelayCommand(CanExecute = nameof(CanGenerate))]
    private async Task GenerateTestAsync()
    {
        await RunGenerationAsync("✅ Test", () => _aiManager.GenerateTest(Topic, CourseYear, QuestionCount));
    }

    [RelayCommand(CanExecute = nameof(CanGenerate))]
    private async Task GenerateTeacherCaseAsync()
    {
        await RunGenerationAsync("📋 Clinical Case", () => _aiManager.GenerateTeacherCase(Topic, CourseYear));
    }

    [RelayCommand]
    private async Task CopyContentAsync()
    {
        if (!string.IsNullOrEmpty(GeneratedContent))
            await Clipboard.SetTextAsync(GeneratedContent);
    }

    private async Task RunGenerationAsync(string title, Func<Task<string>> generate)
    {
        IsBusy = true;
        try
        {
            OutputTitle = title;
            GeneratedContent = string.Empty;
            HasContent = false;

            var result = await generate();
            GeneratedContent = result;
            HasContent = true;
        }
        catch (Exception ex)
        {
            GeneratedContent = $"Error: {ex.Message}";
            HasContent = true;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
