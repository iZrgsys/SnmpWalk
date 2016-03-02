using System;

namespace SnmpWalk.Engines.SnmpEngine.Exceptions
{
    class SnmpEngineConvertorException : Exception
    {
        private readonly string _paramName;
        private readonly object _actualvalue;

        public string ParamName
        {
            get { return _paramName; }
        }

        public object ActualValue
        {
            get { return _paramName; }
        }

        public SnmpEngineConvertorException(string mesage) : base(mesage)
        {
        }

        public SnmpEngineConvertorException(string paramName, object actualValue, string message) : this(message)
        {
            _paramName = paramName;
            _actualvalue = actualValue;
        }

        public SnmpEngineConvertorException(ArgumentOutOfRangeException argumentOutOfRangeException) : this(argumentOutOfRangeException.ParamName, argumentOutOfRangeException.ActualValue, argumentOutOfRangeException.Message)
        {
        }
    }
}
