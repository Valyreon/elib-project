using Valyreon.Elib.Mvvm.Messaging;

namespace Valyreon.Elib.Wpf.Messages;

public class SetGlobalLoaderMessage : MessageBase
{
    public SetGlobalLoaderMessage(bool isVisible = true)
    {
        IsVisible = isVisible;
    }

    public bool IsVisible { get; }
}
