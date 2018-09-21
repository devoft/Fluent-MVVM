using System.Windows.Input;

namespace devoft.ClientModel.Commanding
{
    public interface INamedCommand : ICommand
    {
        string Name { get; }
    }
}
