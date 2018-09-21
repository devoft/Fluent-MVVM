﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace devoft.ClientModel.Commanding
{
    public class DelegateCommand : ICommand
    {
        private readonly Predicate<object> _canExecute;
        private readonly Action<object> _execute;

        public event EventHandler CanExecuteChanged;

        public DelegateCommand(Action<object> execute)
                       : this(execute, null)
        {
        }

        public DelegateCommand(Action<object> execute, Predicate<object> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
            => _canExecute == null || _canExecute(parameter);

        public void Execute(object parameter)
            => _execute(parameter);

        public void Invalidate()
            => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

}
