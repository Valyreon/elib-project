using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Valyreon.Elib.Wpf.Animations;
using Valyreon.Elib.Wpf.CustomComponents;

namespace Valyreon.Elib.Wpf.Views.Controls;

/// <summary>
///     Interaction logic for BookTileControl.xaml
/// </summary>
public partial class BookTileControl : UserControl
{
    private readonly Duration duration;
    private SelectedBannerCheck selectedCheckbox;
    private TextLinkButton theBookTitle;

    private Border tileBorder;

    public BookTileControl()
    {
        InitializeComponent();
        duration = new Duration(new TimeSpan(0, 0, 0, 0, 300));
    }

    private void BookContainer_MouseEnter(object sender, MouseEventArgs ev)
    {
        if (selectedCheckbox.IsChecked == false)
        {
            var borderAnim = new BrushAnimation(Brushes.CornflowerBlue, duration);
            borderAnim.Completed += (e, s) => tileBorder.BorderBrush = Brushes.CornflowerBlue;
            borderAnim.Completed += (e, s) => theBookTitle.Foreground = Brushes.CornflowerBlue;

            var opacAnim = new DoubleAnimation(1, duration, FillBehavior.Stop);
            opacAnim.Completed += (e, s) => selectedCheckbox.Opacity = 1;

            tileBorder.BeginAnimation(Border.BorderBrushProperty, borderAnim);
            theBookTitle.BeginAnimation(ForegroundProperty, borderAnim);
            selectedCheckbox.BeginAnimation(OpacityProperty, opacAnim);
        }
        else
        {
            var borderAnim = new BrushAnimation(Brushes.CornflowerBlue, duration);
            borderAnim.Completed += (e, s) => tileBorder.BorderBrush = Brushes.CornflowerBlue;
            borderAnim.Completed += (e, s) => theBookTitle.Foreground = Brushes.CornflowerBlue;
            tileBorder.BeginAnimation(Border.BorderBrushProperty, borderAnim);
            theBookTitle.BeginAnimation(ForegroundProperty, borderAnim);
        }
    }

    private void BookContainer_MouseLeave(object sender, MouseEventArgs ev)
    {
        var borderAnim = new BrushAnimation(Brushes.LightGray, duration);
        borderAnim.Completed += (e, s) => tileBorder.BorderBrush = Brushes.LightGray;

        var titleAnim = new BrushAnimation(Brushes.Black, duration);
        titleAnim.Completed += (e, s) => theBookTitle.Foreground = Brushes.Black;

        if (selectedCheckbox.IsChecked == false)
        {
            var opacAnim = new DoubleAnimation(0, duration, FillBehavior.Stop);
            opacAnim.Completed += (e, s) =>
            {
                if (!selectedCheckbox.IsChecked.Value)
                {
                    selectedCheckbox.Opacity = 0;
                }
            };

            tileBorder.BeginAnimation(Border.BorderBrushProperty, borderAnim);
            theBookTitle.BeginAnimation(ForegroundProperty, titleAnim);
            selectedCheckbox.BeginAnimation(OpacityProperty, opacAnim);
        }
        else
        {
            tileBorder.BeginAnimation(Border.BorderBrushProperty, borderAnim);
            theBookTitle.BeginAnimation(ForegroundProperty, titleAnim);
        }
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        tileBorder = FindName("TileBorder") as Border;
        theBookTitle = FindName("TheBookTitle") as TextLinkButton;
        selectedCheckbox = FindName("SelectedCheckbox") as SelectedBannerCheck;

        if (selectedCheckbox.IsChecked != null && selectedCheckbox.IsChecked.Value)
        {
            selectedCheckbox.Opacity = 1;
        }
    }

    private void SelectedCheckbox_Checked(object sender, RoutedEventArgs e)
    {
        if(selectedCheckbox != null)
        {
            selectedCheckbox.Opacity = 1;
        }
    }

    private void SelectedCheckbox_Unchecked(object sender, RoutedEventArgs e)
    {
        
    }
}
