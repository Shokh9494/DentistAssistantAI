using DentistAssistantAI.Core.Models;
using DentistAssistantAI.Infrastructure.Services;
using Xunit;

namespace DentistAssistantAI.Infrastructure.Tests.Services;

public sealed class PatientServiceTests
{
    [Fact]
    public async Task GetAllAsync_ReturnsSixSeededPatients()
    {
        var service = new PatientService();
        var patients = await service.GetAllAsync();
        Assert.Equal(6, patients.Count);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsPatientsOrderedByLastVisitDescending()
    {
        var service = new PatientService();
        var patients = await service.GetAllAsync();
        for (int i = 0; i < patients.Count - 1; i++)
            Assert.True(patients[i].LastVisit >= patients[i + 1].LastVisit);
    }

    [Fact]
    public async Task AddAsync_IncreasesPatientCount()
    {
        var service = new PatientService();
        var before = (await service.GetAllAsync()).Count;
        await service.AddAsync(new Patient { Name = "New Patient" });
        var after = (await service.GetAllAsync()).Count;
        Assert.Equal(before + 1, after);
    }

    [Fact]
    public async Task AddAsync_NewPatientAppearsInResult()
    {
        var service = new PatientService();
        var newPatient = new Patient { Name = "Unique Test Patient" };
        await service.AddAsync(newPatient);
        var patients = await service.GetAllAsync();
        Assert.Contains(patients, p => p.Id == newPatient.Id);
    }

    [Fact]
    public async Task AddAsync_MultipleCallsAllAppear()
    {
        var service = new PatientService();
        var a = new Patient { Name = "Alpha" };
        var b = new Patient { Name = "Beta" };
        await service.AddAsync(a);
        await service.AddAsync(b);
        var patients = await service.GetAllAsync();
        Assert.Contains(patients, p => p.Id == a.Id);
        Assert.Contains(patients, p => p.Id == b.Id);
    }
}
