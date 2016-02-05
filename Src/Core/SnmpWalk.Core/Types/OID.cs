namespace SnmpWalk.Core.Types
{
    public class OID
    {
        private string _oid;

        public string Value 
        {
            get { return _oid; }
            set { _oid = value; }
        }

        public OID()
        {
            _oid = string.Empty;
        }

        public OID(string oid)
        {
            _oid = oid;
        }
    }
}
