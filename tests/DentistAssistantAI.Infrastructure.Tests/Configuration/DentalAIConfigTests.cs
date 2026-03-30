using DentistAssistantAI.Core.Configuration;
using Xunit;

namespace DentistAssistantAI.Infrastructure.Tests.Configuration;

public sealed class DentalAIConfigTests
{
    [Fact]
    public void TextModel_IsNotEmpty()
    {
        Assert.False(string.IsNullOrWhiteSpace(DentalAIConfig.TextModel));
    }

    [Fact]
    public void VisionModel_IsNotEmpty()
    {
        Assert.False(string.IsNullOrWhiteSpace(DentalAIConfig.VisionModel));
    }

    [Fact]
    public void SystemPrompt_ContainsMandatoryDisclaimerLine()
    {
        Assert.Contains("⚠️ DentAI tahlili yordamchi vosita. Klinik ko'rik va mutaxassis xulosasi zarur.", DentalAIConfig.SystemPrompt);
    }

    [Fact]
    public void DefaultImagePrompt_IsNotEmpty()
    {
        Assert.False(string.IsNullOrWhiteSpace(DentalAIConfig.DefaultImagePrompt));
    }

    [Fact]
    public void ImageAnalysisInstruction_ContainsExpectedStructuredHeadings()
    {
        Assert.Contains("RASM TURI VA SIFATI / IMAGE TYPE & QUALITY", DentalAIConfig.ImageAnalysisInstruction);
        Assert.Contains("TASHXIS / DIAGNOSIS & DIFFERENTIALS", DentalAIConfig.ImageAnalysisInstruction);
        Assert.Contains("DAVOLASH TAVSIYASI / TREATMENT RECOMMENDATIONS", DentalAIConfig.ImageAnalysisInstruction);
    }
}
