using DentistAssistantAI.Core.Models;

namespace DentistAssistantAI.Core.Interfaces
{
    public interface IPatientService
    {
        Task<IReadOnlyList<Patient>> GetAllAsync();
        Task AddAsync(Patient patient);
    }
}
