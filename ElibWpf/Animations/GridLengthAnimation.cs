using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace ElibWpf.Animations
{
    public class GridLengthAnimation : AnimationTimeline
    {
        public static readonly DependencyProperty FromProperty;

        public static readonly DependencyProperty ToProperty;

        static GridLengthAnimation()
        {
            FromProperty = DependencyProperty.Register("From", typeof(GridLength),
                typeof(GridLengthAnimation));

            ToProperty = DependencyProperty.Register("To", typeof(GridLength),
                typeof(GridLengthAnimation));
        }

        public GridLength From
        {
            get => (GridLength)this.GetValue(FromProperty);
            set => this.SetValue(FromProperty, value);
        }

        public override Type TargetPropertyType => typeof(GridLength);

        public GridLength To
        {
            get => (GridLength)this.GetValue(ToProperty);
            set => this.SetValue(ToProperty, value);
        }

        protected override Freezable CreateInstanceCore()
        {
            return new GridLengthAnimation();
        }

        public override object GetCurrentValue(object defaultOriginValue, object defaultDestinationValue,
            AnimationClock animationClock)
        {
            double fromVal = ((GridLength)this.GetValue(FromProperty)).Value;
            double toVal = ((GridLength)this.GetValue(ToProperty)).Value;
            if (fromVal > toVal)
            {
                return new GridLength((1 - animationClock.CurrentProgress.Value) *
                    (fromVal - toVal) + toVal, GridUnitType.Pixel);
            }

            return new GridLength(animationClock.CurrentProgress.Value *
                (toVal - fromVal) + fromVal, GridUnitType.Pixel);
        }
    }
}