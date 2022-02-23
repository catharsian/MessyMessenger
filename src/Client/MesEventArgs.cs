using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messenger_Client
{
    public class MesErrorEventArgs : EventArgs
    {
        private ClError err;
        public MesErrorEventArgs(ClError err)
        {
            this.err = err;
        }
        public ClError Error
        {
            get
            {
                return err;
            }
        }
    }
    public class MesAvailEventArgs : EventArgs
    {
        string user;
        bool avail;

        public MesAvailEventArgs(string user, bool avail)
        {
            this.user = user;
            this.avail = avail;
        }

        public string UserName
        {
            get { return user; }
        }
        public bool IsAvailable
        {
            get { return avail; }
        }
    }
    public class MesReceivedEventArgs : EventArgs
    {
        string user;
        string msg;

        public MesReceivedEventArgs(string user, string msg)
        {
            this.user = user;
            this.msg = msg;
        }

        public string From
        {
            get { return user; }
        }
        public string Message
        {
            get { return msg; }
        }
    }
}
