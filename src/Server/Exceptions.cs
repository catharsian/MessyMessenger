using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Download_Manager_Server
{
    class UnknownProtocolMessageException : Exception
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
    class PlannedDisconnectionException : Exception
    {
        public PlannedDisconnectionException()
        {

        }
        public PlannedDisconnectionException(string message) : base(message)
        {

        }
        public PlannedDisconnectionException(string message, Exception ex) : base(message, ex)
        {

        }
    }
}
