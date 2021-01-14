using System.Windows;
using System.Windows.Controls;

namespace Valyreon.Elib.Wpf.CustomComponents
{
	public class SelectedBannerCheck : CheckBox
	{
		public static DependencyProperty TextProperty;

		static SelectedBannerCheck()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(SelectedBannerCheck),
				new FrameworkPropertyMetadata(typeof(SelectedBannerCheck)));
			TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(SelectedBannerCheck));
		}

		public string Text
		{
			get => (string)GetValue(TextProperty);
			set => SetValue(TextProperty, value);
		}
	}
}
