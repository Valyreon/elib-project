using Valyreon.Elib.Mvvm.Messaging;

namespace Valyreon.Elib.Wpf.Messages
{
    public class ShowNotificationMessage : MessageBase
    {
        public ShowNotificationMessage(string text, NotificationType type)
        {
            Text = text;
            Type = type;
        }

        public string Text { get; }
        public NotificationType Type { get; }
    }

    public enum NotificationType
    {
        Info,
        Warning,
        Error,
        Success
    }
}
