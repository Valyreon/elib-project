using MahApps.Metro.IconPacks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Valyreon.Elib.Wpf.CustomComponents
{
	public class TextBoxWithSymbol : TextBox
	{
		public static DependencyProperty IconNameProperty;
		public static DependencyProperty IconSizeProperty;
		public static DependencyProperty IconMarginProperty;
		public static DependencyProperty TextboxPaddingProperty;
		public static DependencyProperty WatermarkTextProperty;
		public static DependencyProperty EnterCommandProperty;

		static TextBoxWithSymbol()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(TextBoxWithSymbol),
				new FrameworkPropertyMetadata(typeof(TextBoxWithSymbol)));
			IconNameProperty =
				DependencyProperty.Register("IconName", typeof(PackIconFontAwesomeKind), typeof(TextBoxWithSymbol));
			IconSizeProperty = DependencyProperty.Register("IconSize", typeof(double), typeof(TextBoxWithSymbol));
			WatermarkTextProperty =
				DependencyProperty.Register("WatermarkText", typeof(string), typeof(TextBoxWithSymbol));
			IconMarginProperty =
				DependencyProperty.Register("IconMargin", typeof(Thickness), typeof(TextBoxWithSymbol));
			TextboxPaddingProperty =
				DependencyProperty.Register("TextboxPadding", typeof(Thickness), typeof(TextBoxWithSymbol));
			EnterCommandProperty =
				DependencyProperty.Register("EnterCommand", typeof(ICommand), typeof(TextBoxWithSymbol));
		}

		public ICommand EnterCommand
		{
			get => (ICommand)GetValue(EnterCommandProperty);
			set => SetValue(EnterCommandProperty, value);
		}

		public Thickness IconMargin
		{
			get => (Thickness)GetValue(IconMarginProperty);
			set => SetValue(IconMarginProperty, value);
		}

		public PackIconFontAwesomeKind IconName
		{
			get => (PackIconFontAwesomeKind)GetValue(IconNameProperty);
			set => SetValue(IconNameProperty, value);
		}

		public double IconSize
		{
			get => (double)GetValue(IconSizeProperty);
			set => SetValue(IconSizeProperty, value);
		}

		public Thickness TextboxPadding
		{
			get => (Thickness)GetValue(TextboxPaddingProperty);
			set => SetValue(TextboxPaddingProperty, value);
		}

		public string WatermarkText
		{
			get => (string)GetValue(WatermarkTextProperty);
			set => SetValue(WatermarkTextProperty, value);
		}
	}
}
