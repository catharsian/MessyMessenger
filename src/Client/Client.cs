using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.IO;
using System.Security.Cryptography.X509Certificates;



namespace Messenger_Client
{


    public enum ClError : byte
    {
        TooUserName = MesClient.Mes_TooUsername,
        TooPassword = MesClient.Mes_TooPassword,
        Exists = MesClient.Mes_Exists,
        NoExists = MesClient.Mes_NoExists,
        WrongPassword = MesClient.Mes_WrongPass,
    }
    public class MesClient
    {
        public Thread tcpThread;
        private bool _conn = false;
        private bool _logged = false;
        private string _user;
        private string _pass;
        private bool regged;

        public TcpClient client;
        public NetworkStream netStream;  // Raw-data stream of connection.
        public SslStream ssl;            // Encrypts connection using SSL.
        public BinaryReader br;          // Read simple data
        public BinaryWriter bw;          // Write simple data
        
        public const int  Mes_HELLO = 2021;
        public const byte Mes_OK = 0;           // OK
        public const byte Mes_Login = 1;        // Login
        public const byte Mes_Register = 2;     // Register
        public const byte Mes_TooUsername = 3;  // Too long username
        public const byte Mes_TooPassword = 4;  // Too long password
        public const byte Mes_Exists = 5;       // Already exists
        public const byte Mes_NoExists = 6;     // Doesn't exists
        public const byte Mes_WrongPass = 7;    // Wrong password
        public const byte Mes_IsAvailable = 8;  // Is user available?
        public const byte Mes_Available = 9;    // User is available or not
        public const byte Mes_Send = 10;        // Send message
        public const byte Mes_Received = 11;    // Message received
        public const byte Mes_ChainEnd = 12;     // Users online chain end
        public const byte Mes_ChainContinue = 13;// Users online chain continues
        public const byte Mes_GetUsersOnline = 20;


        public delegate void MesErrorEventHandler(object sender, MesErrorEventArgs args);
        public delegate void MesAvailEventHandler(object sender, MesAvailEventArgs args);
        public delegate void MesReceivedEventHandler(object sender, MesReceivedEventArgs args);

        public event EventHandler LoginOK;
        public event EventHandler RegisterOK;
        public event MesErrorEventHandler LoginFailed;
        public event MesErrorEventHandler RegisterFailed;
        public event EventHandler Disconnected;
        public event MesAvailEventHandler UserAvailable;
        public event MesReceivedEventHandler MessageReceived;
        public event EventHandler GotUsersOnline;
        public event EventHandler NoSuchHost;
        private List<string> _users;

        public MesClient()
        {

        }

        public IPEndPoint ip;

        public  List<string> UsersOnline 
        {
            get
            {
                return _users;
            }
        }

        public bool isLogged 
        {
            get
            {
                return _logged;
            } 
        }
        public string userName 
        {
            get
            {
                return _user;
            }
        }
        public string password
        {
            get
            {
                return _pass;
            }
        }
        public string ChatWith { get; set; }
        void Connect(string name, string password, bool register)
        {

            _user = name;
            _pass = password;
            regged = register;
            tcpThread = new Thread(new ThreadStart(SetupConn));
            tcpThread.Start();
            
        }
        public void Login(string name, string password)
        {
            Connect(name, password, true);
        }
        public void Register(string name, string password)
        {
            Connect(name, password, false);
        }
        public void Disconnect()
        {
            if (_conn)
            {
                CloseConn();
            }
        }
        private bool InitStuff()
        {
            if (!_conn)
            {
                try
                {
                    _conn = true;
                    client = new TcpClient(ip.Address.ToString(), ip.Port);
                    netStream = client.GetStream();
                    ssl = new SslStream(netStream, false, new RemoteCertificateValidationCallback(ValidateCert));
                    ssl.AuthenticateAsClient("Messenger Server");
                    br = new BinaryReader(ssl, Encoding.UTF8);
                    bw = new BinaryWriter(ssl, Encoding.UTF8);
                    return true;
                }
                catch (Exception)
                {
                    _conn = false;
                    NoSuchHost?.Invoke(this, new EventArgs());
                    return false;
                }
            }
            return true;
        }
        public void SetupConn()  // Setup connection and login
        {
            
            if (!InitStuff())
            {
                return;
            }
            try
            {

                int hello = br.ReadInt32();
                if (hello != Mes_HELLO)
                {
                    throw new Exception("SHIT SHIT SHIT");
                }
                bw.Write(Mes_HELLO);
                bw.Flush();


                bw.Write(regged ? Mes_Login : Mes_Register);

                bw.Write(userName);
                bw.Write(password);
                bw.Flush();

                byte ans = br.ReadByte();
                if (ans == Mes_OK)
                {
                    if (!regged)
                    {
                        OnRegOK();
                    }
                    OnLoginOK();
                }
                else
                {
                    MesErrorEventArgs err = new MesErrorEventArgs((ClError)ans);
                    if (regged)
                    {
                        OnLoginFailed(err);
                        goto shit;
                    }
                    else
                    {
                        OnRegisterFailed(err);
                        goto shit;
                    }

                }

                Reciever();


            shit:
                { }
                CloseConn();
            }
            catch (IOException)
            {
                CloseConn();
                OnDisconnect();
            }
        }


