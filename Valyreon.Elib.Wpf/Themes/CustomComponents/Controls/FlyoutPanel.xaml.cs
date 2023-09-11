using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using Valyreon.Elib.Wpf.Interfaces;

namespace Valyreon.Elib.Wpf.Themes.CustomComponents.Controls;

/// <summary>
/// Interaction logic for FlyoutPanel.xaml
/// </summary>
public partial class FlyoutPanel : UserControl, IFlyoutPanel
{
    public static DependencyProperty ContentControlProperty;
    public static DependencyProperty IsOpenProperty;
    private static readonly Duration animationDuration = new Duration(new TimeSpan(0, 0, 0, 0, 250));
    private double opacity;
    private Window window;

    static FlyoutPanel()
    {
        IsOpenProperty = DependencyProperty.Register("IsOpen", typeof(bool), typeof(FlyoutPanel));
        ContentControlProperty = DependencyProperty.Register("ContentControl", typeof(object), typeof(FlyoutPanel));
    }

    public FlyoutPanel()
    {
        InitializeComponent();
        Loaded += new RoutedEventHandler((a, i) =>
        {
            window = Window.GetWindow(this);
            theGrid.Margin = new Thickness { Left = 100, Right = 0, Bottom = 0, Top = 0 };
            theGrid.Visibility = Visibility.Collapsed;
            opacity = theGrid.Opacity;
        });
    }

    public object ContentControl
    {
        get => GetValue(ContentControlProperty);
        set => SetValue(ContentControlProperty, value);
    }

    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set
        {
            if (value != IsOpen)
            {
                if (value)
                {
                    Open();
                }
                else
                {
                    Close();
                }
            }
            SetValue(IsOpenProperty, value);
        }
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        IsOpen = false;
    }

    private void Close()
    {
        theGrid.Margin = new Thickness(0);
        var endThickness = new Thickness { Left = window.Width, Right = 0, Bottom = 0, Top = 0 };
        var thicknessAnimation = new ThicknessAnimation(theGrid.Margin, endThickness, animationDuration);
        var opacityAnimation = new DoubleAnimation(opacity, 0, animationDuration);
        thicknessAnimation.Completed += (a, i) => theGrid.Visibility = Visibility.Collapsed;
        theGrid.BeginAnimation(MarginProperty, thicknessAnimation);
        theGrid.BeginAnimation(OpacityProperty, opacityAnimation);
    }

    private void Open()
    {
        theGrid.Margin = new Thickness { Left = window.Width, Right = 0, Bottom = 0, Top = 0 };
        theGrid.Visibility = Visibility.Visible;
        var endThickness = new Thickness(0);
        var thicknessAnimation = new ThicknessAnimation(theGrid.Margin, endThickness, animationDuration);
        var opacityAnimation = new DoubleAnimation(0, opacity, animationDuration);
        opacityAnimation.Completed += (a, i) => theGrid.Opacity = opacity;
        thicknessAnimation.Completed += (a, i) => theGrid.Margin = endThickness;
        theGrid.BeginAnimation(MarginProperty, thicknessAnimation);
        theGrid.BeginAnimation(OpacityProperty, opacityAnimation);
    }
}
