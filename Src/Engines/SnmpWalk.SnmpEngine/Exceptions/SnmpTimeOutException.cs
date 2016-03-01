using System;

namespace SnmpWalk.Engines.SnmpEngine.Exceptions
{
    public class SnmpTimeOutException : Exception
    {
        public SnmpTimeOutException(string message) : base(message)
        {
        }
    }
}