        public void OnDisconnect()
        {
            Disconnected?.Invoke(this, new EventArgs());
        }

        public static bool ValidateCert(object sender, X509Certificate certificate,
              X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true; // Allow untrusted certificates.
        }
        public void CloseConn() // Close connection.
        {
            if (_conn)
            {
                br.Close();
                bw.Close();
                ssl.Close();
                netStream.Close();
                client.Close();
                _conn = false;
            }
        }
        void Reciever()
        {
            _logged = true;
            try
            {
                while (client.Connected)
                {
                    var type = br.ReadByte();
                    switch (type)
                    {
                        case Mes_IsAvailable:
                            var user = br.ReadString();
                            var isAvail = br.ReadBoolean();
                            OnUserAvail(new MesAvailEventArgs(user, isAvail));
                            break;
                        case Mes_Received:
                            var from = br.ReadString();
                            var msg  = br.ReadString();
                            OnMessageRecieved(new MesReceivedEventArgs(from, msg));
                            break;
                        case Mes_GetUsersOnline:
                            GetUsersOnline();
                            Thread.Sleep(1000);
                            OnGotUsersOnline();
                            break;
                        default:
                            throw new UnknownProtocolMessageException();
                            break;
                    }

                }

            }
            catch (IOException)
            {
                OnDisconnect();
            }
            _logged = false;
        }
        public void IsAvailible(string user)
        {
            bw.Write(Mes_IsAvailable);
            bw.Write(user);
            bw.Flush();
        }
        public void OnUserAvail(MesAvailEventArgs args)
        {

            var UserAvail = UserAvailable;

            if (UserAvail != null)
            {
                UserAvail(this, args);
            }
        }

        public void SendMessage(string to, string msg)
        {
            bw.Write(Mes_Send);
            bw.Write(to);
            bw.Write(msg);
            bw.Flush();
        }
        public void OnMessageRecieved(MesReceivedEventArgs args)
        {
            var MesReceived = MessageReceived;
            if (MessageReceived!=null)
            {
                MesReceived(this, args);
            }
        }
        public void OnLoginFailed(MesErrorEventArgs e)
        {
            var LogFail = LoginFailed;
            if (LogFail != null)
            {
                LogFail(this, e);
            }
        }
        public void OnRegisterFailed(MesErrorEventArgs e)
        {
            var regFail = RegisterFailed;
            if (regFail != null)
            {
                regFail(this, e);
            }
        }
        public void OnRegOK()
        {
            var regok = RegisterOK;
            if (regok != null)
            {
                regok(this, new EventArgs());
            }
        }
        public void OnLoginOK()
        {
            var logok = LoginOK;
            if (logok != null)
            {
                logok(this, new EventArgs());
            }
        }
        private void GetUsersOnline()
        {
            _users = new List<string>();
            bw.Write(Mes_GetUsersOnline);
            bw.Flush();
            byte type = br.ReadByte();
            while (type != Mes_ChainEnd)
            {
                var user = br.ReadString();
                _users.Add(user);
                type = br.ReadByte();
            }
        }
        public void OnGotUsersOnline()
        {
            var gotusersonl = GotUsersOnline;
            if (gotusersonl != null)
            {
                gotusersonl(this, new EventArgs());
            }
        }
    }
}