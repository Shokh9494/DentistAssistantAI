using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using DentistAssistantAI.Core.Configuration;
using DentistAssistantAI.Infrastructure.Services;
using DentistAssistantAI.Infrastructure.Tests.TestDoubles;
using Xunit;

namespace DentistAssistantAI.Infrastructure.Tests.Services;

public sealed class OpenAIServiceTests
{
    [Fact]
    public async Task SendAsync_TextOnlyRequest_PostsExpectedPayloadAndParsesResponse()
    {
        var handler = new StubHttpMessageHandler((request, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                {
                  "choices": [
                    {
                      "message": {
                        "content": "Structured clinical answer"
                      }
                    }
                  ]
                }
                """, Encoding.UTF8, "application/json")
            }));

        var service = CreateService(handler, "test-api-key");

        var result = await service.SendAsync("What do you see?");

        Assert.True(result.IsSuccess);
        Assert.Equal("Structured clinical answer", result.Content);

        var request = AssertRequest(handler);
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("https://api.openai.com/v1/chat/completions", request.RequestUri?.ToString());
        Assert.Equal("Bearer", request.Headers.Authorization?.Scheme);
        Assert.Equal("test-api-key", request.Headers.Authorization?.Parameter);

        using var document = await ReadRequestJsonAsync(request);
        var root = document.RootElement;

        Assert.Equal(DentalAIConfig.TextModel, root.GetProperty("model").GetString());
        var messages = root.GetProperty("messages");
        Assert.Equal(DentalAIConfig.SystemPrompt, messages[0].GetProperty("content").GetString());
        Assert.Equal("system", messages[0].GetProperty("role").GetString());
        Assert.Equal("user", messages[1].GetProperty("role").GetString());
        Assert.Equal("What do you see?", messages[1].GetProperty("content").GetString());
    }

    [Fact]
    public async Task SendAsync_PngImagePath_UsesVisionRequestWithBase64PngAndInstructionPrefix()
    {
        var handler = new StubHttpMessageHandler((request, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                {
                  "choices": [
                    {
                      "message": {
                        "content": "Image analysis"
                      }
                    }
                  ]
                }
                """, Encoding.UTF8, "application/json")
            }));

        var service = CreateService(handler);
        var imagePath = CreateTempImageFile(".png", [0x01, 0x02, 0x03, 0x04]);

        try
        {
            var prompt = "Please analyze this x-ray";

            var result = await service.SendAsync(prompt, imagePath);

            Assert.True(result.IsSuccess);
            var request = AssertRequest(handler);

            using var document = await ReadRequestJsonAsync(request);
            var root = document.RootElement;
            Assert.Equal(DentalAIConfig.VisionModel, root.GetProperty("model").GetString());

            var messages = root.GetProperty("messages");
            Assert.Equal(DentalAIConfig.SystemPrompt, messages[0].GetProperty("content").GetString());

            var userContent = messages[1].GetProperty("content");
            Assert.Equal("text", userContent[0].GetProperty("type").GetString());

            var userText = userContent[0].GetProperty("text").GetString();
            Assert.StartsWith(DentalAIConfig.ImageAnalysisInstruction, userText);
            Assert.EndsWith(prompt, userText);

            Assert.Equal("image_url", userContent[1].GetProperty("type").GetString());
            var imageUrl = userContent[1].GetProperty("image_url");
            Assert.Equal("high", imageUrl.GetProperty("detail").GetString());
            Assert.Equal("data:image/png;base64,AQIDBA==", imageUrl.GetProperty("url").GetString());
        }
        finally
        {
            File.Delete(imagePath);
        }
    }

    [Fact]
    public async Task SendAsync_NonPngImagePath_UsesJpegMimeType()
    {
        var handler = new StubHttpMessageHandler((request, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                {
                  "choices": [
                    {
                      "message": {
                        "content": "Image analysis"
                      }
                    }
                  ]
                }
                """, Encoding.UTF8, "application/json")
            }));

        var service = CreateService(handler);
        var imagePath = CreateTempImageFile(".jpg", [0xFF, 0xD8, 0xFF]);

        try
        {
            await service.SendAsync("Check the lesion", imagePath);

            var request = AssertRequest(handler);
            using var document = await ReadRequestJsonAsync(request);
            var imageUrl = document.RootElement
                .GetProperty("messages")[1]
                .GetProperty("content")[1]
                .GetProperty("image_url")
                .GetProperty("url")
                .GetString();

            Assert.StartsWith("data:image/jpeg;base64,", imageUrl);
        }
        finally
        {
            File.Delete(imagePath);
        }
    }

    [Fact]
    public async Task SendAsync_ApiReturnsErrorStatus_ReturnsApiError()
    {
        var handler = new StubHttpMessageHandler((request, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("invalid request")
            }));

        var service = CreateService(handler);

        var result = await service.SendAsync("Bad request test");

        Assert.False(result.IsSuccess);
        Assert.Equal("API Error: invalid request", result.Error);
    }

    [Fact]
    public async Task SendAsync_ResponseMissingChoices_ReturnsInvalidResponseFormat()
    {
        var handler = new StubHttpMessageHandler((request, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"id\":\"abc\"}", Encoding.UTF8, "application/json")
            }));

        var service = CreateService(handler);

        var result = await service.SendAsync("Check response format");

        Assert.False(result.IsSuccess);
        Assert.Equal("Invalid response format", result.Error);
    }

    [Fact]
    public async Task SendAsync_HttpRequestException_ReturnsNetworkError()
    {
        var handler = new StubHttpMessageHandler((request, _) => throw new HttpRequestException("socket failed"));
        var service = CreateService(handler);

        var result = await service.SendAsync("Network test");

        Assert.False(result.IsSuccess);
        Assert.Equal("Network error: socket failed", result.Error);
    }

    [Fact]
    public async Task SendAsync_TaskCanceledException_ReturnsRequestTimeout()
    {
        var handler = new StubHttpMessageHandler((request, _) => throw new TaskCanceledException());
        var service = CreateService(handler);

        var result = await service.SendAsync("Timeout test");

        Assert.False(result.IsSuccess);
        Assert.Equal("Request timeout", result.Error);
    }

    [Fact]
    public async Task SendAsync_UnexpectedException_ReturnsUnexpectedError()
    {
        var handler = new StubHttpMessageHandler((request, _) => throw new InvalidOperationException("boom"));
        var service = CreateService(handler);

        var result = await service.SendAsync("Unexpected test");

        Assert.False(result.IsSuccess);
        Assert.Equal("Unexpected error: boom", result.Error);
    }

    [Fact]
    public async Task SendAsync_ChoicesArrayIsEmpty_ReturnsUnexpectedError()
    {
        var handler = new StubHttpMessageHandler((request, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"choices\":[]}", Encoding.UTF8, "application/json")
            }));

        var service = CreateService(handler);

        var result = await service.SendAsync("Edge case test");

        Assert.False(result.IsSuccess);
        Assert.StartsWith("Unexpected error:", result.Error);
    }

    [Fact]
    public async Task SendAsync_MessageContentMissing_ReturnsUnexpectedError()
    {
        var handler = new StubHttpMessageHandler((request, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                {
                  "choices": [
                    {
                      "message": {}
                    }
                  ]
                }
                """, Encoding.UTF8, "application/json")
            }));

        var service = CreateService(handler);

        var result = await service.SendAsync("Missing content test");

        Assert.False(result.IsSuccess);
        Assert.StartsWith("Unexpected error:", result.Error);
    }

    private static OpenAIService CreateService(StubHttpMessageHandler handler, string apiKey = "test-key")
    {
        var httpClient = new HttpClient(handler);
        return new OpenAIService(httpClient, apiKey);
    }

    private static HttpRequestMessage AssertRequest(StubHttpMessageHandler handler)
    {
        return Assert.IsType<HttpRequestMessage>(handler.LastRequest);
    }

    private static async Task<JsonDocument> ReadRequestJsonAsync(HttpRequestMessage request)
    {
        var json = await request.Content!.ReadAsStringAsync();
        return JsonDocument.Parse(json);
    }

    private static string CreateTempImageFile(string extension, byte[] bytes)
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}{extension}");
        File.WriteAllBytes(path, bytes);
        return path;
    }
}
