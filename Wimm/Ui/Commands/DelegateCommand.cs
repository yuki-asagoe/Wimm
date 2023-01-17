using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Wimm.Ui.Commands
{
    public class DelegateCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        private Action handler;
        private Func<bool>? canExecute;
        public DelegateCommand(Action handler,Func<bool>? canExecuteHandler=null)
        {
            this.handler = handler;
            this.canExecute = canExecuteHandler;
        }
        public bool CanExecute(object? parameter)
        {
            return canExecute?.Invoke() ?? true;
        }

        public void Execute(object? parameter)
        {
            handler();
        }
    }
}
