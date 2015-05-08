using System;
using System.Windows.Input;
namespace Netgun.Controls
{
    public class ActionCommand : ICommand
    {
        private readonly Action _action;

        public ActionCommand(Action action, Key key)
        {
            _action = action;
            Key = key;
        }

        public event EventHandler CanExecuteChanged { add {} remove {}}

        public Key Key { get; set; }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            _action();
        }
    }
}
