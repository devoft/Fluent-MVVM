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

namespace WpfDemo.Views.Contacts
{
    /// <summary>
    /// Interaction logic for ContactEditorWnd.xaml
    /// </summary>
    public partial class RegisterWnd : Window
    {
        public RegisterWnd(RegisterUserVM viewModel)
        {
            InitializeComponent();
            DataContext = ViewModel = viewModel;
            viewModel.Dispatcher = new WPFDistpatcher(Dispatcher);
        }

        public RegisterUserVM ViewModel { get; private set; }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            ViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(RegisterUserVM.Success))
                    DialogResult = ViewModel.Success;
            };
        }
    }
}
