using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Markup;

namespace ElibWpf.CustomComponents
{
    [ContentProperty("Actions")]
    public class ConditionalEventTrigger : FrameworkContentElement
    {
        public static readonly DependencyProperty ConditionProperty =
            DependencyProperty.Register("Condition", typeof(bool), typeof(ConditionalEventTrigger));

        public static readonly DependencyProperty TriggersProperty = DependencyProperty.RegisterAttached("Triggers",
            typeof(ConditionalEventTriggerCollection), typeof(ConditionalEventTrigger), new PropertyMetadata
            {
                PropertyChangedCallback = (obj, e) =>
                {
                    // When "Triggers" is set, register handlers for each trigger in the list
                    FrameworkElement element = (FrameworkElement)obj;
                    var triggers = (List<ConditionalEventTrigger>)e.NewValue;
                    foreach (ConditionalEventTrigger trigger in triggers)
                    {
                        element.AddHandler(trigger.RoutedEvent, new RoutedEventHandler((obj2, e2) =>
                            trigger.OnRoutedEvent(element)));
                    }
                }
            });

        private static readonly RoutedEvent _triggerActionsEvent = EventManager.RegisterRoutedEvent("",
            RoutingStrategy.Direct, typeof(EventHandler), typeof(ConditionalEventTrigger));

        public ConditionalEventTrigger()
        {
            this.Actions = new List<TriggerAction>();
        }

        public List<TriggerAction> Actions { get; set; }

        // Condition
        public bool Condition
        {
            get => (bool)this.GetValue(ConditionProperty);
            set => this.SetValue(ConditionProperty, value);
        }

        public RoutedEvent RoutedEvent { get; set; }

        // "Triggers" attached property
        public static ConditionalEventTriggerCollection GetTriggers(DependencyObject obj)
        {
            return (ConditionalEventTriggerCollection)obj.GetValue(TriggersProperty);
        }

        public static void SetTriggers(DependencyObject obj, ConditionalEventTriggerCollection value)
        {
            obj.SetValue(TriggersProperty, value);
        }

        // When an event fires, check the condition and if it is true fire the actions
        private void OnRoutedEvent(FrameworkElement element)
        {
            this.DataContext = element.DataContext; // Allow data binding to access element properties
            if (this.Condition)
            {
                // Construct an EventTrigger containing the actions, then trigger it
                EventTrigger dummyTrigger = new EventTrigger { RoutedEvent = _triggerActionsEvent };
                foreach (TriggerAction action in this.Actions)
                {
                    dummyTrigger.Actions.Add(action);
                }

                element.Triggers.Add(dummyTrigger);
                try
                {
                    element.RaiseEvent(new RoutedEventArgs(_triggerActionsEvent));
                }
                finally
                {
                    element.Triggers.Remove(dummyTrigger);
                }
            }
        }
    }

    // Create collection type visible to XAML - since it is attached we cannot construct it in code
    public class ConditionalEventTriggerCollection : List<ConditionalEventTrigger> { }
}