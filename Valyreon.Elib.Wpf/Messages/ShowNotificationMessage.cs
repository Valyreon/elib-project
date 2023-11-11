using Valyreon.Elib.Mvvm.Messaging;

namespace Valyreon.Elib.Wpf.Messages
{
    public enum NotificationType
    {
        Info,
        Warning,
        Error,
        Success
    }

    public class ShowNotificationMessage : MessageBase
    {
        public ShowNotificationMessage(string text, NotificationType type = NotificationType.Info)
        {
            Text = text;
            Type = type;
        }

        public string Text { get; }
        public NotificationType Type { get; }
    }
}
