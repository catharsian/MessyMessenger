using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messenger_Server
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
    class StrangeProtocolMessageException : Exception
    {
        public StrangeProtocolMessageException()
        {

        }
        public StrangeProtocolMessageException(string message) : base(message)
        {

        }
        public StrangeProtocolMessageException(string message, Exception inner) : base(message, inner)
        {

        }
    }
}
