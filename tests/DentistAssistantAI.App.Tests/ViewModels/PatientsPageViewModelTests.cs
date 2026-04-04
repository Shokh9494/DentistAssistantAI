using DentistAssistantAI.App.Tests.TestDoubles;
using DentistAssistantAI.App.ViewModels;
using DentistAssistantAI.Core.Models;
using Xunit;

namespace DentistAssistantAI.App.Tests.ViewModels;

public sealed class PatientsPageViewModelTests
{
    [Fact]
    public async Task LoadPatientsAsync_PopulatesCollection()
    {
        var patients = new[] { new Patient { Name = "Test Patient" } };
        var vm = new PatientsPageViewModel(new FakePatientService(patients));

        await vm.LoadPatientsCommand.ExecuteAsync(null);

        Assert.Single(vm.Patients);
        Assert.Equal("Test Patient", vm.Patients[0].Name);
    }

    [Fact]
    public async Task LoadPatientsAsync_ReplacesExistingItems()
    {
        var fake = new FakePatientService([new Patient { Name = "Old" }]);
        var vm = new PatientsPageViewModel(fake);

        await vm.LoadPatientsCommand.ExecuteAsync(null);
        Assert.Single(vm.Patients);

        await fake.AddAsync(new Patient { Name = "New" });
        await vm.LoadPatientsCommand.ExecuteAsync(null);

        Assert.Equal(2, vm.Patients.Count);
    }

    [Fact]
    public async Task LoadPatientsAsync_EmptyService_LeavesCollectionEmpty()
    {
        var vm = new PatientsPageViewModel(new FakePatientService());
        await vm.LoadPatientsCommand.ExecuteAsync(null);
        Assert.Empty(vm.Patients);
    }

    [Fact]
    public async Task AddPatientAsync_AppendsOneItemToCollection()
    {
        var vm = new PatientsPageViewModel(new FakePatientService());
        await vm.AddPatientCommand.ExecuteAsync(null);
        Assert.Single(vm.Patients);
    }

    [Fact]
    public async Task AddPatientAsync_MultipleCallsAppendMultipleItems()
    {
        var vm = new PatientsPageViewModel(new FakePatientService());
        await vm.AddPatientCommand.ExecuteAsync(null);
        await vm.AddPatientCommand.ExecuteAsync(null);
        Assert.Equal(2, vm.Patients.Count);
    }

    [Fact]
    public async Task AddPatientAsync_InsertsAtFrontOfCollection()
    {
        var existing = new Patient { Name = "Existing" };
        var vm = new PatientsPageViewModel(new FakePatientService([existing]));
        await vm.LoadPatientsCommand.ExecuteAsync(null);

        await vm.AddPatientCommand.ExecuteAsync(null);

        Assert.Equal(2, vm.Patients.Count);
        Assert.NotEqual("Existing", vm.Patients[0].Name); // new patient is at index 0
    }
}
