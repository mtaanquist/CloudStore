using System;
using System.Diagnostics;
using System.Windows.Input;

namespace CloudStoreApp.Commands
{
    public class RelayCommand : ICommand
    {
        private readonly Predicate<object?>? _canExecute;
        private readonly Action<object?> _execute;

        public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        [DebuggerStepThrough]
        public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public void Execute(object? parameter) => _execute(parameter);
    }

    // New generic version for better type safety
    public class RelayCommand<T> : ICommand
    {
        private readonly Predicate<T?>? _canExecute;
        private readonly Action<T?> _execute;

        public RelayCommand(Action<T?> execute, Predicate<T?>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        [DebuggerStepThrough]
        public bool CanExecute(object? parameter)
        {
            if (parameter is T typedParameter)
                return _canExecute?.Invoke(typedParameter) ?? true;
            
            if (parameter is null)
                return _canExecute?.Invoke(default) ?? true;
                
            return false;
        }

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public void Execute(object? parameter)
        {
            if (parameter is T typedParameter)
                _execute(typedParameter);
            else if (parameter is null)
                _execute(default);
        }
    }
}