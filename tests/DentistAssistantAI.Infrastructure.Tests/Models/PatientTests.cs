using DentistAssistantAI.Core.Models;
using Xunit;

namespace DentistAssistantAI.Infrastructure.Tests.Models;

public sealed class PatientTests
{
    [Fact]
    public void Initials_TwoWordName_ReturnsTwoUppercaseInitials()
    {
        var patient = new Patient { Name = "Ivanov Ivan" };
        Assert.Equal("II", patient.Initials);
    }

    [Fact]
    public void Initials_SingleWordName_ReturnsSingleInitial()
    {
        var patient = new Patient { Name = "Madonna" };
        Assert.Equal("M", patient.Initials);
    }

    [Fact]
    public void Initials_ThreeWordName_ReturnsFirstTwoInitialsOnly()
    {
        var patient = new Patient { Name = "Van Der Berg" };
        Assert.Equal("VD", patient.Initials);
    }

    [Fact]
    public void Id_IsUniquePerInstance()
    {
        var a = new Patient { Name = "A" };
        var b = new Patient { Name = "B" };
        Assert.NotEqual(a.Id, b.Id);
    }

    [Fact]
    public void Id_DefaultsToNonEmptyGuid()
    {
        var patient = new Patient { Name = "Test" };
        Assert.NotEqual(Guid.Empty, patient.Id);
    }
}
