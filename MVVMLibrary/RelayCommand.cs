using System;
using System.Windows.Input;

namespace MVVMLibrary
{
    public class RelayCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }

        private readonly Action methodToExecute;
        private readonly Func<bool> canExecuteEvaluator;

        public RelayCommand(Action methodToExecute, Func<bool> canExecuteEvaluator)
        {
            this.methodToExecute = methodToExecute;
            this.canExecuteEvaluator = canExecuteEvaluator;
        }

        public RelayCommand(Action methodToExecute)
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
            methodToExecute.Invoke();
        }
    }
}
