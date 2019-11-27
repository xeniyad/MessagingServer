using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
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
        private SBClientManager _messageClient;
        private readonly Timer mainTimer;

        public MainWindow()
        {
            InitializeComponent();
            status = SBClientStatuses.WaitingForFile;
            var settings = new ClientSettingsDto();
            _messageClient = new SBClientManager(settings, new ConsoleLogger(), UpdateProgress, "MyClient1");
            mainTimer = new Timer(CheckServerStatus);
            mainTimer.Change(0, settings.StatusSendPeriodMs);
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
               
                var file = new Microsoft.ServiceBus.Messaging.BrokeredMessage(new FileStream(dlg.FileName, FileMode.Open));
                var fileMessage = new FileMessage(Path.GetFileName(dlg.FileName), file);
                Task.Factory.StartNew(() => _messageClient.Send(fileMessage))
                            .ContinueWith((t) => FinishSend());
                
            }
            
        }

        private void UpdateProgress()
        {
            Dispatcher.Invoke(new System.Action(() => UploadProgress.Value += UploadProgress.Maximum / 7));
        }

        private void FinishSend()
        {
            Dispatcher.Invoke(new System.Action(() => UploadProgress.Value += UploadProgress.Maximum));
            MessageBox.Show($"File sended successfully!");
            status = SBClientStatuses.WaitingForFile;
        }

        private void CheckServerStatus(object target)
        {
            Task.Factory.StartNew(() => _messageClient.SendClientStatus(status));
            var serverStatus = _messageClient.GetServerStatus();
            Dispatcher.Invoke(new System.Action(() => ServerStatusLV.Items.Add(serverStatus))); 
        }

    }
}
