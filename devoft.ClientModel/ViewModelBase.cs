using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using devoft.ClientModel.Validation;
using devoft.ClientModel.Commanding;
using System.Windows.Input;
using devoft.Core.Patterns;
using System.Threading.Tasks;
using devoft.Core.Patterns.Scoping;
using devoft.Core.System.Collections.Generic;

namespace devoft.ClientModel
{
    public class ViewModelBase<TInheritor> : IPropertyChangedNotifier, INotifyDataErrorInfo, IDisposable, IUndoable
        where TInheritor : ViewModelBase<TInheritor>
    {
        protected internal Dictionary<string, IViewModelProperty> _values = new Dictionary<string, IViewModelProperty>();
        private Journal<IViewModelScopeTask<TInheritor>> _scopesJournal = new Journal<IViewModelScopeTask<TInheritor>>();

        public ViewModelBase(IDispatcher dispatcher)
        {
            Dispatcher = dispatcher;
        }

        public ViewModelBase()
        {
        }

        public virtual Task InitializeAsync(IDispatcher dispatcher = null)
            => Task.CompletedTask;

        internal static Dictionary<string, object> PropertyDescriptors { get; } = new Dictionary<string, object>();

        public bool HasErrors => _values.Values.Any(x => !x.ValidationResults.IsSucceded);
        public IDispatcher Dispatcher { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public T GetValue<T>([CallerMemberName] string propertyName = null, Func<T> def = null)
        {
            if (_values.TryGetValue(propertyName, out IViewModelProperty property))
                return ((ViewModelProperty<TInheritor, T>)property).Value;
            if (def != null)
            {
                var defValue = def();
                SetValue(defValue, propertyName);
                return defValue;
            }
            return default(T);
        }

        protected virtual bool SetValue<T>(T value, [CallerMemberName] string propertyName = null)
        {
            if (!_values.TryGetValue(propertyName, out IViewModelProperty prop))
            {
                if (!PropertyDescriptors.ContainsKey(propertyName))
                    PropertyDescriptors.Add(propertyName, new ViewModelPropertyDescriptor<TInheritor, T>(propertyName));
                prop = new ViewModelProperty<TInheritor, T>();
                _values.Add(propertyName, prop);
            }

            return ((ViewModelProperty<TInheritor, T>)prop).SetValue((TInheritor)this, propertyName, value);
        }

        protected static ViewModelPropertyDescriptor<TInheritor, TResult> RegisterProperty<TResult>(Expression<Func<TInheritor, TResult>> exp)
            => RegisterProperty<TResult>((exp.Body as MemberExpression).Member.Name);

        protected internal static ViewModelPropertyDescriptor<TInheritor, TResult> RegisterProperty<TResult>(string propertyName)
        {
            ViewModelPropertyDescriptor<TInheritor, TResult> result;
            if (PropertyDescriptors.TryGetValue(propertyName, out var desc))
                result = desc as ViewModelPropertyDescriptor<TInheritor, TResult>;
            else
                PropertyDescriptors.Add(propertyName, result = new ViewModelPropertyDescriptor<TInheritor, TResult>(propertyName));
            return result;
        }

        public void ClearValidationResults()
        {
            foreach (var val in _values.Values)
                val.ValidationResults.Clear();
        }

        public ValidationResultCollection GetValidationResults(string propertyName)
            => _values[propertyName].ValidationResults;

        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            InvalidateCommands();
        }

        protected internal virtual void OnErrorChanged([CallerMemberName] string propertyName = null)
            => ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));

        public IEnumerable GetErrors(string propertyName)
            => string.IsNullOrWhiteSpace(propertyName)
                    ? _values.Values.SelectMany(v => v.ValidationResults.Where(vr => vr.Kind != ValidationKind.Success))
                    : _values.TryGetValue(propertyName, out var prop) 
                        ? prop.ValidationResults.Where(vr => vr.Kind != ValidationKind.Success)
                        : null;

        #region [ IUndoable ]

        public async Task Undo()
        {
            if (!_scopesJournal.CanGoBack)
                return;

            await _scopesJournal.Peek().UndoAsync();
            _scopesJournal.GoBack();
        }

        public async Task Redo()
        {
            if (!_scopesJournal.CanGoForward)
                return;

            _scopesJournal.GoForward();
            await _scopesJournal.Peek().RedoAsync();
        }

        #endregion

        public IViewModelScopeTask<TInheritor> ActiveScope
        {
            get => _scopesJournal.IsOutOfrange ? null : _scopesJournal.Peek();
            set
            {
                if (ActiveScope != value)
                    _scopesJournal.Push(value);
            }
        }

        #region [ Commands ]

        public ExpandoObject Commands { get; } = new ExpandoObject();

        public ICommand RegisterCommand(
            string name,
            Action<object> execute,
            Func<ViewModelBase<TInheritor>, object, bool> canExecuteCondition = null)
        {
            /// TODO: Extract properties from condition
            var command = new DelegateNamedCommand(
                name,
                canExecute: x => canExecuteCondition?.Invoke(this, x) ?? true,
                execute: x =>
                {
                    execute(x);
                    OnCommandExecuted(name);
                });
            return RegisterCommand(name, command);
        }

        public ICommand RegisterCommand(string name, ICommand command)
        {
            ((IDictionary<string, object>)Commands)[name] = command;
            return command;
        }

        public void SubscribeToCommand(string commandName, EventHandler<CommandExecuteEventArgs> handler)
        {
            lock (_commandHandlersByName)
                if (!_commandHandlersByName.ContainsKey(commandName))
                    _commandHandlersByName.Add(commandName, handler);
                else 
                    _commandHandlersByName[commandName] += handler;
        }

        Dictionary<string, EventHandler<CommandExecuteEventArgs>> _commandHandlersByName = new Dictionary<string, EventHandler<CommandExecuteEventArgs>>();
        EventHandler<CommandExecuteEventArgs> _commandExecuted;
        public event EventHandler<CommandExecuteEventArgs> CommandExecuted
        {
            add { ConcurrencyHelper.SafeChange(ref _commandExecuted, x => x + value); }
            remove { ConcurrencyHelper.SafeChange(ref _commandExecuted, x => x - value); }
        }

        public void ExecuteCommand(string commandName, object parameter)
            => ((ICommand)((IDictionary<string, object>)Commands)[commandName])?.Execute(parameter);

        protected virtual void OnCommandExecuted(string commandName)
        {
            var args = new CommandExecuteEventArgs(((IDictionary<string, object>)Commands)[commandName] as INamedCommand);
            _commandExecuted?.Invoke(this, args);
            if (_commandHandlersByName.TryGetValue(commandName, out var handler))
                handler?.Invoke(this, args);
        }

        private void InvalidateCommands()
            => ((IDictionary<string, object>)Commands)
                       .Values
                       .OfType<DelegateCommand>()
                       .ForEach(command => command.Invalidate());

        #endregion

        public virtual void Dispose()
        {
            foreach (var command in ((IDictionary<string, object>)Commands).Values.OfType<IDisposable>())
                command.Dispose();
            ((IDictionary<string, object>)Commands).Clear();
            _values.Clear();
        }
    }

    public static class ViewModelExtensions
    {
        public static void AddValidationResult<T, TResult>(this T vm, Expression<Func<T, TResult>> propExp, string message, ValidationKind kind)
            where T : ViewModelBase<T>
        {
            var propertyName = (propExp.Body as MemberExpression).Member.Name;
            if (vm._values.TryGetValue(propertyName, out IViewModelProperty prop))
                ((ViewModelProperty<T, TResult>)prop).AddValidationResult(vm, propertyName, message, kind);
        }
    }
}
