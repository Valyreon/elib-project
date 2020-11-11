using System;

namespace MVVMLibrary.Messaging
{
    public class WeakAction<T> : WeakAction, IExecuteWithObject
    {
        public WeakAction(object target, Action<T> action)
            : base(target, null)
        {
            Action = action;
        }

        public new Action<T> Action { get; }

        public new void Execute()
        {
            if (Action != null
                && IsAlive)
            {
                Action(default);
            }
        }

        public void Execute(T parameter)
        {
            if (Action != null
                && IsAlive)
            {
                Action(parameter);
            }
        }

        public void ExecuteWithObject(object parameter)
        {
            var parameterCasted = (T)parameter;
            Execute(parameterCasted);
        }
    }
}
