using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using Microsoft.Win32;
using Valyreon.Elib.Wpf.Helpers;
using Valyreon.Elib.Wpf.ViewModels.Windows;
using Button = System.Windows.Controls.Button;
using Point = System.Windows.Point;

namespace Valyreon.Elib.Wpf.Views.Windows
{
    /// <summary>
    ///     Interaction logic for TheWindow.xaml
    /// </summary>
    public partial class TheWindow : Window
    {
        private bool isMouseButtonDown;
        private bool isManualDrag;
        private Point mouseDownPosition;
        private Point positionBeforeDrag;
        private System.Windows.Point previousScreenBounds;
        private double HeightBeforeMaximize;
        private double WidthBeforeMaximize;
        private WindowState PreviousState;

        public Grid LayoutRoot { get; private set; }
        public Button MinimizeButton { get; private set; }
        public Button MaximizeButton { get; private set; }
        public Button RestoreButton { get; private set; }
        public Button CloseButton { get; private set; }
        public Grid HeaderBar { get; private set; }

        public TheWindow()
        {
            InitializeComponent();
            DataContext = new TheWindowViewModel();

            var currentDPIScaleFactor = (double)SystemHelper.GetCurrentDPIScaleFactor();
            var screen = Screen.FromHandle(new WindowInteropHelper(this).Handle);
            SizeChanged += new SizeChangedEventHandler(OnSizeChanged);
            StateChanged += new EventHandler(OnStateChanged);
            Loaded += new RoutedEventHandler(OnLoaded);
            var workingArea = screen.WorkingArea;
            MaxHeight = (double)(workingArea.Height + 16) / currentDPIScaleFactor;
            SystemEvents.DisplaySettingsChanged += new EventHandler(SystemEvents_DisplaySettingsChanged);
            AddHandler(MouseLeftButtonUpEvent, new MouseButtonEventHandler(OnMouseButtonUp), true);
            AddHandler(MouseMoveEvent, new System.Windows.Input.MouseEventHandler(OnMouseMove));
        }

        public T GetRequiredTemplateChild<T>(string childName) where T : DependencyObject
        {
            return (T)GetTemplateChild(childName);
        }

        public override void OnApplyTemplate()
        {
            LayoutRoot = GetRequiredTemplateChild<Grid>("LayoutRoot");
            MinimizeButton = GetRequiredTemplateChild<Button>("MinimizeButton");
            MaximizeButton = GetRequiredTemplateChild<Button>("MaximizeButton");
            RestoreButton = GetRequiredTemplateChild<Button>("RestoreButton");
            CloseButton = GetRequiredTemplateChild<Button>("CloseButton");
            HeaderBar = GetRequiredTemplateChild<Grid>("PART_HeaderBar");

            if (CloseButton != null)
            {
                CloseButton.Click += CloseButton_Click;
            }

            if (MinimizeButton != null)
            {
                MinimizeButton.Click += MinimizeButton_Click;
            }

            if (RestoreButton != null)
            {
                RestoreButton.Click += RestoreButton_Click;
            }

            if (MaximizeButton != null)
            {
                MaximizeButton.Click += MaximizeButton_Click;
            }

            HeaderBar.MouseLeftButtonDown += OnHeaderBarMouseLeftButtonDown;
            //HeaderBar.MouseDoubleClick += MaximizeButton_Click;

            HeaderBar.AddHandler(MouseDoubleClickEvent, new MouseButtonEventHandler(MaximizeButton_Click));

            base.OnApplyTemplate();
        }

        protected void OnHeaderBarMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var position = e.GetPosition(this);
            var headerBarHeight = 36;
            var leftmostClickableOffset = 50;

            if (position.X - LayoutRoot.Margin.Left <= leftmostClickableOffset && position.Y <= headerBarHeight)
            {
                if (e.ClickCount != 2)
                {
                    // this.OpenSystemContextMenu(e);
                }
                else
                {
                    Close();
                }
                e.Handled = true;
                return;
            }

            if (e.ClickCount == 2 && ResizeMode == ResizeMode.CanResize)
            {
                ToggleWindowState();
                return;
            }

            if (WindowState == WindowState.Maximized)
            {
                isMouseButtonDown = true;
                mouseDownPosition = position;
            }
            else
            {
                try
                {
                    positionBeforeDrag = new System.Windows.Point(Left, Top);
                    DragMove();
                }
                catch
                {
                }
            }
        }

        protected void ToggleWindowState()
        {
            WindowState = WindowState != WindowState.Maximized
                ? WindowState.Maximized
                : WindowState.Normal;
            MaximizeButton.Visibility = WindowState == WindowState.Normal ? Visibility.Visible : Visibility.Collapsed;
            RestoreButton.Visibility = WindowState == WindowState.Normal ? Visibility.Collapsed : Visibility.Visible;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleWindowState();
        }

