using DentistAssistantAI.App.Services;
using DentistAssistantAI.App.Tests.TestDoubles;
using System.Net;
using System.Text;
using Xunit;

namespace DentistAssistantAI.App.Tests.Services;

public sealed class WebApiAIManagerTests
{
    [Fact]
    public async Task AskDentistAI_TextOnly_PostsMultipartAndReturnsResponse()
    {
        var stub = new StubHttpMessageHandler((req, _) =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"response":"dental answer"}""", Encoding.UTF8, "application/json")
            };
            return Task.FromResult(response);
        });

        var client = new HttpClient(stub) { BaseAddress = new Uri("http://localhost") };
        var manager = new WebApiAIManager(client);

        var result = await manager.AskDentistAI("What is caries?");

        Assert.Equal("dental answer", result);
        Assert.Equal(HttpMethod.Post, stub.LastRequest!.Method);
        Assert.Equal("/api/chat", stub.LastRequest.RequestUri!.PathAndQuery);
        Assert.Contains("multipart/form-data", stub.LastRequest.Content!.Headers.ContentType!.MediaType);
    }

    [Fact]
    public async Task AskDentistAI_WithImagePath_SendsMultipartRequest()
    {
        var tempFile = Path.GetTempFileName();
        await File.WriteAllBytesAsync(tempFile, [0x89, 0x50, 0x4e, 0x47]);

        try
        {
            var stub = new StubHttpMessageHandler((_, _) =>
                Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("""{"response":"xray result"}""", Encoding.UTF8, "application/json")
                }));

            var client = new HttpClient(stub) { BaseAddress = new Uri("http://localhost") };
            var manager = new WebApiAIManager(client);

            var result = await manager.AskDentistAI("Analyze xray", tempFile);

            Assert.Equal("xray result", result);
            Assert.Equal(HttpMethod.Post, stub.LastRequest!.Method);
            Assert.Equal("/api/chat", stub.LastRequest.RequestUri!.PathAndQuery);
            Assert.Contains("multipart/form-data", stub.LastRequest.Content!.Headers.ContentType!.MediaType);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task AskDentistAI_NonSuccessStatus_ThrowsHttpRequestException()
    {
        var stub = new StubHttpMessageHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError)));

        var client = new HttpClient(stub) { BaseAddress = new Uri("http://localhost") };
        var manager = new WebApiAIManager(client);

        await Assert.ThrowsAsync<HttpRequestException>(
            () => manager.AskDentistAI("question"));
    }

    [Fact]
    public async Task GenerateLecture_PostsJsonToLectureEndpoint()
    {
        var stub = new StubHttpMessageHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"response":"lecture content"}""", Encoding.UTF8, "application/json")
            }));

        var client = new HttpClient(stub) { BaseAddress = new Uri("http://localhost") };
        var manager = new WebApiAIManager(client);

        var result = await manager.GenerateLecture("Caries", 3);

        Assert.Equal("lecture content", result);
        Assert.Equal("/api/teacher/lecture", stub.LastRequest!.RequestUri!.PathAndQuery);
    }

    [Fact]
    public async Task AskStudent_PostsJsonToStudentAskEndpoint()
    {
        var stub = new StubHttpMessageHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"response":"student answer"}""", Encoding.UTF8, "application/json")
            }));

        var client = new HttpClient(stub) { BaseAddress = new Uri("http://localhost") };
        var manager = new WebApiAIManager(client);

        var result = await manager.AskStudent("What is pulpitis?");

        Assert.Equal("student answer", result);
        Assert.Equal("/api/student/ask", stub.LastRequest!.RequestUri!.PathAndQuery);
    }
}
