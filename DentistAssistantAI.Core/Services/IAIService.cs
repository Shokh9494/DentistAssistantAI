namespace DentistAssistantAI.Core.Services
{
    public interface IAIService
    {
        Task<string> SendMessageAsync(string message);
        Task<string> AnalyzeImageAsync(string imagePath, string prompt);
    }
}
