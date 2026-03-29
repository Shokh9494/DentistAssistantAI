using DentistAssistantAI.Core.Interfaces;

namespace DentistAssistantAI.Application.Services
{
    public class AIManager
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

        private static string HandleError(string? error)
        {
            if (error != null && error.Contains("quota"))
                return "AI quota exceeded. Please check billing.";

            return "AI temporarily unavailable.";
        }
    }
}
