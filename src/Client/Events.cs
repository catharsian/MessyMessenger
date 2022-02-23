using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Download_Manager_Client
{
    public class AuthorizationErrorEventArgs : EventArgs
    {
        AuthorizationError error;
        public AuthorizationErrorEventArgs(AuthorizationError err)
        {
            error = err;
        }
        public string ReturnAsStr()
        {
            switch (error)
            {
                case AuthorizationError.Username_Exists:
                    return "Error. This username already exist.";
                    break;
                case AuthorizationError.Username_does_not_Exist:
                    return "Error. This user does not exist.";
                    break;
                case AuthorizationError.Bad_Nickname:
                    return "Error. Nickname should be from 7 to 15 characters long";
                    break;
                case AuthorizationError.Bad_Password:
                    return "Error. Password should be from 8 to 20 characters long";
                    break;
                case AuthorizationError.Wrong_Password:
                    return "Error. Wrong password.";
            }
            return "";
        }
    }
    public class ReceiveFileEventArgs : EventArgs
    {
        public string filename { get; }
        public string from { get; }
        public ReceiveFileEventArgs(string from, string filename)
        {
            this.filename = filename;
            this.from = from;
        }
    }
    public class FileSelectedEventArgs : EventArgs
    {
        public string filename { get; }
        public FileSelectedEventArgs(string filename)
        {
            this.filename = filename;
        }
    }
}
