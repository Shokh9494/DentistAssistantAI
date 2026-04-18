using DentistAssistantAI.App.ViewModels;
using DentistAssistantAI.Core.Interfaces;
using Xunit;

namespace DentistAssistantAI.App.Tests.ViewModels;

public sealed class TeacherPageViewModelTests
{
    [Fact]
    public void GenerateLectureCommand_EmptyTopic_CannotExecute()
    {
        var vm = new TeacherPageViewModel(new FakeAIManager());

        Assert.False(vm.GenerateLectureCommand.CanExecute(null));
    }

    [Fact]
    public void GenerateTestCommand_EmptyTopic_CannotExecute()
    {
        var vm = new TeacherPageViewModel(new FakeAIManager());

        Assert.False(vm.GenerateTestCommand.CanExecute(null));
    }

    [Fact]
    public void GenerateTeacherCaseCommand_EmptyTopic_CannotExecute()
    {
        var vm = new TeacherPageViewModel(new FakeAIManager());

        Assert.False(vm.GenerateTeacherCaseCommand.CanExecute(null));
    }

    [Fact]
    public async Task GenerateLectureCommand_WithTopic_SetsGeneratedContent()
    {
        var ai = new FakeAIManager { LectureResponse = "Lecture on caries" };
        var vm = new TeacherPageViewModel(ai) { Topic = "Caries", CourseYear = 3 };

        await vm.GenerateLectureCommand.ExecuteAsync(null);

        Assert.Equal("Lecture on caries", vm.GeneratedContent);
        Assert.True(vm.HasContent);
        Assert.Equal("📖 Lecture", vm.OutputTitle);
        Assert.False(vm.IsBusy);
    }

    [Fact]
    public async Task GenerateTestCommand_PassesQuestionCount()
    {
        var ai = new FakeAIManager { TestResponse = "10 questions" };
        var vm = new TeacherPageViewModel(ai) { Topic = "Caries", QuestionCount = 15 };

        await vm.GenerateTestCommand.ExecuteAsync(null);

        Assert.Equal(15, ai.LastQuestionCount);
        Assert.Equal("10 questions", vm.GeneratedContent);
    }

    [Fact]
    public async Task GenerateTeacherCaseCommand_WithTopic_SetsGeneratedContent()
    {
        var ai = new FakeAIManager { TeacherCaseResponse = "Clinical case" };
        var vm = new TeacherPageViewModel(ai) { Topic = "Pulpitis" };

        await vm.GenerateTeacherCaseCommand.ExecuteAsync(null);

        Assert.Equal("Clinical case", vm.GeneratedContent);
        Assert.True(vm.HasContent);
        Assert.Equal("📋 Clinical Case", vm.OutputTitle);
    }

    [Fact]
    public async Task GenerateLectureCommand_OnException_ShowsErrorInContent()
    {
        var ai = new FakeAIManager { ExceptionToThrow = new HttpRequestException("Network error") };
        var vm = new TeacherPageViewModel(ai) { Topic = "Caries" };

        await vm.GenerateLectureCommand.ExecuteAsync(null);

        Assert.Contains("Error", vm.GeneratedContent);
        Assert.True(vm.HasContent);
        Assert.False(vm.IsBusy);
    }

    [Fact]
    public async Task GenerateLectureCommand_ClearsContentBeforeGenerating()
    {
        var ai = new FakeAIManager { LectureResponse = "New lecture" };
        var vm = new TeacherPageViewModel(ai)
        {
            Topic = "Topic",
            GeneratedContent = "Old content"
        };

        await vm.GenerateLectureCommand.ExecuteAsync(null);

        Assert.Equal("New lecture", vm.GeneratedContent);
    }

    [Fact]
    public void Years_ContainsFiveEntries()
    {
        var vm = new TeacherPageViewModel(new FakeAIManager());

        Assert.Equal([1, 2, 3, 4, 5], vm.Years);
    }

    private sealed class FakeAIManager : IAIManager
    {
        public string LectureResponse { get; set; } = string.Empty;
        public string TestResponse { get; set; } = string.Empty;
        public string TeacherCaseResponse { get; set; } = string.Empty;
        public Exception? ExceptionToThrow { get; set; }
        public int LastQuestionCount { get; private set; }

        public Task<string> AskDentistAI(string question, string? imagePath = null) => Task.FromResult(string.Empty);
        public Task<string> GenerateLecture(string topic, int courseYear)
        {
            if (ExceptionToThrow is not null) throw ExceptionToThrow;
            return Task.FromResult(LectureResponse);
        }
        public Task<string> GenerateTest(string topic, int courseYear, int questionCount = 10)
        {
            LastQuestionCount = questionCount;
            if (ExceptionToThrow is not null) throw ExceptionToThrow;
            return Task.FromResult(TestResponse);
        }
        public Task<string> GenerateTeacherCase(string topic, int courseYear)
        {
            if (ExceptionToThrow is not null) throw ExceptionToThrow;
            return Task.FromResult(TeacherCaseResponse);
        }
        public Task<string> GenerateStudentCase(string topic, int courseYear) => Task.FromResult(string.Empty);
        public Task<string> AskStudent(string question, int courseYear = 2) => Task.FromResult(string.Empty);
        public Task<string> EvaluateStudentAnswer(string caseText, string diagnosis, string treatment) => Task.FromResult(string.Empty);
    }
}
