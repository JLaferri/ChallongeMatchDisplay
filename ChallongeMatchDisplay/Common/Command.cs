using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Fizzi.Applications.ChallongeVisualization.Common
{
    public class Command : ICommand
    {
        public static Command<TIn> Create<TIn>(Func<TIn, bool> canExecute, Action<TIn> execute)
        {
            return new Command<TIn>(canExecute, execute);
        }

        public static Command Create(Func<bool> canExecute, Action execute)
        {
            return new Command(canExecute, execute);
        }

        private readonly Func<bool> _canExecute;
        private readonly Action _execute;

        public Command(Func<bool> canExecute, Action execute)
        {
            _canExecute = canExecute;
            _execute = execute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute();
        }

        public void Execute(object parameter)
        {
            _execute();
        }

        public event EventHandler CanExecuteChanged { add { } remove { } }
    }

    public class Command<TIn> : ICommand
    {
        private readonly Func<TIn, bool> _canExecute;
        private readonly Action<TIn> _execute;

        public Command(Func<TIn, bool> canExecute, Action<TIn> execute)
        {
            _canExecute = canExecute;
            _execute = execute;
        }

        public bool CanExecute(object parameter)
        {
            TIn input;
            try { input = (TIn)parameter; }
            catch { return false; }

            return _canExecute(input);
        }

        public void Execute(object parameter)
        {
            _execute((TIn)parameter);
        }

        public event EventHandler CanExecuteChanged { add { } remove { } }
    }
}
