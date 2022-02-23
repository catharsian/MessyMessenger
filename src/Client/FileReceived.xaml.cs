using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using Microsoft.Win32;
namespace Download_Manager_Client
{
    /// <summary>
    /// Interaction logic for FileReceived.xaml
    /// </summary>
    public partial class FileReceived : Window
    {
        private Client client;
        private string _filename;
        public FileReceived(string filename, string from, Client client)
        {
            InitializeComponent();
            this.client = client;
            username.Text = $"{from} wants to send you a file.";
            this.filename.Text = $"Filename is {filename}";
            _filename = filename;
        }
        private void btnpressed(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn.Name == "declinebutton")
            {
                client.FileReceiveAnswer(false);
                this.Close();
            }
            else
            {
                if (pathBlock.Text.Contains(@"\"))
                {
                    client.SetFolder(pathBlock.Text);
                    client.FileReceiveAnswer(true);
                    this.Close();
                }
                else
                {

                }
            }
        }
        private void findPath(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog();
            dialog.Filter = $"Downloaded File (*{StringRegexps.FindExtension(_filename)}) | *{StringRegexps.FindExtension(_filename)}";
            dialog.InitialDirectory = Environment.CurrentDirectory;
            if (dialog.ShowDialog() == true)
            {
                pathBlock.Text = dialog.FileName;
            }
        }

    }
}
