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
    private string _topic = string.Empty;

    [ObservableProperty] private int _courseYear = 3;
    [ObservableProperty] private int _questionCount = 10;

    [ObservableProperty] private string _generatedContent = string.Empty;
    [ObservableProperty] private string _outputTitle = string.Empty;
    [ObservableProperty] private bool _hasContent;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GenerateLectureCommand))]
    [NotifyCanExecuteChangedFor(nameof(GenerateTestCommand))]
    [NotifyCanExecuteChangedFor(nameof(GenerateTeacherCaseCommand))]
    private bool _isBusy;

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
