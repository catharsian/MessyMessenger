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

namespace Messenger_Client
{
    /// <summary>
    /// Interaction logic for Chat.xaml
    /// </summary>
    public partial class Chat : Window
    {
        MainWindow parent;
        public string who;
        private string me;
        public Chat(string me, string name, MainWindow s)
        {
            InitializeComponent();
            Owner = parent;
            UserNameBlock.Text = $"You're chatting with {name}" ;
            Title = $"{me} -> {name}";
            parent = s;
            who = name;
            this.me = me;
            Closing += Chat_Closing;
        }
        private void SendMsg(object sender, EventArgs e)
        {
            if (MessageBox.Text != "")
            {
                parent.cl.SendMessage(who, MessageBox.Text);
                CreateMessage(me, MessageBox.Text);
                MessageBox.Text = "";
            }
        }
        private void Chat_Closing(object sender, EventArgs e)
        {
            parent.chatList.Remove(this);
        }
        public void CreateMessage(string user, string msg)
        {
            TextBlock newBlock = new TextBlock();
            
            newBlock.Text = $"<{user}>: {msg}";
            newBlock.Style = (Style)Resources["mIRC_Font"];
            newBlock.TextWrapping = TextWrapping.Wrap;
            MessagesList.Items.Add(newBlock);
        }

        public void CreateMessage(string prevMsg)
        {
            TextBlock newBlock = new TextBlock();
            newBlock.Style = (Style)Resources["mIRC_Font"];
            newBlock.TextWrapping = TextWrapping.Wrap;
            newBlock.Text = prevMsg;
            MessagesList.Items.Add(newBlock);
        }
        private void AlwaysCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
    }
}
