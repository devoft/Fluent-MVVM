using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using WpfDemo.Views.Contacts;
using WpfDemo.Views.Main;

namespace WpfDemo
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static ServiceProvider Services { get; private set; }

        public App()
        {
            InjectDependencies();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            MainWindow = Services.GetRequiredService<MainWnd>();
            MainWindow.Show();
        }

        private void InjectDependencies()
        {
            var services = new ServiceCollection();
            Configure(services);
            Services = services.BuildServiceProvider();
        }

        private void Configure(ServiceCollection services)
        {
            services.AddTransient<MainVM>();
            services.AddTransient<MainWnd>();
            services.AddTransient<RegisterUserVM>();
            services.AddTransient<RegisterWnd>();
        }
    }
}
