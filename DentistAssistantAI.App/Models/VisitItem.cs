using DentistAssistantAI.Core.Models;

namespace DentistAssistantAI.App.Models
{
    public sealed class VisitItem
    {
        public VisitItem(VisitRecord record)
        {
            DateLabel = record.Date.ToString("MMMM d, yyyy");
            DateMonthDay = record.Date.ToString("MMMM d");
            Complaint = record.Complaint;
            Diagnoses = record.Diagnoses;
            Treatments = record.Treatments;
        }

        public string DateLabel { get; }
        public string DateMonthDay { get; }
        public string Complaint { get; }
        public IReadOnlyList<string> Diagnoses { get; }
        public IReadOnlyList<string> Treatments { get; }
        public IReadOnlyList<string> AllBulletItems =>
            [.. Diagnoses, .. Treatments];
    }
}
