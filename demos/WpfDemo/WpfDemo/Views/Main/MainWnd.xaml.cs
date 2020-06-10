using devoft.Core.Patterns.Mapping;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WpfDemo.Views.Contacts;

namespace WpfDemo.Views.Main
{
    /// <summary>
    /// Interaction logic for MainWnd.xaml
    /// </summary>
    public partial class MainWnd : Window
    {
        public MainWnd(MainVM viewModel)
        {
            DataContext = ViewModel = viewModel;
            InitializeComponent();
            viewModel.Dispatcher = new WPFDistpatcher(Dispatcher);
        }

        public MainVM ViewModel { get; private set; }

        private void AddUserBtn_Click(object sender, RoutedEventArgs e)
        {
            var wnd = App.Services.GetRequiredService<RegisterWnd>();
            if (wnd.ShowDialog() == true)
                ViewModel.ExecuteCommand("Add", wnd.ViewModel.MapTo(new UserViewModel()));
        }

        private async void UndoExecuted(object sender, ExecutedRoutedEventArgs e) 
            => await ViewModel.Undo();

        private void UndoCanExecute(object sender, CanExecuteRoutedEventArgs e)
            => e.CanExecute = ViewModel.CanUndo();

        private async void RedoExecuted(object sender, ExecutedRoutedEventArgs e)
            => await ViewModel.Redo();

        private void RedoCanExecute(object sender, CanExecuteRoutedEventArgs e)
            => e.CanExecute = ViewModel.CanRedo();
    }
}
