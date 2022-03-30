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

namespace Messenger_Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MesClient cl;
        public List<Chat> chatList = new List<Chat>();
        private Dictionary<string, int> notifsCount = new Dictionary<string, int>();
        public Dictionary<string, List<string> > chats = new Dictionary<string, List<string> >();
        public MainWindow()
        {
            cl = new MesClient();
            InitializeComponent();
            Show();
            IsEnabled = false;
            Start_Chatting.IsEnabled = false;
            CreateLogin(false);
            cl.GotUsersOnline += (sender, e) =>
             {
                 Dispatcher.Invoke(() =>
                 {

                     while (OnlineUsersList.Items.Count != 0)
                     {
                     
                         OnlineUsersList.Items.RemoveAt(0);
                         OnlineUsersList.Items.Refresh();
                     }
                 });


                 foreach (string user in cl.UsersOnline)
                 {
                     if (!chats.ContainsKey(user))
                     {
                         chats[user] = new List<string>();
                     }
                     if (!notifsCount.ContainsKey(user))
                     {
                         notifsCount[user] = 0;
                     }
                     Dispatcher.Invoke(() =>
                     {
                         Grid grid = new Grid();
                         var first = new ColumnDefinition();
                         var second = new ColumnDefinition();
                         first.Width = new GridLength(2.0, GridUnitType.Star);
                         second.Width = new GridLength(1.0, GridUnitType.Star);
                         grid.ColumnDefinitions.Add(first);
                         grid.ColumnDefinitions.Add(second);
                         TextBlock newBlock = new TextBlock
                         {
                             Text = user,
                             Name = "UserName",
                             Style = (Style)Resources["mIRC_Font"]
                         };
                         TextBlock notif = new TextBlock
                         {
                             Name = "Notif",
                             Text = GetNotifCorrectly(notifsCount[user]),
                             Style = (Style)Resources["mIRC_Font"],
                             
                         };
                         
                         Grid.SetColumn(newBlock, 0);
                         Grid.SetColumn(notif, 1);
                         grid.Children.Add(newBlock);
                         grid.Children.Add(notif);
                         grid.MouseDown += ListUserMouseDownHandler;
                         OnlineUsersList.Items.Add(grid);

                     });
                 }
                 Dispatcher.Invoke(() =>
                 {
                     if (cl.UsersOnline.Count == 0)
                     {
                         Start_Chatting.IsEnabled = false;
                     }
                     else
                     {
                     
                         Start_Chatting.IsEnabled = true;
                         OnlineUsersList.SelectedIndex = 0;
                     }
                 });
             };

            cl.Disconnected += WhenDisconnected;
            this.Closing += (sender, e) =>
            {
                cl.Disconnected -= WhenDisconnected;
                cl.CloseConn();
                App.Current.Shutdown();
            };
            cl.MessageReceived += GotMessage;
        }

        private void ListUserMouseDownHandler(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                CreateChat();
            }
        }
        private string GetNotifCorrectly(int count)
        {
            if (count == 0)
            {
                return "";
            }
            else
            {
                return count + " new msgs";
            }
        }
        private void WhenDisconnected(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                CreateLogin(true);
            });
        }
        private void MakeChat(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() => {
                CreateChat();
            });
        }
        public void Connect()
        {
            IsEnabled = true;
            TopTextBlock.Text = $"Hello, {cl.userName}! Here are users online:";
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
            this.Close();
        }
        private void CreateLogin(bool disc)
        {
            //Dispatcher.Invoke(() =>
            //{
            //    Thread thread = new Thread(new ThreadStart(() =>
            //    {
            //        Login login = new Login(this);
            //        login.Owner = this;
            //        login.Show();
            //        login.ErrLabel.Text = "You got disconnected";
            //        System.Windows.Threading.Dispatcher.Run();
            //    }));
            //    thread.SetApartmentState(ApartmentState.STA);
            //    thread.IsBackground = true;
            //    thread.Start();
            //});
            IsEnabled = false;
            Login login = new Login(this);
            if (disc)
            {
                login.ErrLabel.Text = "You got disconnected";
            }
            login.Owner = this;
            login.Show();
        }
        private void CreateChat()
        {
            string to = ((TextBlock)(((Grid)OnlineUsersList.SelectedItem).Children[0])).Text;
            SetNotifTo0(to);

            Chat chat = new Chat(
                cl.userName, to, this)
            {
                Owner = this
            };
            chat.Show();
            chatList.Add(chat);
            foreach (var message in chats[to])
            {
                chat.CreateMessage(message);
            }
        }
        private void SetNotifTo0(string user)
        {
            notifsCount[((TextBlock)(((Grid)OnlineUsersList.SelectedItem).Children[0])).Text] = 0;
            foreach (Grid gr in OnlineUsersList.Items)
            {
                if (((TextBlock)(gr.Children[0])).Text == user)
                {
                    ((TextBlock)gr.Children[1]).Text = "";
                }
            }
        }
        private void GotMessage(object sender, MesReceivedEventArgs e)
        {
            bool found = false;
            foreach (Chat c in chatList)
            {
                if (c.who == e.From)
                {
                    found = true;
                    Dispatcher.Invoke(() =>
                    {
                        c.CreateMessage(c.who, e.Message);
                    });
                }
            }
            if (!found)
            {
                notifsCount[e.From] += 1;
                chats[e.From].Add($"<{e.From}>: {e.Message}");
                foreach (Grid g in OnlineUsersList.Items)
                {
                    Dispatcher.Invoke(() =>
                    {
                        if (((TextBlock)g.Children[0]).Text == e.From)
                        {
                            ((TextBlock)g.Children[1]).Text = GetNotifCorrectly(notifsCount[e.From]);
                        }
                    });
                }
            } 
        }
    }
}

