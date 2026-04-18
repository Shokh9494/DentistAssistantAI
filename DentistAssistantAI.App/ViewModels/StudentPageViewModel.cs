using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DentistAssistantAI.Core.Interfaces;

namespace DentistAssistantAI.App.ViewModels;

public partial class StudentPageViewModel : ObservableObject
{
    private readonly IAIManager _aiManager;

    public StudentPageViewModel(IAIManager aiManager)
    {
        _aiManager = aiManager;
    }

    // ── Ask a question ──
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AskStudentCommand))]
    private string _question = string.Empty;

    [ObservableProperty] private int _questionYear = 2;
    [ObservableProperty] private string _answerText = string.Empty;
    [ObservableProperty] private bool _hasAnswer;

    // ── Clinical case ──
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GenerateCaseCommand))]
    private string _caseTopic = string.Empty;

    [ObservableProperty] private int _caseYear = 3;
    [ObservableProperty] private string _caseText = string.Empty;
    [ObservableProperty] private bool _hasCase;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EvaluateCaseCommand))]
    private string _diagnosis = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EvaluateCaseCommand))]
    private string _treatment = string.Empty;

    [ObservableProperty] private string _feedbackText = string.Empty;
    [ObservableProperty] private bool _hasFeedback;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AskStudentCommand))]
    [NotifyCanExecuteChangedFor(nameof(GenerateCaseCommand))]
    [NotifyCanExecuteChangedFor(nameof(EvaluateCaseCommand))]
    private bool _isBusy;

    public List<int> Years { get; } = [1, 2, 3, 4, 5];

    private bool CanAsk() => !IsBusy && !string.IsNullOrWhiteSpace(Question);
    private bool CanGenerateCase() => !IsBusy && !string.IsNullOrWhiteSpace(CaseTopic);
    private bool CanEvaluate() => !IsBusy && HasCase
        && !string.IsNullOrWhiteSpace(Diagnosis)
        && !string.IsNullOrWhiteSpace(Treatment);

    [RelayCommand(CanExecute = nameof(CanAsk))]
    private async Task AskStudentAsync()
    {
        IsBusy = true;
        try
        {
            AnswerText = await _aiManager.AskStudent(Question, QuestionYear);
            HasAnswer = true;
        }
        catch (Exception ex)
        {
            AnswerText = $"Error: {ex.Message}";
            HasAnswer = true;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanGenerateCase))]
    private async Task GenerateCaseAsync()
    {
        IsBusy = true;
        try
        {
            Diagnosis = string.Empty;
            Treatment = string.Empty;
            FeedbackText = string.Empty;
            HasFeedback = false;
            HasCase = false;

            CaseText = await _aiManager.GenerateStudentCase(CaseTopic, CaseYear);
            HasCase = true;
        }
        catch (Exception ex)
        {
            CaseText = $"Error: {ex.Message}";
            HasCase = true;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanEvaluate))]
    private async Task EvaluateCaseAsync()
    {
        IsBusy = true;
        try
        {
            FeedbackText = await _aiManager.EvaluateStudentAnswer(CaseText, Diagnosis, Treatment);
            HasFeedback = true;
        }
        catch (Exception ex)
        {
            FeedbackText = $"Error: {ex.Message}";
            HasFeedback = true;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
