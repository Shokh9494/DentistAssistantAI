namespace DentistAssistantAI.WebApi.DTOs;

public record EvaluateCaseRequest(string CaseText, string Diagnosis, string Treatment);
