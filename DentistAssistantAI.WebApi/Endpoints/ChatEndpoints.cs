using DentistAssistantAI.Application.Services;
using DentistAssistantAI.WebApi.DTOs;

namespace DentistAssistantAI.WebApi.Endpoints;

public static class ChatEndpoints
{
    public static void MapChatEndpoints(this WebApplication app)
    {
        app.MapPost("/api/chat", async (ChatRequest req, AIManager ai) =>
        {
            if (string.IsNullOrWhiteSpace(req.Message))
                return Results.BadRequest(new { error = "Message cannot be empty." });

            var response = await ai.AskDentistAI(req.Message);
            return Results.Ok(new { response });
        });
    }
}
