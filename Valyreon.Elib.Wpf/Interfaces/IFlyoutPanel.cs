namespace Valyreon.Elib.Wpf.Interfaces
{
    public interface IFlyoutPanel
    {
        public object ContentControl { get; set; }
        public bool IsOpen { get; set; }
    }
}
