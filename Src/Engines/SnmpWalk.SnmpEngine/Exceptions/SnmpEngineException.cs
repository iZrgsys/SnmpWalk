using System;

namespace SnmpWalk.Engines.SnmpEngine.Exceptions
{
    public class SnmpEngineException : Exception
    {
        public SnmpEngineException(string message) : base(message)
        {     
        }
    }
}
