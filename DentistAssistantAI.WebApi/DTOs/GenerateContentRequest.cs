namespace DentistAssistantAI.WebApi.DTOs;

public record GenerateContentRequest(string Topic, int CourseYear = 3, int QuestionCount = 10);
