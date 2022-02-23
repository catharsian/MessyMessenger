using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;
using System.Net.Security;
using System.IO;
using System.Security.Cryptography.X509Certificates;


namespace Download_Manager_Server
{
    class ServersClient
    {
        public TcpClient client { get; set; }
        private Server serv;
        public SslStream ssl { get; private set; }
        public NetworkStream networkStream { get; private set; }
        public BinaryWriter bw { get; private set; }
        public BinaryReader br { get; private set; }
        private UserInfo thisUser;
        public UserInfo User
        {
            get
            {
                return thisUser;
            }
        }

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

        // You need to subscribe to these events in your window class
        // So that you can change the UI when event happens.
        public static event EventHandler RefreshUsersList;

        public ServersClient(Server parent, TcpClient me)
        {
            client = me;
            serv = parent;

            try
            {
                new Thread(new ThreadStart(SetupConnection)).Start();
            }
            catch
            {

            }
        }
        private void SetupConnection()
        {
            try
            {
                networkStream = client.GetStream();
                ssl = new SslStream(networkStream, false);
                ssl.AuthenticateAsServer(serv.cert, false, System.Security.Authentication.SslProtocols.Tls, true);
                bw = new BinaryWriter(ssl, Encoding.UTF8);
                br = new BinaryReader(ssl, Encoding.UTF8);


                bw.Write(PR_CONNECTION_START);
                bw.Flush();
                var mess = br.ReadByte();
                if (mess != PR_CONNECTION_START)
                {
                    throw new UnknownProtocolMessageException();
                }

                var registration = br.ReadBoolean();
                var username = br.ReadString();
                var password = br.ReadString();
                try
                {
                    if (username.Length > 15 || username.Length < 7)
                    {
                        bw.Write(PR_BAD_NICKNAME);
                        bw.Flush();
                        throw new PlannedDisconnectionException();
                    }
                    else if (password.Length < 8 || password.Length > 20)
                    {
                        bw.Write(PR_BAD_PASSWORD);
                        bw.Flush();
                        throw new PlannedDisconnectionException();
                    }
                    else
                    {
                        if (registration)
                        {
                            if (serv.users.TryGetValue(username, out thisUser))
                            {
                                bw.Write(PR_EXISTS);
                                bw.Flush();
                                throw new PlannedDisconnectionException();
                            }
                            else
                            {
                                thisUser = new UserInfo(username, password, this);

                                serv.users.Add(username, thisUser);
                                bw.Write(PR_OK);
                                bw.Flush();
                                serv.UpdateUsersList();
                            }
                        }
                        else
                        {
                            if (serv.users.TryGetValue(username, out thisUser))
                            {
                                if (thisUser.password == password)
                                {
                                    bw.Write(PR_OK);
                                    bw.Flush();
                                    thisUser.connection = this;
                                }
                                else
                                {
                                    bw.Write(PR_WRONG_PASSWORD);
                                    bw.Flush();
                                    throw new PlannedDisconnectionException();
                                }
                            }
                            else
                            {
                                bw.Write(PR_DOES_NOT_EXIST);
                                bw.Flush();
                                throw new PlannedDisconnectionException();
                            }
                        }
                    }
                }
                catch (PlannedDisconnectionException)
                {
                    CloseConn();
                    return;
                }

                Reciever();

                CloseConn();
            }
            catch
            {

            }
        }


        private void CloseConn()
        {
            bw.Close();
            br.Close();
            ssl.Close();
            networkStream.Close();
            client.Close();
        }

        private void Reciever()
        {
            thisUser.loggedIn = true;
            try
            {
                RefreshUsersList?.Invoke(this, new EventArgs());
                SendEveryoneRequestToGetUsersOnline();
                while (client.Connected)
                {
                    var type = br.ReadByte();
                    switch (type)
                    {
                        case PR_START_USERS_CHAIN:
                            SendUsersOnline();
                            break;
                        case PR_SEND_FILE:
                            var to = br.ReadString();
                            var filename = br.ReadString();
                            AskForSend(to, filename);
                            break;
                        case PR_RECEIVE_ACCEPT:
                            if (thisUser.pending_request != null)
                            {
                                thisUser.pending_request.connection.bw.Write(PR_OK);
                                thisUser.pending_request.connection.bw.Flush();
                            }
                            break;
                        case PR_RECEIVE_REJECT:
                            thisUser.pending_request.connection.bw.Write(PR_NO);
                            thisUser.pending_request.connection.bw.Flush();
                            break;
                        case PR_SENDING_STARTED:
                            ContinueSending();
                            break;
                        default:
                            throw new UnknownProtocolMessageException();
                            break;
                    }
                }
            }
            catch
            {

            }
            thisUser.loggedIn = false;
            RefreshUsersList.Invoke(this, new EventArgs());
            SendEveryoneRequestToGetUsersOnline();
        }
        private IEnumerable<string> getUsernamesOnline()
        {
            foreach (var user in serv.users.Values)
            {
                if (user.loggedIn)
                {
                    yield return user.username;
                }
            }
        }
        private IEnumerable<UserInfo> getUserInfosOnline()
        {
            foreach (var user in serv.users.Values)
            {
                if (user.loggedIn)
                {
                    yield return user;
                }
            }
        }

        UserInfo reciever;
        private void AskForSend(string to, string filename)
        {
            if (serv.users.TryGetValue(to, out reciever))
            {
                if (reciever.loggedIn)
                {
                    reciever.connection.bw.Write(PR_FILE_RECEIVE);
                    reciever.connection.bw.Write(thisUser.username);
                    reciever.connection.bw.Write(filename);
                    reciever.connection.bw.Flush();
                    reciever.pending_request = thisUser;
                }
                else
                {
                    bw.Write(PR_NO);
                    bw.Flush();
                }
            }
            else
            {
                bw.Write(PR_NO);
                bw.Flush();
            }
        }
        public void ContinueSending()
        {
            reciever.connection.bw.Write(PR_SENDING_STARTED);
            reciever.connection.bw.Flush();
            byte[] buffer = new byte[1024];
            
            int bytesRemaining = br.ReadInt32();
            reciever.connection.bw.Write(bytesRemaining);
            reciever.connection.bw.Flush();
            while (bytesRemaining > 0)
            {
                buffer = br.ReadBytes(bytesRemaining);
                reciever.connection.bw.Write(buffer);
                reciever.connection.bw.Flush();
                bytesRemaining = br.ReadInt32();
                reciever.connection.bw.Write(bytesRemaining);
                reciever.connection.bw.Flush();
            }
        }
        private void SendUsersOnline()
        {
            foreach(var User in getUsernamesOnline())
            {
                bw.Write(PR_CONTINUE_USERS_CHAIN);
                bw.Write(User);
                bw.Flush();
            }
            bw.Write(PR_END_USERS_CHAIN);
            bw.Flush();
        }
        public void SendEveryoneRequestToGetUsersOnline()
        {
            foreach (var user in getUserInfosOnline())
            {
                user.connection.bw.Write(PR_START_USERS_CHAIN);
                user.connection.bw.Flush();
            }
        }
    }
}
