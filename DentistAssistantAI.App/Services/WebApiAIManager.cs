using DentistAssistantAI.Core.Interfaces;
using System.Net.Http.Json;
using System.Text.Json;

namespace DentistAssistantAI.App.Services;

public class WebApiAIManager : IAIManager
{
    private readonly HttpClient _httpClient;

    public WebApiAIManager(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> AskDentistAI(string question, string? imagePath = null)
    {
        using var form = new MultipartFormDataContent();
        form.Add(new StringContent(question), "message");

        if (imagePath != null)
        {
            var bytes = await File.ReadAllBytesAsync(imagePath);
            form.Add(new ByteArrayContent(bytes), "image", Path.GetFileName(imagePath));
        }

        var resp = await _httpClient.PostAsync("/api/chat", form);
        resp.EnsureSuccessStatusCode();
        return await ExtractResponse(resp);
    }

    public async Task<string> GenerateLecture(string topic, int courseYear)
    {
        var resp = await _httpClient.PostAsJsonAsync("/api/teacher/lecture",
            new { Topic = topic, CourseYear = courseYear });
        resp.EnsureSuccessStatusCode();
        return await ExtractResponse(resp);
    }

    public async Task<string> GenerateTest(string topic, int courseYear, int questionCount = 10)
    {
        var resp = await _httpClient.PostAsJsonAsync("/api/teacher/test",
            new { Topic = topic, CourseYear = courseYear, QuestionCount = questionCount });
        resp.EnsureSuccessStatusCode();
        return await ExtractResponse(resp);
    }

    public async Task<string> GenerateTeacherCase(string topic, int courseYear)
    {
        var resp = await _httpClient.PostAsJsonAsync("/api/teacher/case",
            new { Topic = topic, CourseYear = courseYear });
        resp.EnsureSuccessStatusCode();
        return await ExtractResponse(resp);
    }

    public async Task<string> GenerateStudentCase(string topic, int courseYear)
    {
        var resp = await _httpClient.PostAsJsonAsync("/api/cases/generate",
            new { Topic = topic, CourseYear = courseYear });
        resp.EnsureSuccessStatusCode();
        return await ExtractResponse(resp);
    }

    public async Task<string> AskStudent(string question, int courseYear = 2)
    {
        var resp = await _httpClient.PostAsJsonAsync("/api/student/ask",
            new { Question = question, CourseYear = courseYear });
        resp.EnsureSuccessStatusCode();
        return await ExtractResponse(resp);
    }

    public async Task<string> EvaluateStudentAnswer(string caseText, string diagnosis, string treatment)
    {
        var resp = await _httpClient.PostAsJsonAsync("/api/cases/evaluate",
            new { CaseText = caseText, Diagnosis = diagnosis, Treatment = treatment });
        resp.EnsureSuccessStatusCode();
        return await ExtractResponse(resp);
    }

    private static async Task<string> ExtractResponse(HttpResponseMessage resp)
    {
        var json = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("response").GetString() ?? string.Empty;
    }
}
