using devoft.System;
using System;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace WpfDemo
{
    public class WPFDistpatcher : IDispatcher
    {
        private Dispatcher _dispatcher;

        public WPFDistpatcher(Dispatcher dispatcher)
            => _dispatcher = dispatcher;

        public async Task InvokeAsync(Action action) 
            => await _dispatcher.InvokeAsync(action);
    }
}
