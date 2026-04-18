using DentistAssistantAI.Core.Interfaces;
using DentistAssistantAI.Core.Models;

namespace DentistAssistantAI.Infrastructure.Services
{
    public sealed class VisitService : IVisitService
    {
        private static readonly Guid _ivanovIvanId = PatientService.IvanovIvanId;

        private readonly List<VisitRecord> _visits =
        [
            new()
            {
                PatientId  = _ivanovIvanId,
                Date       = new DateTime(2024, 4, 15),
                Complaint  = "Toothache in upper left molar",
                Diagnoses  = ["Caries"],
                Treatments = ["Cleaning", "Filling"]
            },
            new()
            {
                PatientId  = _ivanovIvanId,
                Date       = new DateTime(2024, 4, 5),
                Complaint  = "Sensitivity in lower right molar",
                Diagnoses  = ["Enamel erosion"],
                Treatments = ["Fluoride treatment"]
            },
            new()
            {
                PatientId  = _ivanovIvanId,
                Date       = new DateTime(2024, 3, 22),
                Complaint  = "Routine checkup",
                Diagnoses  = ["Healthy teeth"],
                Treatments = ["Professional cleaning"]
            },
        ];

        private readonly Lock _lock = new();

        public Task<IReadOnlyList<VisitRecord>> GetByPatientIdAsync(Guid patientId)
        {
            lock (_lock)
            {
                var result = _visits
                    .Where(v => v.PatientId == patientId)
                    .OrderByDescending(v => v.Date)
                    .ToList();
                return Task.FromResult<IReadOnlyList<VisitRecord>>(result);
            }
        }

        public Task AddAsync(VisitRecord visit)
        {
            lock (_lock)
            {
                _visits.Add(visit);
            }
            return Task.CompletedTask;
        }
    }
}
