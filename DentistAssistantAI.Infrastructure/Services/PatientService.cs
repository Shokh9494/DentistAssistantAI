using DentistAssistantAI.Core.Interfaces;
using DentistAssistantAI.Core.Models;

namespace DentistAssistantAI.Infrastructure.Services
{
    public sealed class PatientService : IPatientService
    {
        internal static readonly Guid IvanovIvanId = new("a1b2c3d4-e5f6-7890-abcd-ef1234567890");

        private readonly List<Patient> _patients =
        [
            new() { Id = IvanovIvanId, Name = "Ivanov Ivan", LastVisit = new DateTime(2024, 4, 15), Phone = "+7 (999) 123-45-67", DateOfBirth = new DateTime(1985, 3, 12) },
            new() { Name = "Petrov Petr",     LastVisit = new DateTime(2024, 4, 10), Phone = "+7 (999) 234-56-78", DateOfBirth = new DateTime(1990, 7, 24) },
            new() { Name = "Sidorov Dmitriy", LastVisit = new DateTime(2024, 4, 8),  Phone = "+7 (999) 345-67-89", DateOfBirth = new DateTime(1978, 11, 5)  },
            new() { Name = "Aliyeva Aisha",   LastVisit = new DateTime(2024, 4, 5),  Phone = "+7 (999) 456-78-90", DateOfBirth = new DateTime(1995, 1, 30)  },
            new() { Name = "Karimov Ahmad",   LastVisit = new DateTime(2024, 4, 2),  Phone = "+7 (999) 567-89-01", DateOfBirth = new DateTime(1982, 6, 18)  },
            new() { Name = "Kosimova Olga",   LastVisit = new DateTime(2024, 4, 1),  Phone = "+7 (999) 678-90-12", DateOfBirth = new DateTime(1993, 9, 9)   },
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
