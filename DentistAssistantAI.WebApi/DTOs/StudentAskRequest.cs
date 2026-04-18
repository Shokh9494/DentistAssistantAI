namespace DentistAssistantAI.WebApi.DTOs;

public record StudentAskRequest(string Question, int CourseYear = 2);
