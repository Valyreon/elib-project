using System;
using System.Windows.Input;

namespace Valyreon.Elib.Mvvm
{
    public class RelayCommand<T> : ICommand
    {
        private readonly Func<bool> canExecuteEvaluator;

        private readonly Action<T> methodToExecute;

        public RelayCommand(Action<T> methodToExecute, Func<bool> canExecuteEvaluator)
        {
            this.methodToExecute = methodToExecute;
            this.canExecuteEvaluator = canExecuteEvaluator;
        }

        public RelayCommand(Action<T> methodToExecute)
            : this(methodToExecute, null)
        {
        }

        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object parameter)
        {
            if (canExecuteEvaluator == null)
            {
                return true;
            }

            return canExecuteEvaluator.Invoke();
        }

        public void Execute(object parameter)
        {
            methodToExecute.Invoke((T)parameter);
        }
    }
}
