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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;

namespace Download_Manager_Server
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Server serv;
        Thread servThread;
        public MainWindow()
        {
            InitializeComponent();
            Server.ServerLaunched += ServerLaunched;
            this.Closing += (sender, e) => {
                Application.Current.Shutdown();
            };
        }
        public void Start(object sender, RoutedEventArgs e)
        {
            StartButton.Click -= Start;
            serv = new Server();    
            ServersClient.RefreshUsersList += RefreshOnlineUsers;

        }
        public void RefreshOnlineUsers(object sender, EventArgs e)
        {
            //for (int i = UsersBox.Items.Count - 1; i >= 0; i--)
            //{
            //    Dispatcher.Invoke(() =>
            //    {
            //        UsersBox.Items.RemoveAt(i);
            //        UsersBox.Items.Refresh();
            //    });
            //}
            foreach (string user in serv.getUsernamesOnline())
            {
                Dispatcher.Invoke(() =>
                {
                    TextBlock newUserBox = new TextBlock();
                    newUserBox.Text = user;
                    UsersBox.Items.Add(newUserBox);                
                });
            }
        }
        private void ServerLaunched(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                IPblock.Text=$"Your IP is {serv.ip.ToString()}:{serv.port}";
            });
        }
        private void Closing_(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }
    }
}
