using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Castle.DynamicProxy;
using LogHelper;
using Microsoft.WindowsAPICodePack.Dialogs;
using ServiceBusHelper;

namespace MessagingClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SBClientStatuses status;
        private readonly IMessageSend _messageClient;
        private readonly BrokerMessageSender _brokerMessageSender;
        private readonly Timer mainTimer;

        public MainWindow(ClientSettingsDto settings)
        {
            InitializeComponent();
            status = SBClientStatuses.WaitingForFile;

            // Неплохое решение насчет передачи Экшена UpdateProgress для апдейта прогрес-бара,
            // однако я бы лучше сделал подписку на событие, потому что тогда SBClientManager делает слишком много вещей.
            var generator = new ProxyGenerator();
            _messageClient =
                generator.CreateInterfaceProxyWithTarget<IMessageSend>(
                    new SBClientManager(settings, "MyClient1"), new LogInterceptor(settings.LogFilePath));
            _messageClient = new SBClientManager(settings, "MyClient1");
            _messageClient.FilePartSentNotify += UpdateProgress;

            mainTimer = new Timer(CheckServerStatus);
            mainTimer.Change(0, settings.StatusSendPeriodMs);
            _brokerMessageSender = new BrokerMessageSender(_messageClient);
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            UploadProgress.Value = 0;
            status = SBClientStatuses.UploadingFile;
            var dlg = new CommonOpenFileDialog();

            dlg.Title = "Select file to send";
            dlg.IsFolderPicker = false;

            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                _brokerMessageSender
                    .SendFile(dlg.FileName)
                    .ContinueWith((t) => FinishSend());
            }
        }

        private void UpdateProgress()
        {
            // 1. Не обязательно указывать new System.Action()
            // 2. Не очень понятно, что за число 7, почему именно на 7 частей делится прогресс.
            Dispatcher.Invoke(() => UploadProgress.Value += UploadProgress.Maximum / 7);
        }

        private void FinishSend()
        {
            Dispatcher.Invoke(() => UploadProgress.Value += UploadProgress.Maximum);
            MessageBox.Show($"File was sent successfully!");
            status = SBClientStatuses.WaitingForFile;
        }

        private void CheckServerStatus(object target)
        {
            Task.Factory.StartNew(() => _messageClient.SendClientStatus(status));
            var serverStatus = _messageClient.GetServerStatus();
            Dispatcher.Invoke(() => ServerStatusLV.Items.Add(serverStatus));
        }

    }
}
