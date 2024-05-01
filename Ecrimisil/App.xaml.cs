using System.Windows;
using System.Windows.Threading;

namespace Ecrimisil
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e) => Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;

        private void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            _ = Current.Dispatcher.Invoke(() => MessageBox.Show(e.Exception?.Message, "ECRİMİSİL", MessageBoxButton.OK, MessageBoxImage.Warning));
            e.Handled = true;
        }
    }
}
