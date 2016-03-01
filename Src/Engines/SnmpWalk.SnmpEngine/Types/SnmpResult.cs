using Lextm.SharpSnmpLib;

namespace SnmpWalk.Engines.SnmpEngine.Types
{
    public class SnmpResult
    {
        private OID _oid;
        private object _data;

        public SnmpResult()
        {
        }

        public SnmpResult(OID oid, object data)
        {
            _oid = oid;
            _data = data;
        }

        public SnmpResult(Variable variable)
        {
            _oid = new OID
            {
                Value = variable.Id.ToString()
            };

            _data = variable.Data;
        }
    }
}
