using System;
using System.Windows;
using System.Windows.Controls;

namespace Valyreon.Elib.Wpf.CustomComponents
{
	public class SimpleFlatSearchBox : TextBox
	{
		public static DependencyProperty IconProperty;
		public static DependencyProperty WatermarkTextProperty;

		static SimpleFlatSearchBox()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(SimpleFlatSearchBox),
				new FrameworkPropertyMetadata(typeof(SimpleFlatSearchBox)));
			IconProperty = DependencyProperty.Register("IconName", typeof(Enum), typeof(SimpleFlatSearchBox));
			WatermarkTextProperty =
				DependencyProperty.Register("WatermarkText", typeof(string), typeof(SimpleFlatSearchBox));
		}

		public Enum IconName
		{
			get => (Enum)GetValue(IconProperty);
			set => SetValue(IconProperty, value);
		}

		public string WatermarkText
		{
			get => (string)GetValue(WatermarkTextProperty);
			set => SetValue(WatermarkTextProperty, value);
		}
	}
}
