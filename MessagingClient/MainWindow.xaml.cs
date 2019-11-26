using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace MessagingClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var thisButton = (Button)sender;
            var controlID = thisButton.Name.Substring(thisButton.Name.Length - 1);
            var textBox = (TextBox)FindName("FileName" + controlID);
            var dlg = new CommonOpenFileDialog();

            dlg.Title = "Select folder to save files";
            dlg.IsFolderPicker = false;
            dlg.AddToMostRecentlyUsedList = false;
            dlg.AllowNonFileSystemItems = false;
            dlg.EnsureFileExists = true;
            dlg.EnsurePathExists = true;
            dlg.EnsureReadOnly = false;
            dlg.EnsureValidNames = true;
            dlg.Multiselect = false;
            dlg.ShowPlacesList = true;

            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                textBox.Text = dlg.FileName;
                var messageClient = new MessagesSender();
                var fileStream = new FileStream(dlg.FileName, FileMode.Open);
                messageClient.SendMessage(fileStream);
            }
        }
    }
}
