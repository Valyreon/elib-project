using System.Windows.Input;

namespace Valyreon.Elib.Wpf.Messages
{
    public class KeyPressedMessage
    {
        public KeyPressedMessage(Key key)
        {
            Key = key;
        }

        public Key Key { get; }
    }
}
