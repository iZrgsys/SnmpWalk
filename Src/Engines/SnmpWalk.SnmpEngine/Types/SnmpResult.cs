using System;
using System.Globalization;
using Lextm.SharpSnmpLib;
using SnmpWalk.Engines.SnmpEngine.Convertor;
using SnmpWalk.Engines.SnmpEngine.Types.Enums;

namespace SnmpWalk.Engines.SnmpEngine.Types
{
    public class SnmpResult
    {
        private OID _oid;
        private SnmpDataType _dataType;
        private object _data;

        public object Data
        {
            get { return _data; }
            set { _data = value; }
        }

        public SnmpDataType DataType
        {
            get { return _dataType; }
            set { _dataType = value; }
        }

        public OID Oid
        {
            get { return _oid; }
            set { _oid = value; }
        }


        public SnmpResult()
        {
        }

        public SnmpResult(OID oid, object data, SnmpDataType dataType)
        {
            _oid = oid;
            _data = data;
            _dataType = dataType;
        }

        public SnmpResult(Variable variable)
        {
            _oid = new OID
            {
                Value = variable.Id.ToString()
            };

            _data = variable.Data;
            _dataType = SnmpEngineConverter.ToSnmpDataType(variable.Data.TypeCode);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "oid: {0} - {1}:{2}", _oid, Enum.GetName(typeof(SnmpDataType), _dataType), _data);
        }
    }
}
