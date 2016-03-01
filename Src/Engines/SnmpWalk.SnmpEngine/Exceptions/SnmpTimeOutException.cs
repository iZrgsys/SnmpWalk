using System;

namespace SnmpWalk.Engines.SnmpEngine.Exceptions
{
    public class SnmpTimeOutException : Exception
    {
        private int _timeOut;

        public SnmpTimeOutException(string message) : base(message)
        {
        }
        
        public SnmpTimeOutException(string message, int timeOut) : base(message)
        {
            _timeOut = timeOut;
        }

        public int TimeOut
        {
            get { return _timeOut; }
        }
    }
}
