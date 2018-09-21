using System;

namespace devoft.ClientModel.Commanding
{
    public class CommandExecuteEventArgs : EventArgs
    {
        public CommandExecuteEventArgs(INamedCommand command)
        {
            Command = command;
        }

        public INamedCommand Command { get; }
    }
}
