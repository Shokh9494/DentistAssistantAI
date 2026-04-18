using DentistAssistantAI.App.Tests.TestDoubles;
using DentistAssistantAI.App.ViewModels;
using DentistAssistantAI.Core.Models;
using Xunit;

namespace DentistAssistantAI.App.Tests.ViewModels;

public sealed class PatientDetailViewModelTests
{
    private static Patient MakePatient() =>
        new() { Name = "Ivanov Ivan" };

    private static VisitRecord MakeVisit(
        Guid patientId,
        string complaint = "Toothache",
        DateTime? date = null) =>
        new()
        {
            PatientId = patientId,
            Complaint = complaint,
            Date = date ?? new DateTime(2024, 4, 15),
            Diagnoses = ["Caries"],
            Treatments = ["Filling"]
        };

    [Fact]
    public async Task LoadVisits_PopulatesVisitsCollection_WhenPatientIsSet()
    {
        var patient = MakePatient();
        var fake = new FakeVisitService([MakeVisit(patient.Id), MakeVisit(patient.Id)]);
        var vm = new PatientDetailViewModel(fake);

        vm.Patient = patient;
        await vm.LoadVisitsCommand.ExecuteAsync(null);

        Assert.Equal(2, vm.Visits.Count);
    }

    [Fact]
    public async Task LoadVisits_LeavesCollectionEmpty_WhenNoVisitsExist()
    {
        var patient = MakePatient();
        var vm = new PatientDetailViewModel(new FakeVisitService());

        vm.Patient = patient;
        await vm.LoadVisitsCommand.ExecuteAsync(null);

        Assert.Empty(vm.Visits);
    }

    [Fact]
    public async Task LoadVisits_MapsComplaint_Correctly()
    {
        var patient = MakePatient();
        var fake = new FakeVisitService([MakeVisit(patient.Id, "Sharp pain in molar")]);
        var vm = new PatientDetailViewModel(fake);

        vm.Patient = patient;
        await vm.LoadVisitsCommand.ExecuteAsync(null);

        Assert.Equal("Sharp pain in molar", vm.Visits[0].Complaint);
    }

    [Fact]
    public async Task LoadVisits_MapsDateLabel_Correctly()
    {
        var patient = MakePatient();
        var date = new DateTime(2024, 4, 15);
        var fake = new FakeVisitService([MakeVisit(patient.Id, date: date)]);
        var vm = new PatientDetailViewModel(fake);

        vm.Patient = patient;
        await vm.LoadVisitsCommand.ExecuteAsync(null);

        Assert.Equal("April 15, 2024", vm.Visits[0].DateLabel);
    }

    [Fact]
    public async Task IsBusy_IsFalseAfterLoad()
    {
        var patient = MakePatient();
        var vm = new PatientDetailViewModel(new FakeVisitService([MakeVisit(patient.Id)]));

        vm.Patient = patient;
        await vm.LoadVisitsCommand.ExecuteAsync(null);

        Assert.False(vm.IsBusy);
    }
}
