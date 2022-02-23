using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Text.Json;

namespace Download_Manager_Server
{
    class Server
    {
        public void UpdateUsersList()
        {
            var file = File.Open("./users.json", FileMode.OpenOrCreate);
            var writer = new Utf8JsonWriter(file);
            JsonSerializer.Serialize(writer, UserInfoFactory.ArrTransform(users.Values.ToArray()));
            file.Close();
        }
        private void InitializeUsersList()
        {
            if (File.Exists("./users.json"))
            {
                try
                {

                    var fileContents = File.ReadAllText("./users.json");
                    
                    var deserd = JsonSerializer.Deserialize<UserInfoPattern[]>(fileContents);
                    users = deserd.ToDictionary((t) => t.name, (t) => UserInfoFactory.SingleTransform(t));
                }
                catch
                {

                }
            }
            else
            {
                File.Create("./users.json");
            }
        }
        private TcpListener _listener;
        public TcpListener Listener 
        { 
            get
            {
                return _listener;
            }
            private set
            {
                _listener = value;
            }
        }
        public IPAddress ip { get; set; }
        public int port { get; set; }
        private bool running = true;
        public X509Certificate2 cert = new X509Certificate2("server.pfx", "instant");
        public Dictionary<string, UserInfo> users = new Dictionary<string, UserInfo>();

        public static event EventHandler ServerLaunched;

        public Server()
        {
            var thread = new Thread(new ThreadStart(() => { 
                getAddress();

                Listener = new TcpListener(ip, port);
           
                Listener.Start();
                ServerLaunched?.Invoke(this, new EventArgs());
                Listen();
            }));
            InitializeUsersList();
            thread.Start();
        }
        private void getAddress()
        {
            var pathToCfgFile = Environment.CurrentDirectory + "/cfg.txt";
            var CfgFileLines = File.ReadLines(pathToCfgFile);

            foreach(var line in CfgFileLines)
            {
                if(line.ToLower().Contains("ip: "))
                {
                    Regex ipReg = new Regex(@"\b(?:[0-9]{1,3}\.){3}[0-9]{1,3}\b");
                    ip = IPAddress.Parse(ipReg.Match(line).Value);
                    ipReg = new Regex(@":([0-9]{1,5})"); // port

                    port = Convert.ToInt32(ipReg.Match(line).Value.Remove(0,1));
                    return;
                }
            }
        }
        private void Listen()
        {
            while (running)
            {
                var newTcpClient = Listener.AcceptTcpClientAsync();

                var newClient = new ServersClient(this, newTcpClient.Result);
            }
        }
        public IEnumerable<string> getUsernamesOnline()
        {
            foreach (var user in users.Values)
            {
                if (user.loggedIn)
                {
                    yield return user.username;
                }
            }
        }
        public IEnumerable<UserInfo> getUserInfosOnline()
        {
            foreach (var user in users.Values)
            {
                if (user.loggedIn)
                {
                    yield return user;
                }
            }
        }
    }
    class UserInfo
    {
        
        private string _username;
        private string _password;
        public UserInfo pending_request { get; set; } = null;
        public string password
        {
            get
            {
                return _password;
            }
            private set
            {
                _password = value;
            }
        }
        public string username
        {
            get { return _username; }
            private set
            {
                _username = value;
            }
        }
        private bool _log = false;
        public bool loggedIn
        {
            get
            {
                return _log;
            }
            set
            {
                _log = value;
            }
        }
        private ServersClient _conn;
        public ServersClient connection
        {
            get
            {
                return _conn;
            }
            set
            {
                _conn = value;
            }
        }
        public UserInfo(string username, string password, ServersClient conn)
        {
            this.username = username;
            this.password = password;
            connection = conn;
            loggedIn = false;
        }
        public UserInfo(string username, string password)
        {
            this.username = username;
            this.password = password;
            connection = null;
            loggedIn = false;
        }
    }
}
