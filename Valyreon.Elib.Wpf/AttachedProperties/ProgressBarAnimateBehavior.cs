using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using Microsoft.Xaml.Behaviors;

namespace Valyreon.Elib.Wpf.AttachedProperties
{
    public class ProgresBarAnimateBehavior : Behavior<ProgressBar>
    {
        private bool _IsAnimating = false;

        protected override void OnAttached()
        {
            base.OnAttached();
            var progressBar = AssociatedObject;
            progressBar.ValueChanged += ProgressBar_ValueChanged;
        }

        private void ProgressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_IsAnimating)
            {
                return;
            }

            _IsAnimating = true;

            var doubleAnimation = new DoubleAnimation
                (e.OldValue, e.NewValue, new Duration(TimeSpan.FromSeconds(0.3)), FillBehavior.Stop);
            doubleAnimation.Completed += Db_Completed;

            ((ProgressBar)sender).BeginAnimation(System.Windows.Controls.Primitives.RangeBase.ValueProperty, doubleAnimation);

            e.Handled = true;
        }

        private void Db_Completed(object sender, EventArgs e)
        {
            _IsAnimating = false;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            var progressBar = AssociatedObject;
            progressBar.ValueChanged -= ProgressBar_ValueChanged;
        }
    }
}
