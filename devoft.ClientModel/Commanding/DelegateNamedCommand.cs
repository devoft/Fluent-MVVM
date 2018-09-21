using System;

namespace devoft.ClientModel.Commanding
{
    public class DelegateNamedCommand : DelegateCommand, INamedCommand
    {
        public DelegateNamedCommand(string name, Action<object> execute)
            : this(name, execute, null)
        {
        }

        public DelegateNamedCommand(string name, Action<object> execute, Predicate<object> canExecute)
            : base(execute, canExecute)
        {
            Name= name;
        }

        public string Name { get; }
    }

}
