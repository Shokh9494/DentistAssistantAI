using DentistAssistantAI.Core.Interfaces;
using DentistAssistantAI.WebApi.DTOs;

namespace DentistAssistantAI.WebApi.Endpoints;

public static class ChatEndpoints
{
    public static void MapChatEndpoints(this WebApplication app)
    {
        app.MapPost("/api/chat", async (HttpRequest req, IAIManager ai) =>
        {
            string message;
            string? tempPath = null;

            var contentType = req.ContentType ?? string.Empty;

            if (contentType.Contains("multipart/form-data"))
            {
                var form = await req.ReadFormAsync();
                message = form["message"].FirstOrDefault() ?? string.Empty;
                var imageFile = form.Files.GetFile("image");

                if (imageFile is not null)
                {
                    tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                    await using var fs = File.OpenWrite(tempPath);
                    await imageFile.CopyToAsync(fs);
                }
            }
            else
            {
                var body = await req.ReadFromJsonAsync<ChatRequest>();
                message = body?.Message ?? string.Empty;
            }

            if (string.IsNullOrWhiteSpace(message) && tempPath is null)
                return Results.BadRequest(new { error = "Message or image is required." });

            try
            {
                var response = await ai.AskDentistAI(message, tempPath);
                return Results.Ok(new { response });
            }
            finally
            {
                if (tempPath is not null && File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }).DisableAntiforgery();
    }
}
