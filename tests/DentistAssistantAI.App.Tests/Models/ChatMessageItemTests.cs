using System.Text.RegularExpressions;
using DentistAssistantAI.App.Models;
using Xunit;

namespace DentistAssistantAI.App.Tests.Models;

public sealed class ChatMessageItemTests
{
    [Fact]
    public void Constructor_StoresAssignedValues()
    {
        var item = new ChatMessageItem("Findings", isFromUser: true, imagePath: "C:\\temp\\scan.jpg");

        Assert.Equal("Findings", item.Text);
        Assert.True(item.IsFromUser);
        Assert.Equal("C:\\temp\\scan.jpg", item.ImagePath);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("C:\\temp\\scan.jpg", true)]
    public void HasImage_ReturnsExpectedValue(string? imagePath, bool expected)
    {
        var item = new ChatMessageItem("Findings", isFromUser: false, imagePath: imagePath);

        Assert.Equal(expected, item.HasImage);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("Findings", true)]
    public void HasText_ReturnsExpectedValue(string? text, bool expected)
    {
        var item = new ChatMessageItem(text ?? string.Empty, isFromUser: false);

        Assert.Equal(expected, item.HasText);
    }

    [Fact]
    public void TimestampText_UsesHourMinuteFormat()
    {
        var item = new ChatMessageItem("Findings", isFromUser: false);

        Assert.Matches(new Regex("^\\d{2}:\\d{2}$"), item.TimestampText);
    }
}
