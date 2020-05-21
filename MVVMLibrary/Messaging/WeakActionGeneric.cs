using System;

namespace MVVMLibrary.Messaging
{
    public class WeakAction<T> : WeakAction, IExecuteWithObject
    {
        private readonly Action<T> _action;

        public WeakAction(object target, Action<T> action)
            : base(target, null)
        {
            _action = action;
        }

        public new Action<T> Action
        {
            get
            {
                return _action;
            }
        }

        public new void Execute()
        {
            if (_action != null
                && IsAlive)
            {
                _action(default(T));
            }
        }

        public void Execute(T parameter)
        {
            if (_action != null
                && IsAlive)
            {
                _action(parameter);
            }
        }

        public void ExecuteWithObject(object parameter)
        {
            var parameterCasted = (T)parameter;
            Execute(parameterCasted);
        }
    }
}