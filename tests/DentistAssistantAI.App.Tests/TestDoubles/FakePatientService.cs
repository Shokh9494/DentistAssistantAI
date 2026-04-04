using DentistAssistantAI.Core.Interfaces;
using DentistAssistantAI.Core.Models;

namespace DentistAssistantAI.App.Tests.TestDoubles;

internal sealed class FakePatientService : IPatientService
{
    private readonly List<Patient> _patients = [];

    public FakePatientService(IEnumerable<Patient>? seed = null)
    {
        if (seed is not null)
            _patients.AddRange(seed);
    }

    public Task<IReadOnlyList<Patient>> GetAllAsync() =>
        Task.FromResult<IReadOnlyList<Patient>>(_patients.ToList());

    public Task AddAsync(Patient patient)
    {
        _patients.Add(patient);
        return Task.CompletedTask;
    }
}
