using DentistAssistantAI.Core.Interfaces;
using DentistAssistantAI.Core.Models;

namespace DentistAssistantAI.App.Tests.TestDoubles;

internal sealed class FakeVisitService : IVisitService
{
    private readonly List<VisitRecord> _visits = [];

    public FakeVisitService(IEnumerable<VisitRecord>? seed = null)
    {
        if (seed is not null)
            _visits.AddRange(seed);
    }

    public Task<IReadOnlyList<VisitRecord>> GetByPatientIdAsync(Guid patientId) =>
        Task.FromResult<IReadOnlyList<VisitRecord>>(
            _visits.Where(v => v.PatientId == patientId).ToList());

    public Task AddAsync(VisitRecord visit)
    {
        _visits.Add(visit);
        return Task.CompletedTask;
    }
}
