namespace DentistAssistantAI.App.Models
{
    public sealed class ChatMessageItem
    {
        public ChatMessageItem(string text, bool isFromUser, string? imagePath = null)
        {
            Text = text;
            IsFromUser = isFromUser;
            ImagePath = imagePath;
            Timestamp = DateTime.Now;
        }

        public string Text { get; }
        public bool IsFromUser { get; }
        public string? ImagePath { get; }
        public DateTime Timestamp { get; }

        public string TimestampText => Timestamp.ToString("HH:mm");
        public bool HasImage => !string.IsNullOrEmpty(ImagePath);
        public bool HasText => !string.IsNullOrEmpty(Text);
        public ImageSource? ImageSource => HasImage ? ImageSource.FromFile(ImagePath!) : null;
    }
}