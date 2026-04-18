using DentistAssistantAI.Core.Interfaces;
using DentistAssistantAI.WebApi.DTOs;

namespace DentistAssistantAI.WebApi.Endpoints;

public static class TeacherEndpoints
{
    public static void MapTeacherEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/teacher");

        group.MapPost("/lecture", async (GenerateContentRequest req, IAIManager ai) =>
        {
            if (string.IsNullOrWhiteSpace(req.Topic))
                return Results.BadRequest(new { error = "Topic cannot be empty." });

            var response = await ai.GenerateLecture(req.Topic, req.CourseYear);
            return Results.Ok(new { response });
        });

        group.MapPost("/test", async (GenerateContentRequest req, IAIManager ai) =>
        {
            if (string.IsNullOrWhiteSpace(req.Topic))
                return Results.BadRequest(new { error = "Topic cannot be empty." });

            var response = await ai.GenerateTest(req.Topic, req.CourseYear, req.QuestionCount);
            return Results.Ok(new { response });
        });

        group.MapPost("/case", async (GenerateContentRequest req, IAIManager ai) =>
        {
            if (string.IsNullOrWhiteSpace(req.Topic))
                return Results.BadRequest(new { error = "Topic cannot be empty." });

            var response = await ai.GenerateTeacherCase(req.Topic, req.CourseYear);
            return Results.Ok(new { response });
        });
    }
}
