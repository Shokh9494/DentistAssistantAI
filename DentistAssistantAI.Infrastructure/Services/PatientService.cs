using DentistAssistantAI.Core.Interfaces;
using DentistAssistantAI.Core.Models;

namespace DentistAssistantAI.Infrastructure.Services
{
    public sealed class PatientService : IPatientService
    {
        private readonly List<Patient> _patients =
        [
            new() { Name = "Ivanov Ivan",     LastVisit = new DateTime(2024, 4, 15) },
            new() { Name = "Petrov Petr",     LastVisit = new DateTime(2024, 4, 10) },
            new() { Name = "Sidorov Dmitriy", LastVisit = new DateTime(2024, 4, 8)  },
            new() { Name = "Aliyeva Aisha",   LastVisit = new DateTime(2024, 4, 5)  },
            new() { Name = "Karimov Ahmad",   LastVisit = new DateTime(2024, 4, 2)  },
            new() { Name = "Kosimova Olga",   LastVisit = new DateTime(2024, 4, 1)  },
        ];

        private readonly Lock _lock = new();

        public Task<IReadOnlyList<Patient>> GetAllAsync()
        {
            lock (_lock)
            {
                var sorted = _patients
                    .OrderByDescending(p => p.LastVisit)
                    .ToList();
                return Task.FromResult<IReadOnlyList<Patient>>(sorted);
            }
        }

        public Task AddAsync(Patient patient)
        {
            lock (_lock)
            {
                _patients.Add(patient);
            }
            return Task.CompletedTask;
        }
    }
}
