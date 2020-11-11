using System.Windows;
using System.Windows.Controls;

namespace ElibWpf.Views.Controls
{
	/// <summary>
	///     Interaction logic for BooksControl.xaml
	/// </summary>
	public partial class BooksTabControl : UserControl
	{
		public BooksTabControl()
		{
			InitializeComponent();
		}

		private void SearchOptionsButton_Click(object sender, RoutedEventArgs e)
		{
			switch(SearchContentRow.Height.Value)
			{
				case 60:
					//SearchContentRow.Height = new System.Windows.GridLength(150);
					SearchOptionsHeightAnimation.From = new GridLength(60);
					SearchOptionsHeightAnimation.To = new GridLength(150);
					AngleDoubleAnimation.To = -180;
					break;

				case 150:
					//SearchContentRow.Height = new System.Windows.GridLength(60);
					SearchOptionsHeightAnimation.From = new GridLength(150);
					SearchOptionsHeightAnimation.To = new GridLength(60);
					AngleDoubleAnimation.To = 0;
					break;

				default:
					return;
			}
		}
	}
}
