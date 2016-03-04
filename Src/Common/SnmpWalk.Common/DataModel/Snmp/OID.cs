using System.Collections.Generic;

namespace SnmpWalk.Common.DataModel.Snmp
{
    public class Oid
    {
        private string _oid;
        private string _name;
        private bool _isRoot;
        private bool _hasCodeDictionary;
        private List<Oid> _oids; 

        public string Value 
        {
            get { return _oid; }
            set { _oid = value; }
        }

        public bool IsRoot
        {
            get { return _isRoot;}
            set { _isRoot = value; }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public Oid()
        {
            _oid = string.Empty;
            _name = string.Empty;
        }

        public Oid(string oid, string name)
        {
            _oid = oid;
            _name = name;
        }
    }
}
