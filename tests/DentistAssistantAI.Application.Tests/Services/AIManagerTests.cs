using DentistAssistantAI.Application.Services;
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

    private sealed class FakeOpenAIService : IOpenAIService
    {
        public AIResult Result { get; set; } = new();

        public string? LastPrompt { get; private set; }

        public string? LastImagePath { get; private set; }

        public Task<AIResult> SendAsync(string prompt, string? imagePath = null)
        {
            LastPrompt = prompt;
            LastImagePath = imagePath;
            return Task.FromResult(Result);
        }
    }
}
