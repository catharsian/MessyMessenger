using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Net.Security;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Security.Authentication;
using System.Runtime.Serialization.Formatters.Binary;
using System.Data;



namespace Messenger_Server
{
    public class Server
    {
        public TcpListener server;
        public IPAddress ip;
        public int port = 25665;
        public bool running = true;
        public X509Certificate2 cert = new X509Certificate2("server.pfx", "instant");

        public Dictionary<string, UserInfo> users;

        public const int  IM_HELLO = 2021;
        public const byte IM_OK = 0;           // OK
        public const byte IM_Login = 1;        // Login
        public const byte IM_Register = 2;     // Register
        public const byte IM_TooUsername = 3;  // Too long username
        public const byte IM_TooPassword = 4;  // Too long password
        public const byte IM_Exists = 5;       // Already exists
        public const byte IM_NoExists = 6;     // Doesn't exists
        public const byte IM_WrongPass = 7;    // Wrong password
        public const byte IM_IsAvailable = 8;  // Is user available?
        public const byte IM_Available = 9;    // User is available or not
        public const byte IM_Send = 10;        // Send message
        public const byte IM_Received = 11;    // Message received

        public Server()
        {
            
            Console.WriteLine("---------Messenger Server initializization---------");
            users = new Dictionary<string, UserInfo>();
            LoadUsers();
            byte[] addr = { 127,0,0,1 };
            ip = new IPAddress(addr);
            server = new TcpListener(ip, port);
            server.Start();
        }
        public void TryToStop()
        {
            server.Stop();
            running = false;
        }

        public async Task<object> ListenAsync()
        {
            while (running)
            {
                TcpClient tcpClient = await server.AcceptTcpClientAsync();
                Console.WriteLine("Someone's connecting...");
                Client client = new Client(this, tcpClient);

            }
            return null;
        }
        
        string usersFileName = Environment.CurrentDirectory + "\\users.dat";
        public void SaveUsers()  // Save users data
        {
            try
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = new FileStream(usersFileName, FileMode.Create, FileAccess.Write);
                bf.Serialize(file, users.Values.ToArray());  // Serialize UserInfo array
                file.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        public void LoadUsers()  // Load users data
        {
            try
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = new FileStream(usersFileName, FileMode.Open, FileAccess.Read);
                UserInfo[] infos = (UserInfo[])bf.Deserialize(file);      // Deserialize UserInfo array
                file.Close();
                users = infos.ToDictionary((u) => u.UserName, (u) => u);  // Convert UserInfo array to Dictionary
            }
            catch { }
        }

    }
    public class Client
    {
        Server serv;
        public TcpClient client;
        public NetworkStream netStream;  // Raw-data stream of connection.
        public SslStream ssl;            // Encrypts connection using SSL.
        public BinaryReader br;          // Read simple data
        public BinaryWriter bw;          // Write simple data
        UserInfo info;

        public const int Mes_HELLO = 2021;
        public const byte Mes_OK = 0;            // OK
        public const byte Mes_Login = 1;         // Login
        public const byte Mes_Register = 2;      // Register
        public const byte Mes_TooUsername = 3;   // Too long username
        public const byte Mes_TooPassword = 4;   // Too long password
        public const byte Mes_Exists = 5;        // Already exists
        public const byte Mes_NoExists = 6;      // Doesn't exists
        public const byte Mes_WrongPass = 7;     // Wrong password
        public const byte Mes_IsAvailable = 8;   // Is user available?
        public const byte Mes_Available = 9;     // User is available or not
        public const byte Mes_Send = 10;         // Send message
        public const byte Mes_Received = 11;     // Message received
        public const byte Mes_ChainEnd = 12;     // Users online chain end
        public const byte Mes_ChainContinue = 13;// Users online chain continues
        public const byte Mes_GetUsersOnline = 20;


        private UserInfo userInfo = null;

