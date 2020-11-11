using System;
using System.Windows.Input;

namespace MVVMLibrary
{
    public class RelayCommand<T> : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }

        private readonly Action<T> methodToExecute;
        private readonly Func<bool> canExecuteEvaluator;

        public RelayCommand(Action<T> methodToExecute, Func<bool> canExecuteEvaluator)
        {
            this.methodToExecute = methodToExecute;
            this.canExecuteEvaluator = canExecuteEvaluator;
        }

        public RelayCommand(Action<T> methodToExecute)
            : this(methodToExecute, null)
        {
        }

        public bool CanExecute(object parameter)
        {
            if (canExecuteEvaluator == null)
            {
                return true;
            }
            else
            {
                var result = canExecuteEvaluator.Invoke();
                return result;
            }
        }

        public void Execute(object parameter)
        {
            methodToExecute.Invoke((T)parameter);
        }
    }
}
