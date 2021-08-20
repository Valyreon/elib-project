using CefSharp;
using CefSharp.Wpf;
using System.Windows;

namespace Valyreon.Elib.Wpf.AttachedProperties
{
    public static class BrowserBehavior
    {
        public static readonly DependencyProperty HtmlProperty = DependencyProperty.RegisterAttached(
            "Html",
            typeof(string),
            typeof(BrowserBehavior),
            new FrameworkPropertyMetadata(OnHtmlChanged));

        [AttachedPropertyBrowsableForType(typeof(ChromiumWebBrowser))]
        public static string GetHtml(ChromiumWebBrowser d)
        {
            return (string)d.GetValue(HtmlProperty);
        }

        public static void SetHtml(ChromiumWebBrowser d, string value)
        {
            d.SetValue(HtmlProperty, value);
        }

        private static void OnHtmlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ChromiumWebBrowser wb)
            {
                wb.LoadHtml(e.NewValue as string, "http://rendering/");
            }
        }
    }
}
