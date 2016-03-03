namespace SnmpWalk.Common.DataModel.Snmp
{
    public class OID
    {
        private string _oid;
        private string _name;

        public string Value 
        {
            get { return _oid; }
            set { _oid = value; }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public OID()
        {
            _oid = string.Empty;
            _name = string.Empty;
        }

        public OID(string oid, string name)
        {
            _oid = oid;
            _name = name;
        }
    }
}
