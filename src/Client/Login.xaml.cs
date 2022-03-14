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
using System.Threading;


namespace Messenger_Client
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        MainWindow wind;
        private readonly Object _lock = new Object();
        private bool pressed = true;
        MainWindow parent;
        private bool IsNonCloseButtonClick = false;
        public Login(MainWindow caller)
        {
            InitializeComponent();
            parent = caller;
            parent.cl.NoSuchHost += (sender, e) =>
            {
                Dispatcher.Invoke(() =>
                {
                    pressed = true;
                    ErrLabel.Foreground = Brushes.DarkRed;
                    ErrLabel.Text = "Error. Server not available.";
                });
            };
            parent.cl.RegisterFailed += (obj, e) =>
            {
                Dispatcher.Invoke(() =>
                {
                    pressed = true;
                    ErrLabel.Foreground = Brushes.DarkRed;
                    ErrLabel.Text = "Error. " + ErrToStr(e.Error);
                });
            };
            parent.cl.LoginFailed += (obj, e) =>
            {
                Dispatcher.Invoke(() =>
                {
                    pressed = true;
                    ErrLabel.Foreground = Brushes.DarkRed;
                    ErrLabel.Text = "Error. " + ErrToStr(e.Error);
                });
            };
            parent.cl.LoginOK += (sender, e) =>
            {
                IsNonCloseButtonClick = true;
                Dispatcher.Invoke(() =>
                {
                    parent.Connect();
                    this.Close();
                });
            };
            this.Closing += (sender, e) =>
            {
                if (IsNonCloseButtonClick)
                {
                    return;
                }
                else
                {
                    Dispatcher.Invoke(() =>
                    {
                        parent.Close();
                    });
                }
            };
        }
        public void OnTxtBoxInput(object sender, EventArgs e)
        {
            if (sender == passwordtxtbox)
            {
                passwordlabel.Visibility = Visibility.Hidden;
                passwordtxtbox.TextChanged -= OnTxtBoxInput;
            }
            else if (sender == usernametxtbox)
            {
                usernamelabel.Visibility = Visibility.Hidden;
                usernametxtbox.TextChanged -= OnTxtBoxInput;
            }
        }
        public void TryLogin(object sender, EventArgs e)
        {
            if (pressed)
            {
                pressed = false;
                ErrLabel.Foreground = Brushes.Blue;
                ErrLabel.Text = "Connecting...";
                parent.cl.Login(usernametxtbox.Text, passwordtxtbox.Text);

            }
        }
        public void TryRegister(object sender, EventArgs e)
        {
            if (pressed)
            {
                pressed = false;
                ErrLabel.Foreground = Brushes.Blue;
                ErrLabel.Text = "Connecting...";
                parent.cl.Register(usernametxtbox.Text, passwordtxtbox.Text);
            }
        }
        private static string ErrToStr(ClError err)
        {
            switch (err)
            {
                case ClError.TooPassword:
                    return "Too short password.";
                    break;
                case ClError.TooUserName:
                    return "Too long username.";
                    break;
                case ClError.Exists:
                    return "User with this nickname already exists.";
                    break;
                case ClError.NoExists:
                    return "User with this nickname doesn't exist.";
                    break;
                case ClError.WrongPassword:
                    return "Wrong password.";
                    break;
                default:
                    return "";
                    break;
            }
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
    }
}
