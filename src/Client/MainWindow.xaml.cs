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

namespace Messenger_Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Login parent;
        public MainWindow(Login wind)
        {

            InitializeComponent();
            this.parent = wind;

            parent.cl.GotUsersOnline += (sender, e) =>
             {
                 Dispatcher.Invoke(() =>
                 {

                     while (OnlineUsersList.Items.Count != 0)
                     {
                     
                         OnlineUsersList.Items.RemoveAt(0);
                         OnlineUsersList.Items.Refresh();
                     }
                 });


                 foreach (string user in parent.cl.UsersOnline)
                 {
                     Dispatcher.Invoke(() =>
                     {
                         
                         TextBlock newBlock = new TextBlock();
                         newBlock.Text = user;
                         newBlock.PreviewMouseDown += (sender, e) =>
                          {
                              chatWith.Text = newBlock.Text;
                          };
                         OnlineUsersList.Items.Add(newBlock);

                     });
                 }
             };

            parent.cl.Disconnected += (sender, e) =>
            {
                Dispatcher.Invoke(() =>
                {
                    parent.Show();
                    parent.ErrLabel.Text = "You got disconnected";
                    this.Close();
                });
            };
            parent.cl.UserAvailable += (sender, e) =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (e.IsAvailable)
                    {
                        chatWith.Text = e.UserName;
                    }
                    else
                    {
                        chatWith.Text = "No one";
                    }
                });
            };
            parent.cl.MessageReceived += (obj, e) =>
            {
                Dispatcher.Invoke(() =>
                {
                    var newItem = new TextBlock();
                    newItem.Text = $"You <- {e.From}: {e.Message}";

                    msgs.Items.Add(newItem);
                });
            };
            
        }
        public void OnTxtBoxInput(object sender, RoutedEventArgs e)
        {
            Notify.Visibility = Visibility.Hidden;
            TypedTxt.TextChanged -= OnTxtBoxInput;
        }
        public void sendMessage(object sender, RoutedEventArgs e)
        {
            if (chatWith.Text.ToLower() != "all")
            {
                var mymsg = new TextBlock();
                mymsg.Text = $"You -> {parent.cl.userName}: {TypedTxt.Text}";
                msgs.Items.Add(mymsg);
            }

            parent.cl.SendMessage(chatWith.Text, TypedTxt.Text);

            TypedTxt.Text = "";
        }
        public void FullyHide()
        {
            this.ShowInTaskbar = false;
            this.Visibility = Visibility.Hidden;
        }
        public void FullyShow()
        {
            this.ShowInTaskbar = true;
            this.Visibility = Visibility.Visible;
        }
        public void CloseAll(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() => parent.Close());
            this.Close();
        }
    }

}

