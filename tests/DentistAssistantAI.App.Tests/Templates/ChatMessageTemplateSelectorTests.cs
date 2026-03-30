using DentistAssistantAI.App.Models;
using DentistAssistantAI.App.Templates;
using Microsoft.Maui.Controls;
using Xunit;

namespace DentistAssistantAI.App.Tests.Templates;

public sealed class ChatMessageTemplateSelectorTests
{
    [Fact]
    public void SelectTemplate_UserMessage_ReturnsUserTemplate()
    {
        var userTemplate = new DataTemplate(() => new object());
        var aiTemplate = new DataTemplate(() => new object());
        var selector = new ChatMessageTemplateSelector
        {
            UserTemplate = userTemplate,
            AiTemplate = aiTemplate
        };

        var result = selector.SelectTemplate(new ChatMessageItem("Hi", isFromUser: true), null!);

        Assert.Same(userTemplate, result);
    }

    [Fact]
    public void SelectTemplate_AiMessage_ReturnsAiTemplate()
    {
        var userTemplate = new DataTemplate(() => new object());
        var aiTemplate = new DataTemplate(() => new object());
        var selector = new ChatMessageTemplateSelector
        {
            UserTemplate = userTemplate,
            AiTemplate = aiTemplate
        };

        var result = selector.SelectTemplate(new ChatMessageItem("Hello", isFromUser: false), null!);

        Assert.Same(aiTemplate, result);
    }

    [Fact]
    public void SelectTemplate_NonChatMessageItem_FallsBackToAiTemplate()
    {
        var userTemplate = new DataTemplate(() => new object());
        var aiTemplate = new DataTemplate(() => new object());
        var selector = new ChatMessageTemplateSelector
        {
            UserTemplate = userTemplate,
            AiTemplate = aiTemplate
        };

        var result = selector.SelectTemplate(new object(), null!);

        Assert.Same(aiTemplate, result);
    }
}
