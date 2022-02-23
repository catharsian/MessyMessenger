using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Net.Security;
using System.IO;
using System.Threading;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Threading;


namespace Download_Manager_Client
{
    public enum AuthorizationError : byte
    {
        Username_Exists = Client.PR_EXISTS,
        Username_does_not_Exist = Client.PR_DOES_NOT_EXIST,
        Bad_Password = Client.PR_BAD_PASSWORD,
        Bad_Nickname = Client.PR_BAD_NICKNAME,
        Wrong_Password = Client.PR_WRONG_PASSWORD,
    }
    public class Client
    {
        private Thread tcpThread;
        private bool connected = false;
        private bool loggedIn = false;
        private bool registered;
        private string username;
        private string password;

        public TcpClient tcpClient { get; private set; }
        public SslStream ssl { get; private set; }
        public NetworkStream networkStream { get; private set; }
        public BinaryReader br { get; private set; }
        public BinaryWriter bw { get; private set; }

        public Dispatcher dispatcher { get; set; }

        private string wantedToSend;
        private string folder;


        public const byte PR_CONNECTION_START = 0;
        public const byte PR_OK = 1;
        public const byte PR_NO = 2;
        public const byte PR_EXISTS = 3;
        public const byte PR_DOES_NOT_EXIST = 4;
        public const byte PR_BAD_NICKNAME = 5;
        public const byte PR_BAD_PASSWORD = 6;
        public const byte PR_SEND_FILE = 7;
        public const byte PR_FILE_RECEIVE = 8;
        public const byte PR_WRONG_PASSWORD = 12;
        public const byte PR_SENDING_STARTED = 13;
        public const byte PR_USER_CONNECTED = 19;
        public const byte PR_START_USERS_CHAIN = 20;
        public const byte PR_CONTINUE_USERS_CHAIN = 21;
        public const byte PR_END_USERS_CHAIN = 22;
        public const byte PR_RECEIVE_ACCEPT = 23;
        public const byte PR_RECEIVE_REJECT = 24;

        public delegate void AuthorizationErrorHandler(object sender, AuthorizationErrorEventArgs e);
        public delegate void ReceiveFileEventHandler(object sender, ReceiveFileEventArgs e);
        public delegate (string, string) SelectFolderEventHandler(object sender, EventArgs e);

