namespace DentistAssistantAI.Core.Interfaces;

public interface IAIManager
{
    Task<string> AskDentistAI(string question, string? imagePath = null);
    Task<string> GenerateLecture(string topic, int courseYear);
    Task<string> GenerateTest(string topic, int courseYear, int questionCount = 10);
    Task<string> GenerateTeacherCase(string topic, int courseYear);
    Task<string> GenerateStudentCase(string topic, int courseYear);
    Task<string> AskStudent(string question, int courseYear = 2);
    Task<string> EvaluateStudentAnswer(string caseText, string diagnosis, string treatment);
}
