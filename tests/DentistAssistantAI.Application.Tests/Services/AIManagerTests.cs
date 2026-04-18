using DentistAssistantAI.Application.Services;
using DentistAssistantAI.Core.Configuration;
using DentistAssistantAI.Core.Interfaces;
using DentistAssistantAI.Core.Models;
using Xunit;

namespace DentistAssistantAI.Application.Tests.Services;

public sealed class AIManagerTests
{
    [Fact]
    public async Task AskDentistAI_ReturnsContent_WhenServiceSucceeds()
    {
        var openAIService = new FakeOpenAIService
        {
            Result = new AIResult
            {
                IsSuccess = true,
                Content = "Clinical answer"
            }
        };

        var manager = new AIManager(openAIService);

        var result = await manager.AskDentistAI("What is the diagnosis?");

        Assert.Equal("Clinical answer", result);
    }

    [Fact]
    public async Task AskDentistAI_ReturnsEmptyString_WhenContentIsNull()
    {
        var openAIService = new FakeOpenAIService
        {
            Result = new AIResult
            {
                IsSuccess = true,
                Content = null
            }
        };
        var manager = new AIManager(openAIService);

        var result = await manager.AskDentistAI("What is the diagnosis?");

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public async Task AskDentistAI_ReturnsQuotaMessage_WhenErrorContainsQuota()
    {
        var openAIService = new FakeOpenAIService
        {
            Result = new AIResult
            {
                IsSuccess = false,
                Error = "quota exceeded"
            }
        };
        var manager = new AIManager(openAIService);

        var result = await manager.AskDentistAI("What is the diagnosis?");

        Assert.Equal("AI quota exceeded. Please check billing.", result);
    }

    [Fact]
    public async Task AskDentistAI_ReturnsGenericMessage_WhenErrorDoesNotContainQuota()
    {
        var openAIService = new FakeOpenAIService
        {
            Result = new AIResult
            {
                IsSuccess = false,
                Error = "service unavailable"
            }
        };
        var manager = new AIManager(openAIService);

        var result = await manager.AskDentistAI("What is the diagnosis?");

        Assert.Equal("AI temporarily unavailable.", result);
    }

    [Fact]
    public async Task AskDentistAI_ReturnsGenericMessage_WhenQuotaErrorUsesDifferentCase()
    {
        var openAIService = new FakeOpenAIService
        {
            Result = new AIResult
            {
                IsSuccess = false,
                Error = "Quota exceeded"
            }
        };
        var manager = new AIManager(openAIService);

        var result = await manager.AskDentistAI("What is the diagnosis?");

        Assert.Equal("AI temporarily unavailable.", result);
    }

    [Fact]
    public async Task AskDentistAI_ForwardsQuestionAndImagePath_ToOpenAIService()
    {
        var openAIService = new FakeOpenAIService
        {
            Result = new AIResult
            {
                IsSuccess = true,
                Content = "Clinical answer"
            }
        };
        var manager = new AIManager(openAIService);

        await manager.AskDentistAI("Check image", "C:\\temp\\scan.jpg");

        Assert.Equal("Check image", openAIService.LastPrompt);
        Assert.Equal("C:\\temp\\scan.jpg", openAIService.LastImagePath);
    }

    [Fact]
    public async Task GenerateLecture_ReturnsContent_WhenServiceSucceeds()
    {
        var fake = new FakeOpenAIService { Result = new AIResult { IsSuccess = true, Content = "Lecture text" } };
        var manager = new AIManager(fake);

        var result = await manager.GenerateLecture("Glass ionomer cements", 3);

        Assert.Equal("Lecture text", result);
    }

    [Fact]
    public async Task GenerateLecture_UsesTeacherSystemPrompt()
    {
        var fake = new FakeOpenAIService { Result = new AIResult { IsSuccess = true, Content = "ok" } };
        var manager = new AIManager(fake);

        await manager.GenerateLecture("Composites", 2);

        Assert.Equal(EducationAIConfig.TeacherSystemPrompt, fake.LastSystemPrompt);
    }

    [Fact]
    public async Task GenerateTest_ReturnsContent_WhenServiceSucceeds()
    {
        var fake = new FakeOpenAIService { Result = new AIResult { IsSuccess = true, Content = "1. Question..." } };
        var manager = new AIManager(fake);

        var result = await manager.GenerateTest("Pulpitis", 3, 10);

        Assert.Equal("1. Question...", result);
    }

    [Fact]
    public async Task GenerateTest_ReturnsErrorMessage_WhenServiceFails()
    {
        var fake = new FakeOpenAIService { Result = new AIResult { IsSuccess = false, Error = "timeout" } };
        var manager = new AIManager(fake);

        var result = await manager.GenerateTest("Pulpitis", 3, 10);

        Assert.Equal("AI temporarily unavailable.", result);
    }

    [Fact]
    public async Task GenerateTeacherCase_ReturnsContent_WhenServiceSucceeds()
    {
        var fake = new FakeOpenAIService { Result = new AIResult { IsSuccess = true, Content = "Case: patient..." } };
        var manager = new AIManager(fake);

        var result = await manager.GenerateTeacherCase("Acute pulpitis", 4);

        Assert.Equal("Case: patient...", result);
    }

    [Fact]
    public async Task GenerateTeacherCase_UsesClinicalCaseSystemPrompt()
    {
        var fake = new FakeOpenAIService { Result = new AIResult { IsSuccess = true, Content = "ok" } };
        var manager = new AIManager(fake);

        await manager.GenerateTeacherCase("Periodontitis", 3);

        Assert.Equal(EducationAIConfig.ClinicalCaseSystemPrompt, fake.LastSystemPrompt);
    }

    [Fact]
    public async Task AskStudent_ReturnsContent_WhenServiceSucceeds()
    {
        var fake = new FakeOpenAIService { Result = new AIResult { IsSuccess = true, Content = "Simple explanation" } };
        var manager = new AIManager(fake);

        var result = await manager.AskStudent("What is GIC?", 2);

        Assert.Equal("Simple explanation", result);
    }

    [Fact]
    public async Task AskStudent_UsesStudentSystemPrompt()
    {
        var fake = new FakeOpenAIService { Result = new AIResult { IsSuccess = true, Content = "ok" } };
        var manager = new AIManager(fake);

        await manager.AskStudent("Explain caries", 1);

        Assert.Equal(EducationAIConfig.StudentSystemPrompt, fake.LastSystemPrompt);
    }

    [Fact]
    public async Task EvaluateStudentAnswer_ReturnsContent_WhenServiceSucceeds()
    {
        var fake = new FakeOpenAIService { Result = new AIResult { IsSuccess = true, Content = "✅ Correct diagnosis" } };
        var manager = new AIManager(fake);

        var result = await manager.EvaluateStudentAnswer("Patient case...", "Pulpitis", "RCT");

        Assert.Equal("✅ Correct diagnosis", result);
    }

    [Fact]
    public async Task EvaluateStudentAnswer_ReturnsQuotaMessage_WhenQuotaExceeded()
    {
        var fake = new FakeOpenAIService { Result = new AIResult { IsSuccess = false, Error = "quota exceeded" } };
        var manager = new AIManager(fake);

        var result = await manager.EvaluateStudentAnswer("case", "diagnosis", "treatment");

        Assert.Equal("AI quota exceeded. Please check billing.", result);
    }

    [Fact]
    public async Task GenerateStudentCase_ReturnsContent_WhenServiceSucceeds()
    {
        var fake = new FakeOpenAIService { Result = new AIResult { IsSuccess = true, Content = "## ЖАЛОБЫ\nБоль..." } };
        var manager = new AIManager(fake);

        var result = await manager.GenerateStudentCase("Periapical abscess", 3);

        Assert.Equal("## ЖАЛОБЫ\nБоль...", result);
    }

    private sealed class FakeOpenAIService : IOpenAIService
    {
        public AIResult Result { get; set; } = new();

        public string? LastPrompt { get; private set; }

        public string? LastImagePath { get; private set; }

        public string? LastSystemPrompt { get; private set; }

        public Task<AIResult> SendAsync(string prompt, string? imagePath = null, string? systemPrompt = null)
        {
            LastPrompt = prompt;
            LastImagePath = imagePath;
            LastSystemPrompt = systemPrompt;
            return Task.FromResult(Result);
        }
    }
}
