using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Wimm.Ui.Commands
{
    public class ParamsDelegateCommand :ICommand
    {
        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        private Action<object?> handler;
        private Func<object?,bool>? canExecute;
        public ParamsDelegateCommand(Action<object?> handler, Func<object?,bool>? canExecuteHandler = null)
        {
            this.handler = handler;
            this.canExecute = canExecuteHandler;
        }
        public bool CanExecute(object? parameter)
        {
            return canExecute?.Invoke(parameter) ?? true;
        }

        public void Execute(object? parameter)
        {
            handler(parameter);
        }
    }
}
