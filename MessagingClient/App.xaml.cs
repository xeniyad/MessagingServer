using System.Windows;
using ServiceBusHelper;

namespace MessagingClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Здесь мы передаем настройки главному окну, и так главное окно не знает, откуда ему провайднули настройки.
            // Здесь уже можно читать из файла или еще откуда-то.
            // https://stackoverflow.com/questions/47370784/start-two-windows-in-sequence-from-app-xaml-cs-wpf
            var mainWindow = new MainWindow(new ClientSettingsDto());
            mainWindow.Show();
        }
    }
}