        public Client(Server s, TcpClient t)
        {
            client = t;

            serv = s;
            try
            {
                (new Thread(new ThreadStart(SetupConn))).Start();
            }
            catch {

            }
        }
        void SetupConn()  // Setup connection
        {
            try
            {
                netStream = client.GetStream();
                ssl = new SslStream(netStream, false);
                ssl.AuthenticateAsServer(serv.cert, false, SslProtocols.Tls, true);
                br = new BinaryReader(ssl, Encoding.UTF8);
                bw = new BinaryWriter(ssl, Encoding.UTF8);

                bw.Write(Mes_HELLO);
                bw.Flush();
                int hello = br.ReadInt32();
                if (hello == Mes_HELLO)
                {

                }
                var logMode = br.ReadByte();
                var userName = br.ReadString();
                var password = br.ReadString();

                if (userName.Length < 10)
                {
                    if (password.Length > 20)
                    {
                        bw.Write(Mes_TooPassword);
                        bw.Flush();
                        goto shit;
                    }
                    else
                    {
                    }
                }
                else
                {
                    bw.Write(Mes_TooUsername);
                    bw.Flush();
                    goto shit;
                }
                

                if (logMode == Mes_Register)
                {
                    if (serv.users.ContainsKey(userName))
                    {
                        bw.Write(Mes_Exists);
                        bw.Flush();
                        goto shit;
                    }
                    else
                    {
                        userInfo = new UserInfo(userName, password, this);
                        serv.users.Add(userName, userInfo);
                        bw.Write(Mes_OK);
                        bw.Flush();
                        serv.SaveUsers();
                        Console.WriteLine($"{userName} successfully registered!");
                    }
                }
                else if (logMode == Mes_Login)
                {
                    if (serv.users.TryGetValue(userName, out userInfo))
                    {
                        if (password != userInfo.Password)
                        {
                            bw.Write(Mes_WrongPass);
                            bw.Flush();
                            goto shit;
                        }
                        else
                        {
                            if (userInfo.LoggedIn)
                            {
                                userInfo.Connection.CloseConn();
                            }
                            userInfo.Connection = this;
                            bw.Write(Mes_OK);
                            bw.Flush();
                            Console.WriteLine($"{userInfo.UserName} successfully logged in!");
                        }
                    }
                    else
                    {
                        bw.Write(Mes_NoExists);
                        bw.Flush();
                        goto shit;
                    }
                }

                Reciever();
                shit:
                CloseConn();
            }
            catch
            {
                Console.WriteLine("Something went wrong during this session");
                CloseConn();
            }
        }
        public void CloseConn() // Close connection
        {
            br.Close();
            bw.Close();
            ssl.Close();
            netStream.Close();
            client.Close();
        }
        void Reciever()
        {
            userInfo.LoggedIn = true;
            foreach (string user in getUsersOnline())
            {
                serv.users[user].Connection.bw.Write(Mes_GetUsersOnline);
                serv.users[user].Connection.bw.Flush();
            }
            try
            {
                while (client.Connected)
                {
                    var type = br.ReadByte();
                    switch (type)
                    {
                        case Mes_IsAvailable:
                            CheckAvailability();
                            break;
                        case Mes_Send:
                            var to  = br.ReadString();
                            var msg = br.ReadString();
                            Console.WriteLine($"{userInfo.UserName} to {to}: {msg}");
                            if (to.ToLower() == "all")
                                SendAll(msg, userInfo.UserName);
                            else
                            {
                                UserInfo recipient;
                                if (serv.users.TryGetValue(to, out recipient))
                                {
                                    if (recipient.LoggedIn)
                                    {
                                        recipient.Connection.bw.Write(Mes_Received);
                                        recipient.Connection.bw.Write(userInfo.UserName);
                                        recipient.Connection.bw.Write(msg);
                                        recipient.Connection.bw.Flush();
                                    }
                                }
                            }
                            break;
                        case Mes_GetUsersOnline:
                            SendUsersOnline();
                            break;
                        default:
                            Console.WriteLine($"Unknown pattern --> {type}");
                            throw new UnknownProtocolMessageException();
                            break;
                    }
                }
            }
            catch (IOException)
            {
                Console.WriteLine($"{userInfo.UserName} disconnected.");
            }
            catch (UnknownProtocolMessageException)
            {
                Console.WriteLine("User tried to send unknown pattern.");
            }
            userInfo.LoggedIn = false;
        }
        private void CheckAvailability()
        {
            string who = br.ReadString();
            bw.Write(Mes_IsAvailable);
            bw.Write(who);

            if (serv.users.TryGetValue(who, out info))
            {
                if (info.LoggedIn)
                {
                    bw.Write(true);
                }
                else
                {
                    bw.Write(false);
                }
            }
            else
            {
                bw.Write(false);
            }
            bw.Flush();
        }
        private void SendUsersOnline()
        {
            foreach (string user in getUsersOnline())
            {
                bw.Write(Mes_ChainContinue);
                bw.Write(user);
                bw.Flush();
            }
            bw.Write(Mes_ChainEnd);
            bw.Flush();
        }
        private IEnumerable<string> getUsersOnline()
        {
            foreach (UserInfo info in serv.users.Values)
            {
                if (info.LoggedIn)
                {
                    yield return info.UserName;
                }
            }
        }
        private void SendAll(string msg, string from)
        {
            foreach (string user in getUsersOnline())
            {
                serv.users[user].Connection.bw.Write(Mes_Received);
                serv.users[user].Connection.bw.Write(from);
                serv.users[user].Connection.bw.Write(msg);
                serv.users[user].Connection.bw.Flush();
            }
        }
    }
    [Serializable]
    public class UserInfo
    {
        public string UserName;
        public string Password;
        [NonSerialized]public bool LoggedIn;
        [NonSerialized]public Client Connection;
        

        public UserInfo(string user, string password)
        {
            this.Password = password;
            this.UserName = user;
            this.LoggedIn = false;
        }
        public UserInfo(string user, string password, Client conn)
        {
            this.Password   = password;
            this.UserName   = user;
            this.LoggedIn   = true;
            this.Connection = conn;
        }
    }
}