        public event AuthorizationErrorHandler AuthorizationFailed;
        public event EventHandler AuthorizationSucceeded;
        public event EventHandler Disconnected;
        public event EventHandler StrangeProtocolAccepted;
        public event EventHandler RefreshUsersList;
        public event EventHandler OnTransmissionAccepted;
        public event EventHandler OnTransmissionDeclined;
        public event ReceiveFileEventHandler FileReceiveSuggestion;
        public List<string> UsersOnline { get; private set; } = new List<string>();
        public string ip { get; set; }
        public Client()
        {

        }
        private void Connect(string username, string password, bool registered)
        {
            this.username = username;
            this.password = password;
            this.registered = registered;

            tcpThread = new Thread(new ThreadStart(MainIdea));
            tcpThread.Start();
        }
        public void Login(string username, string password)
        {
            Connect(username, password, true);
        }
        public void Register(string username, string password)
        {
            Connect(username, password, false);
        }
        private void InitConnection()
        {
            if (!connected)
            { 
                connected = true;
                tcpClient = new TcpClient("localhost",25665);
                networkStream = tcpClient.GetStream();
                ssl = new SslStream(networkStream, false, new RemoteCertificateValidationCallback(ValidateCert));
                ssl.AuthenticateAsClient("Download Manager Server");
                br = new BinaryReader(ssl, Encoding.UTF8);
                bw = new BinaryWriter(ssl, Encoding.UTF8);
            }
        }
        private bool ValidateCert(object sender, X509Certificate certificate,
            X509Chain chain, SslPolicyErrors sslPolicyErrors) // not a good idea but since I don't have a verified certificate it is the best thing I can do.
        {
            return true;
        }
        private void MainIdea()
        {
            dispatcher = Dispatcher.CurrentDispatcher;
            InitConnection();
            
            try
            {
                var type = br.ReadByte();
                if (type != PR_CONNECTION_START)
                {
                    throw new UnknownProtocolMessageException();
                }
                bw.Write(PR_CONNECTION_START);
                bw.Flush();
                bw.Write(!registered);
                bw.Write(username);
                bw.Write(password);
                bw.Flush();
                type = br.ReadByte();
                if (type != PR_OK)
                {
                    AuthorizationFailed?.Invoke(this, new AuthorizationErrorEventArgs((AuthorizationError)type));
                    throw new AuthorizationErrorException();
                }
                AuthorizationSucceeded?.Invoke(this, new EventArgs());
                Thread.Sleep(3000);
                Reciever();
            }
            catch (IOException)
            {
                Disconnected?.Invoke(this, new EventArgs());
            }
            catch (UnknownProtocolMessageException)
            {
                StrangeProtocolAccepted?.Invoke(this, new EventArgs());
            }
            catch (AuthorizationErrorException)
            {

            }
            EndConnection();
        }
        private void EndConnection()
        {
            connected = false;
            bw.Close();
            br.Close();
            ssl.Close();
            networkStream.Close();
            tcpClient.Close();
        }
        private void Reciever()
        {
            loggedIn = true;
            try
            {
                while (tcpClient.Connected)
                {
                    var type = br.ReadByte();
                    switch (type)
                    {
                        case PR_START_USERS_CHAIN:
                            GetUsersOnlineList();
                            RefreshUsersList?.Invoke(this, new EventArgs());
                            break;
                        case PR_SEND_FILE:

                            break;
                        case PR_FILE_RECEIVE:
                            var from = br.ReadString();
                            var filename = br.ReadString();
                            FileReceiveSuggestion?.Invoke(this, new ReceiveFileEventArgs(from, filename));
                            break;
                        case PR_OK:
                            bw.Write(PR_SENDING_STARTED);
                            bw.Flush();
                            ContinueSending();
                            break;
                        case PR_NO:
                            OnTransmissionDeclined?.Invoke(this, new EventArgs());
                            break;
                        case PR_SENDING_STARTED:
                            ReceiveFile(folder);
                            break;
                        default:

                            break;
                    }
                }

            }
            catch (IOException)
            {
                Disconnected.Invoke(this, new EventArgs());
                EndConnection();
            }
            loggedIn = false;
        }
        private void GetUsersOnlineList()
        {
            UsersOnline = new List<string>();
            bw.Write(PR_START_USERS_CHAIN);
            var type = br.ReadByte();
            while (type != PR_END_USERS_CHAIN)
            {
                var newUser = br.ReadString();
                UsersOnline.Add(newUser);
                type = br.ReadByte();
            }
        }
        public void SendFile(string to, string pathToFile, string filename)
        {
            bw.Write(PR_SEND_FILE);
            bw.Write(to);
            bw.Write(filename);
            wantedToSend = pathToFile;
            bw.Flush();
        }
        private void ContinueSending()
        {
            try
            {
                OnTransmissionAccepted?.Invoke(this, new EventArgs());
                byte[] buffer = new byte[1024];
                int bytesRemaining;
                var file = File.OpenRead(wantedToSend);
                bytesRemaining = file.Read(buffer, 0, 1024);
                if (bytesRemaining > 0)
                {
                    bw.Write(bytesRemaining);
                    bw.Write(buffer);
                    bw.Flush();
                    buffer = new byte[1024];
                }

                while (bytesRemaining == 1024)
                {
                    bytesRemaining = file.Read(buffer, 0, 1024);
                    if (bytesRemaining == 0)
                    {
                        break;
                    }
                    bw.Write(bytesRemaining);
                    bw.Write(buffer);
                    bw.Flush();
                    buffer = new byte[1024];
                }
                bw.Write(0);
                bw.Flush();
                
                file.Close();
            }
            catch (FileNotFoundException)
            {
                bw.Write(0);
                bw.Flush();
            }
        }
        private void ReceiveFile(string folder)
        {
            int bytesRemaining = br.ReadInt32();
            
            if (bytesRemaining > 0)
            {
                try
                {
                    FileStream file = File.Create(folder);
                    byte[] buffer = new byte[1024];
                    while (bytesRemaining > 0)
                    {
                        buffer = br.ReadBytes(bytesRemaining);
                        file.Write(buffer, 0, bytesRemaining);;
                        bytesRemaining = br.ReadInt32();
                    }
                    file.Close();
                }
                catch (ObjectDisposedException)
                {

                }
            }
        }
        public void FileReceiveAnswer(bool ans)
        {
            if (ans)
            {
                bw.Write(PR_RECEIVE_ACCEPT);
            }
            else
            {
                bw.Write(PR_RECEIVE_REJECT);
            }
            bw.Flush();
        }
        public void SetFolder(string folder)
        {
            this.folder = folder;
        }
    }

}
