using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messenger_Client
{
    class UnknownProtocolMessageException : Exception
    {
        public UnknownProtocolMessageException()
        {

        }
        public UnknownProtocolMessageException(string message) : base(message)
        {

        }
        public UnknownProtocolMessageException(string message, Exception inner) : base(message, inner)
        {

        }
    }
}
