using DentistAssistantAI.App.Models;

namespace DentistAssistantAI.App.Templates
{
    public sealed class ChatMessageTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? UserTemplate { get; set; }
        public DataTemplate? AiTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
            => item is ChatMessageItem { IsFromUser: true } ? UserTemplate! : AiTemplate!;
    }
}