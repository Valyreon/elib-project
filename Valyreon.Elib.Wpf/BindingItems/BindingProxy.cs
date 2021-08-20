﻿using System.Windows;

namespace Valyreon.Elib.Wpf.BindingItems
{
	public class BindingProxy : Freezable
	{
		public static readonly DependencyProperty DataProperty =
			DependencyProperty.Register("Data", typeof(object), typeof(BindingProxy), new UIPropertyMetadata(null));

		public object Data
		{
			get => GetValue(DataProperty);
			set => SetValue(DataProperty, value);
		}

		protected override Freezable CreateInstanceCore()
		{
			return new BindingProxy();
		}
	}
}
