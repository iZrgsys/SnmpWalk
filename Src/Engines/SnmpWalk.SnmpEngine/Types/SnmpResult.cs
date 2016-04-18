using System;
using System.Globalization;
using SnmpWalk.Common.DataModel.Snmp;

namespace SnmpWalk.Engines.SnmpEngine.Types
{
    public class SnmpResult
    {
        private Oid _oid;
        private SnmpDataType _dataType;
        private object _data;

        public object Data
        {
            get { return _data; }
            set { _data = value; }
        }

        public string OidValue
        {
            get { return _oid.Value; }
        }

        public SnmpDataType DataType
        {
            get { return _dataType; }
            set { _dataType = value; }
        }

        public Oid Oid
        {
            get { return _oid; }
            set { _oid = value; }
        }


        public SnmpResult()
        {
        }

        public SnmpResult(Oid oid, object data, SnmpDataType dataType)
        {
            _oid = oid;
            _data = data;
            _dataType = dataType;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0} - {1} : {2}", _oid.Value, Enum.GetName(typeof(SnmpDataType), _dataType), _data);
        }
    }
}
