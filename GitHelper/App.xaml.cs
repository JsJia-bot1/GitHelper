using GitHelper.Common.Exceptions;
using GitHelper.ViewModels;
using GitHelper.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace GitHelper
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly IHost _host = Host.CreateDefaultBuilder()
        .ConfigureAppConfiguration(c =>
        {
            var basePath = Path.GetDirectoryName(AppContext.BaseDirectory)!;
            c.SetBasePath(basePath);
        })
        .ConfigureServices(
            (context, services) =>
            {
                // Main window
                services.AddSingleton<MainWindow>();

                // Initial page
                services.AddSingleton<InitialPage>();
                services.AddSingleton<InitialViewModel>();

                // Main page
                services.AddSingleton<MainPage>();
                services.AddSingleton<MainViewModel>();
            }
        )
        .Build();

        public static IServiceProvider ServiceProvider { get; private set; } = null!;


#pragma warning disable S2325 // Methods and properties that don't access instance data should be static

        /// <summary>
        /// Occurs when the application is loading.
        /// </summary>
        private async void OnStartup(object sender, StartupEventArgs e)
        {
            await _host.StartAsync();
            ServiceProvider = _host.Services;
            MainWindow window = ServiceProvider.GetRequiredService<MainWindow>();
            window.Show();
        }

        /// <summary>
        /// Occurs when the application is closing.
        /// </summary>
        private async void OnExit(object sender, ExitEventArgs e)
        {
            await _host.StopAsync();
            _host.Dispose();
        }

        private void OnException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            if (e.Exception is HandledException)
            {
                MessageBox.Show(e.Exception.Message);
            }
            else
            {
                MessageBox.Show($"Unhandled exception occurred: {e.Exception.Message} | {e.Exception.StackTrace}");
            }

            e.Handled = true;
        }
#pragma warning restore S2325 // Methods and properties that don't access instance data should be static

    }
}
