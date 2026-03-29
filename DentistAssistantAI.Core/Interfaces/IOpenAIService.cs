using DentistAssistantAI.Core.Models;

namespace DentistAssistantAI.Core.Interfaces
{
    public interface IOpenAIService
    {
        Task<AIResult> SendAsync(string prompt, string? imagePath = null);
    }
}
