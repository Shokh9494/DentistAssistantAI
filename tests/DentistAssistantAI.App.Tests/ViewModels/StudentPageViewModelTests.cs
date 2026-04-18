using DentistAssistantAI.App.ViewModels;
using DentistAssistantAI.Core.Interfaces;
using Xunit;

namespace DentistAssistantAI.App.Tests.ViewModels;

public sealed class StudentPageViewModelTests
{
    [Fact]
    public void AskStudentCommand_EmptyQuestion_CannotExecute()
    {
        var vm = new StudentPageViewModel(new FakeAIManager());

        Assert.False(vm.AskStudentCommand.CanExecute(null));
    }

    [Fact]
    public async Task AskStudentCommand_WithQuestion_SetsAnswerTextAndHasAnswer()
    {
        var ai = new FakeAIManager { StudentAnswer = "Pulpitis is inflammation..." };
        var vm = new StudentPageViewModel(ai) { Question = "What is pulpitis?", QuestionYear = 2 };

        await vm.AskStudentCommand.ExecuteAsync(null);

        Assert.Equal("Pulpitis is inflammation...", vm.AnswerText);
        Assert.True(vm.HasAnswer);
        Assert.False(vm.IsBusy);
        Assert.Equal("What is pulpitis?", ai.LastQuestion);
        Assert.Equal(2, ai.LastQuestionYear);
    }

    [Fact]
    public async Task AskStudentCommand_OnException_ShowsErrorAndSetsHasAnswer()
    {
        var ai = new FakeAIManager { ExceptionToThrow = new HttpRequestException("timeout") };
        var vm = new StudentPageViewModel(ai) { Question = "test" };

        await vm.AskStudentCommand.ExecuteAsync(null);

        Assert.Contains("Error", vm.AnswerText);
        Assert.True(vm.HasAnswer);
        Assert.False(vm.IsBusy);
    }

    [Fact]
    public void GenerateCaseCommand_EmptyTopic_CannotExecute()
    {
        var vm = new StudentPageViewModel(new FakeAIManager());

        Assert.False(vm.GenerateCaseCommand.CanExecute(null));
    }

    [Fact]
    public async Task GenerateCaseCommand_WithTopic_SetsCaseTextAndHasCase()
    {
        var ai = new FakeAIManager { CaseText = "Patient presents with..." };
        var vm = new StudentPageViewModel(ai) { CaseTopic = "Pulpitis", CaseYear = 3 };

        await vm.GenerateCaseCommand.ExecuteAsync(null);

        Assert.Equal("Patient presents with...", vm.CaseText);
        Assert.True(vm.HasCase);
        Assert.False(vm.IsBusy);
    }

    [Fact]
    public async Task GenerateCaseCommand_ClearsPreviousFeedback()
    {
        var ai = new FakeAIManager { CaseText = "New case" };
        var vm = new StudentPageViewModel(ai)
        {
            CaseTopic = "Topic",
            FeedbackText = "Old feedback",
            HasFeedback = true,
            Diagnosis = "Old diagnosis",
            Treatment = "Old treatment"
        };

        await vm.GenerateCaseCommand.ExecuteAsync(null);

        Assert.Empty(vm.Diagnosis);
        Assert.Empty(vm.Treatment);
        Assert.Empty(vm.FeedbackText);
        Assert.False(vm.HasFeedback);
    }

    [Fact]
    public void EvaluateCaseCommand_WhenHasCaseFalse_CannotExecute()
    {
        var vm = new StudentPageViewModel(new FakeAIManager())
        {
            Diagnosis = "Some diagnosis",
            Treatment = "Some treatment"
        };

        Assert.False(vm.EvaluateCaseCommand.CanExecute(null));
    }

    [Fact]
    public void EvaluateCaseCommand_EmptyDiagnosis_CannotExecute()
    {
        var vm = new StudentPageViewModel(new FakeAIManager())
        {
            HasCase = true,
            Treatment = "Some treatment"
        };

        Assert.False(vm.EvaluateCaseCommand.CanExecute(null));
    }

    [Fact]
    public void EvaluateCaseCommand_EmptyTreatment_CannotExecute()
    {
        var vm = new StudentPageViewModel(new FakeAIManager())
        {
            HasCase = true,
            Diagnosis = "Some diagnosis"
        };

        Assert.False(vm.EvaluateCaseCommand.CanExecute(null));
    }

    [Fact]
    public async Task EvaluateCaseCommand_AllFieldsPresent_SetsFeedback()
    {
        var ai = new FakeAIManager { FeedbackText = "Good diagnosis! Score: 8/10" };
        var vm = new StudentPageViewModel(ai)
        {
            HasCase = true,
            CaseText = "Patient case...",
            Diagnosis = "Pulpitis acuta",
            Treatment = "Root canal therapy"
        };

        await vm.EvaluateCaseCommand.ExecuteAsync(null);

        Assert.Equal("Good diagnosis! Score: 8/10", vm.FeedbackText);
        Assert.True(vm.HasFeedback);
        Assert.False(vm.IsBusy);
        Assert.Equal("Patient case...", ai.LastCaseText);
        Assert.Equal("Pulpitis acuta", ai.LastDiagnosis);
        Assert.Equal("Root canal therapy", ai.LastTreatment);
    }

    [Fact]
    public void Years_ContainsFiveEntries()
    {
        var vm = new StudentPageViewModel(new FakeAIManager());

        Assert.Equal([1, 2, 3, 4, 5], vm.Years);
    }

    private sealed class FakeAIManager : IAIManager
    {
        public string StudentAnswer { get; set; } = string.Empty;
        public string CaseText { get; set; } = string.Empty;
        public string FeedbackText { get; set; } = string.Empty;
        public Exception? ExceptionToThrow { get; set; }
        public string? LastQuestion { get; private set; }
        public int LastQuestionYear { get; private set; }
        public string? LastCaseText { get; private set; }
        public string? LastDiagnosis { get; private set; }
        public string? LastTreatment { get; private set; }

        public Task<string> AskDentistAI(string question, string? imagePath = null) => Task.FromResult(string.Empty);
        public Task<string> GenerateLecture(string topic, int courseYear) => Task.FromResult(string.Empty);
        public Task<string> GenerateTest(string topic, int courseYear, int questionCount = 10) => Task.FromResult(string.Empty);
        public Task<string> GenerateTeacherCase(string topic, int courseYear) => Task.FromResult(string.Empty);
        public Task<string> GenerateStudentCase(string topic, int courseYear)
        {
            if (ExceptionToThrow is not null) throw ExceptionToThrow;
            return Task.FromResult(CaseText);
        }
        public Task<string> AskStudent(string question, int courseYear = 2)
        {
            LastQuestion = question;
            LastQuestionYear = courseYear;
            if (ExceptionToThrow is not null) throw ExceptionToThrow;
            return Task.FromResult(StudentAnswer);
        }
        public Task<string> EvaluateStudentAnswer(string caseText, string diagnosis, string treatment)
        {
            LastCaseText = caseText;
            LastDiagnosis = diagnosis;
            LastTreatment = treatment;
            return Task.FromResult(FeedbackText);
        }
    }
}
