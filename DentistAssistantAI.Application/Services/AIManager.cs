using DentistAssistantAI.Core.Configuration;
using DentistAssistantAI.Core.Interfaces;

namespace DentistAssistantAI.Application.Services
{
    public class AIManager : IAIManager
    {
        private readonly IOpenAIService _openAIService;

        public AIManager(IOpenAIService openAIService)
        {
            _openAIService = openAIService;
        }

        public async Task<string> AskDentistAI(string question, string? imagePath = null)
        {
            var result = await _openAIService.SendAsync(question, imagePath);

            if (!result.IsSuccess)
                return HandleError(result.Error);

            return result.Content ?? string.Empty;
        }

        public async Task<string> GenerateLecture(string topic, int courseYear)
        {
            var prompt = EducationAIConfig.LecturePromptTemplate(topic, courseYear);
            var result = await _openAIService.SendAsync(prompt, systemPrompt: EducationAIConfig.TeacherSystemPrompt);
            return result.IsSuccess ? result.Content ?? string.Empty : HandleError(result.Error);
        }

        public async Task<string> GenerateTest(string topic, int courseYear, int questionCount = 10)
        {
            var prompt = EducationAIConfig.TestPromptTemplate(topic, courseYear, questionCount);
            var result = await _openAIService.SendAsync(prompt, systemPrompt: EducationAIConfig.TeacherSystemPrompt);
            return result.IsSuccess ? result.Content ?? string.Empty : HandleError(result.Error);
        }

        public async Task<string> GenerateTeacherCase(string topic, int courseYear)
        {
            var prompt = EducationAIConfig.TeacherCasePromptTemplate(topic, courseYear);
            var result = await _openAIService.SendAsync(prompt, systemPrompt: EducationAIConfig.ClinicalCaseSystemPrompt);
            return result.IsSuccess ? result.Content ?? string.Empty : HandleError(result.Error);
        }

        public async Task<string> AskStudent(string question, int courseYear = 2)
        {
            var prompt = EducationAIConfig.StudentAskTemplate(question, courseYear);
            var result = await _openAIService.SendAsync(prompt, systemPrompt: EducationAIConfig.StudentSystemPrompt);
            return result.IsSuccess ? result.Content ?? string.Empty : HandleError(result.Error);
        }

        public async Task<string> GenerateStudentCase(string topic, int courseYear)
        {
            var prompt = EducationAIConfig.StudentCasePromptTemplate(topic, courseYear);
            var result = await _openAIService.SendAsync(prompt, systemPrompt: EducationAIConfig.ClinicalCaseSystemPrompt);
            return result.IsSuccess ? result.Content ?? string.Empty : HandleError(result.Error);
        }

        public async Task<string> EvaluateStudentAnswer(string caseText, string diagnosis, string treatment)
        {
            var prompt = EducationAIConfig.CaseEvaluationTemplate(caseText, diagnosis, treatment);
            var result = await _openAIService.SendAsync(prompt, systemPrompt: EducationAIConfig.ClinicalCaseSystemPrompt);
            return result.IsSuccess ? result.Content ?? string.Empty : HandleError(result.Error);
        }

        private static string HandleError(string? error)
        {
            if (error != null && error.Contains("quota"))
                return "AI quota exceeded. Please check billing.";

            return "AI temporarily unavailable.";
        }
    }
}
