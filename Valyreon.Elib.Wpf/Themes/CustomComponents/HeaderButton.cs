using System.Windows;
using System.Windows.Controls;

namespace Valyreon.Elib.Wpf.CustomComponents
{
	public class HeaderButton : Button
	{
		public static DependencyProperty IconNameProperty;
		public static DependencyProperty TextProperty;

		static HeaderButton()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(HeaderButton),
				new FrameworkPropertyMetadata(typeof(HeaderButton)));
			TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(HeaderButton));
		}

		public string Text
		{
			get => (string)GetValue(TextProperty);
			set => SetValue(TextProperty, value);
		}
	}
}
