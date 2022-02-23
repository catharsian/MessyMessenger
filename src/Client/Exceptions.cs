using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Download_Manager_Client
{
    public class UnknownProtocolMessageException : Exception
    {
        public UnknownProtocolMessageException()
        {

        }
        public UnknownProtocolMessageException(string message) : base(message)
        {

        }
        public UnknownProtocolMessageException(string message, Exception ex) : base(message, ex)
        {

        }
    }
    public class AuthorizationErrorException : Exception
    {
        public AuthorizationErrorException()
        {

        }
        public AuthorizationErrorException(string message) : base(message)
        {

        }
        public AuthorizationErrorException(string message, Exception ex) : base(message, ex)
        {

        }
    }
}