        private void RestoreButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleWindowState();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void NotificationGrid_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var sb = FindResource("ShowNotificationStoryboard") as Storyboard;
            sb?.Begin(NotificationGrid, false);
        }

        private void DialogGrid_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var newValue = (bool)e.NewValue;
            if (newValue)
            {
                DialogGrid.Visibility = Visibility.Visible;
                var sb = FindResource("DialogGridFadeInStoryboard") as Storyboard;
                sb?.Begin(DialogGrid, false);
                //sb.Completed += (s, e) => DialogContentControl.Focus();
            }
            else
            {
                var sb = FindResource("DialogGridFadeOutStoryboard") as Storyboard;
                sb?.Begin(DialogGrid, false);
                sb.Completed += (s, e) => DialogGrid.Visibility = Visibility.Collapsed;

                var binding = BindingOperations.GetBinding(DialogContentControl, ContentProperty);

                sb.Completed += (s, e) => DialogContentControl.Content = null;
            }
        }
        protected virtual Thickness GetDefaultMarginForDpi()
        {
            var currentDPI = SystemHelper.GetCurrentDPI();

            return currentDPI switch
            {
                120 => new Thickness(7, 7, 4, 5),
                144 => new Thickness(7, 7, 3, 1),
                168 => new Thickness(6, 6, 2, 0),
                192 => new Thickness(6, 6, 0, 0),
                240 => new Thickness(6, 6, 0, 0),
                _ => new Thickness(7, 7, 7, 7),
            };
        }

        protected virtual Thickness GetFromMinimizedMarginForDpi()
        {
            var currentDPI = SystemHelper.GetCurrentDPI();
            var thickness = new Thickness(7, 7, 5, 7);
            if (currentDPI == 120)
            {
                thickness = new Thickness(6, 6, 4, 6);
            }
            else if (currentDPI == 144)
            {
                thickness = new Thickness(7, 7, 4, 4);
            }
            else if (currentDPI == 168)
            {
                thickness = new Thickness(6, 6, 2, 2);
            }
            else if (currentDPI == 192)
            {
                thickness = new Thickness(6, 6, 2, 2);
            }
            else if (currentDPI == 240)
            {
                thickness = new Thickness(6, 6, 0, 0);
            }
            return thickness;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var screen = Screen.FromHandle(new WindowInteropHelper(this).Handle);
            var width = (double)screen.WorkingArea.Width;
            var workingArea = screen.WorkingArea;
            previousScreenBounds = new System.Windows.Point(width, (double)workingArea.Height);
        }

        private void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
        {
            var screen = Screen.FromHandle(new WindowInteropHelper(this).Handle);
            var width = (double)screen.WorkingArea.Width;
            var workingArea = screen.WorkingArea;
            previousScreenBounds = new System.Windows.Point(width, (double)workingArea.Height);
            RefreshWindowState();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
            {
                HeightBeforeMaximize = ActualHeight;
                WidthBeforeMaximize = ActualWidth;
                return;
            }
            if (WindowState == WindowState.Maximized)
            {
                var screen = Screen.FromHandle(new WindowInteropHelper(this).Handle);
                if (previousScreenBounds.X != (double)screen.WorkingArea.Width || previousScreenBounds.Y != (double)screen.WorkingArea.Height)
                {
                    var width = (double)screen.WorkingArea.Width;
                    var workingArea = screen.WorkingArea;
                    previousScreenBounds = new System.Windows.Point(width, (double)workingArea.Height);
                    RefreshWindowState();
                }
            }
        }

        private void OnStateChanged(object sender, EventArgs e)
        {
            var screen = Screen.FromHandle(new WindowInteropHelper(this).Handle);
            var thickness = new Thickness(0);
            if (WindowState != WindowState.Maximized)
            {
                var currentDPIScaleFactor = (double)SystemHelper.GetCurrentDPIScaleFactor();
                var workingArea = screen.WorkingArea;
                MaxHeight = (double)(workingArea.Height + 16) / currentDPIScaleFactor;
                MaxWidth = double.PositiveInfinity;

                SetMaximizeButtonsVisibility(Visibility.Visible, Visibility.Collapsed);
            }
            else
            {
                thickness = GetDefaultMarginForDpi();
                if (PreviousState == WindowState.Minimized || Left == positionBeforeDrag.X && Top == positionBeforeDrag.Y)
                {
                    thickness = GetFromMinimizedMarginForDpi();
                }

                SetMaximizeButtonsVisibility(Visibility.Collapsed, Visibility.Visible);
            }

            LayoutRoot.Margin = thickness;
            PreviousState = WindowState;
        }

        private void SetMaximizeButtonsVisibility(Visibility collapsed, Visibility visible)
        {
            MaximizeButton.Visibility = collapsed;
            RestoreButton.Visibility = visible;
        }

        private void OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!isMouseButtonDown)
            {
                return;
            }

            var currentDPIScaleFactor = (double)SystemHelper.GetCurrentDPIScaleFactor();
            var position = e.GetPosition(this);
            System.Diagnostics.Debug.WriteLine(position);
            var screen = PointToScreen(position);
            var x = mouseDownPosition.X - position.X;
            var y = mouseDownPosition.Y - position.Y;
            if (Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2)) > 1)
            {
                var actualWidth = mouseDownPosition.X;

                if (mouseDownPosition.X <= 0)
                {
                    actualWidth = 0;
                }
                else if (mouseDownPosition.X >= ActualWidth)
                {
                    actualWidth = WidthBeforeMaximize;
                }

                if (WindowState == WindowState.Maximized)
                {
                    ToggleWindowState();
                    Top = (screen.Y - position.Y) / currentDPIScaleFactor;
                    Left = (screen.X - actualWidth) / currentDPIScaleFactor;
                    CaptureMouse();
                }

                isManualDrag = true;

                Top = (screen.Y - mouseDownPosition.Y) / currentDPIScaleFactor;
                Left = (screen.X - actualWidth) / currentDPIScaleFactor;
            }
        }

        private void OnMouseButtonUp(object sender, MouseButtonEventArgs e)
        {
            isMouseButtonDown = false;
            isManualDrag = false;
            ReleaseMouseCapture();
        }

        private void RefreshWindowState()
        {
            if (WindowState == WindowState.Maximized)
            {
                ToggleWindowState();
                ToggleWindowState();
            }
        }
    }
}
