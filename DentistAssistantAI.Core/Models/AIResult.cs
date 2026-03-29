namespace DentistAssistantAI.Core.Models
{
    public class AIResult
    {
        public bool IsSuccess { get; set; }
        public string? Content { get; set; }
        public string? Error { get; set; }
    }
}
