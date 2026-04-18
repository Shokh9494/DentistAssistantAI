using DentistAssistantAI.Core.Models;

namespace DentistAssistantAI.Core.Interfaces
{
    public interface IVisitService
    {
        Task<IReadOnlyList<VisitRecord>> GetByPatientIdAsync(Guid patientId);
        Task AddAsync(VisitRecord visit);
    }
}
