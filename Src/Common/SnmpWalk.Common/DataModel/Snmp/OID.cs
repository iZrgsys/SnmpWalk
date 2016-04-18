using System.Collections.Generic;
using System.Linq;

namespace SnmpWalk.Common.DataModel.Snmp
{
    public class Oid
    {
        private string _oid;
        private bool _hasAdditionalCodes;
        private readonly string _name;
        private readonly string _fullName;
        private List<Oid> _childOids;
        private string _description;

        public List<Oid> ChildOids
        {
            get { return _childOids; }
            set { _childOids = value; }
        }

        public string FullName
        {
            get { return _fullName; }
        }

        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        public bool HasChildOids
        {
            get { return _childOids != null; }
        }

        public string Value
        {
            get { return _oid; }
            set { _oid = value; }
        }

        public string Name
        {
            get { return _name; }
        }

        public bool HasAdditionalCodes
        {
            get { return _hasAdditionalCodes; }
            set { _hasAdditionalCodes = value; }
        }

        public Oid()
        {
            _oid = string.Empty;
            _name = string.Empty;
        }

        public Oid(string oid)
        {
            _oid = oid;
        }

        public Oid(string oid, string name, string fullName)
        {
            _oid = oid;
            _name = name;
            _fullName = fullName;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        protected bool Equals(Oid other)
        {
            return string.Equals(_oid, other._oid) 
                && string.Equals(_name, other._name) 
                && string.Equals(_fullName, other._fullName) 
                && Equals(_childOids, other._childOids) 
                && string.Equals(_description, other._description);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (_oid != null ? _oid.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (_name != null ? _name.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (_fullName != null ? _fullName.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
