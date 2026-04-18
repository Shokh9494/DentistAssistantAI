using DentistAssistantAI.Application.Services;
using DentistAssistantAI.WebApi.DTOs;

namespace DentistAssistantAI.WebApi.Endpoints;

public static class ClinicalCaseEndpoints
{
    public static void MapClinicalCaseEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/cases");

        group.MapPost("/generate", async (GenerateContentRequest req, AIManager ai) =>
        {
            if (string.IsNullOrWhiteSpace(req.Topic))
                return Results.BadRequest(new { error = "Topic cannot be empty." });

            var response = await ai.GenerateStudentCase(req.Topic, req.CourseYear);
            return Results.Ok(new { response });
        });

        group.MapPost("/evaluate", async (EvaluateCaseRequest req, AIManager ai) =>
        {
            if (string.IsNullOrWhiteSpace(req.CaseText) || string.IsNullOrWhiteSpace(req.Diagnosis))
                return Results.BadRequest(new { error = "CaseText and Diagnosis are required." });

            var response = await ai.EvaluateStudentAnswer(req.CaseText, req.Diagnosis, req.Treatment);
            return Results.Ok(new { response });
        });
    }
}
