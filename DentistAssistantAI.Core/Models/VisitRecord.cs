namespace DentistAssistantAI.Core.Models
{
    public sealed class VisitRecord
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public required Guid PatientId { get; init; }
        public DateTime Date { get; init; } = DateTime.Now;
        public required string Complaint { get; init; }
        public IReadOnlyList<string> Diagnoses { get; init; } = [];
        public IReadOnlyList<string> Treatments { get; init; } = [];
    }
}
