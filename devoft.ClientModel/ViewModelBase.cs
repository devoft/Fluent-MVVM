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

        /// <summary>
        /// Creates a ViewModel with a default dispatcher object
        /// </summary>
        /// <param name="dispatcher"></param>
        public ViewModelBase(IDispatcher dispatcher)
        {
            Dispatcher = dispatcher;
        }

        public ViewModelBase()
        {
        }

        /// <summary>
        /// Used to initialize the ViewModel once the UI components are ready. 
        /// It can be used to register commands, to set initial values to properties, etc
        /// </summary>
        /// <param name="dispatcher">Dispatcher object used to invoke this method in the UI thread</param>
        public virtual Task InitializeAsync(IDispatcher dispatcher = null)
            => Task.CompletedTask;

        internal static Dictionary<string, object> PropertyDescriptors { get; } = new Dictionary<string, object>();

        /// <summary>
        /// Return whether there are some validation errors
        /// </summary>
        public bool HasErrors => _values.Values.Any(x => !x.ValidationResults.IsSucceded);
        
        /// <summary>
        /// Dispatcher object used to update properties on the UI thread
        /// </summary>
        public IDispatcher Dispatcher { get; set; }

        /// <summary>
        /// Event raised when any property value is changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Event raised when some errors happens in any property of the viewmodel.
        /// </summary>
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        /// <summary>
        /// It is intended to be used as the implementation of the <b>get</b> accessors of the properties on the inheritor ViewModel class. It returns the value set using <see cref="SetValue{T}(T, string)"/>
        /// </summary>
        /// <typeparam name="T">Type of the property</typeparam>
        /// <param name="propertyName">Name of the property</param>
        /// <param name="def">Initialization function used to return a default value when <see cref="SetValue{T}(T, string)"/> is not being called yet. When this method is used the <see cref="SetValue{T}(T, string)"/> is called to set the default value</param>
        /// <returns>The Value set through <see cref="SetValue{T}(T, string)"/></returns>
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

        /// <summary>
        /// It is intended to be used as the implementation of the <b>set</b> accessors of the properties on the inheritor ViewModel class. It returns the value set using <see cref="SetValue{T}(T, string)"/>
        /// </summary>
        /// <typeparam name="T">Type of the property</typeparam>
        /// <param name="value">The value assigned to the property</param>
        /// <param name="propertyName">Name of the property</param>
        /// <remarks>
        /// This method ensures the following in order:
        /// - Apply validations on the property. <see cref="ViewModelPropertyDescriptor{TOwner, TResult}.ValidateAlways"/>, <see cref="HasErrors"/>, <see cref="GetValidationResults(string)"/> 
        /// - Apply coerce on new values
        /// - Validate coerced values if desired
        /// - Assign value to the property
        /// - Apply value setters on the property
        /// - Record the property set if the property assignation is in an edition scope
        /// - Notify property change when property is not in an edition scope
        /// </remarks>
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

        /// <summary>
        /// Fluent entry point to configure the viewmodel's property specified through the param: <paramref name="exp"/> (e.g. RegisterProperty(myViewModel => myViewModel.Property)) 
        /// </summary>
        /// <typeparam name="TResult">Type of the property</typeparam>
        /// <param name="exp">Property reference expression</param>
        /// <returns>A <see cref="ViewModelPropertyDescriptor{TOwner, TResult}"/> describing the property specified through <paramref name="exp"/></returns>
        protected static ViewModelPropertyDescriptor<TInheritor, TResult> RegisterProperty<TResult>(Expression<Func<TInheritor, TResult>> exp)
            => RegisterProperty<TResult>((exp.Body as MemberExpression).Member.Name);

        /// <summary>
        /// Fluent entry point to configure the viewmodel's property specified through the param: <paramref name="propertyName"/> (e.g. RegisterProperty(nameof(Property))) 
        /// </summary>
        /// <typeparam name="TResult">Type of the property</typeparam>
        /// <param name="propertyName">Property name</param>
        /// <returns>A <see cref="ViewModelPropertyDescriptor{TOwner, TResult}"/> describing the property specified through <paramref name="exp"/></returns>
        protected internal static ViewModelPropertyDescriptor<TInheritor, TResult> RegisterProperty<TResult>(string propertyName)
        {
            ViewModelPropertyDescriptor<TInheritor, TResult> result;
            if (PropertyDescriptors.TryGetValue(propertyName, out var desc))
                result = desc as ViewModelPropertyDescriptor<TInheritor, TResult>;
            else
                PropertyDescriptors.Add(propertyName, result = new ViewModelPropertyDescriptor<TInheritor, TResult>(propertyName));
            return result;
        }

        /// <summary>
        /// Clear all existing validation results 
        /// </summary>
        public void ClearValidationResults()
        {
            foreach (var val in _values.Values)
                val.ValidationResults.Clear();
        }

        /// <summary>
        /// Return the validations results for the specified <paramref name="propertyName"/>
        /// </summary>
        /// <param name="propertyName">The name of the property validated</param>
        /// <returns><see cref="ValidationResultCollection"/> containing the last validation results</returns>
        public ValidationResultCollection GetValidationResults(string propertyName)
            => _values[propertyName].ValidationResults;

        /// <summary>
        /// Notifies the change of the property specified on <paramref name="propertyName"/>. 
        /// Also invalidates the registered commands in order to verify if they can be executed
        /// </summary>
        /// <param name="propertyName">Name of the property changed</param>
        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            InvalidateCommands();
        }

        /// <summary>
        /// Called to raise the <see cref="ErrorsChanged"/> for the property <paramref name="propertyName"/>
        /// </summary>
        /// <param name="propertyName"></param>
        protected internal virtual void OnErrorChanged([CallerMemberName] string propertyName = null)
            => ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));

        /// <summary>
        /// Obtains the errors associated with the property specified by <paramref name="propertyName"/>. 
        /// It's a filter over the method <see cref="GetValidationResults(string)"/>. 
        /// If the <paramref name="propertyName"/> is null, then it searches for and returns all errors of every property registered 
        /// </summary>
        /// <param name="propertyName">The name of the property failing</param>
        /// <returns>A collection of <see cref="ValidationResult"/> representing the errors ocurred on the property or class.</returns>
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
