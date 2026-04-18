using DentistAssistantAI.Application.Services;
using DentistAssistantAI.WebApi.DTOs;

namespace DentistAssistantAI.WebApi.Endpoints;

public static class StudentEndpoints
{
    public static void MapStudentEndpoints(this WebApplication app)
    {
        app.MapPost("/api/student/ask", async (StudentAskRequest req, AIManager ai) =>
        {
            if (string.IsNullOrWhiteSpace(req.Question))
                return Results.BadRequest(new { error = "Question cannot be empty." });

            var response = await ai.AskStudent(req.Question, req.CourseYear);
            return Results.Ok(new { response });
        });
    }
}
