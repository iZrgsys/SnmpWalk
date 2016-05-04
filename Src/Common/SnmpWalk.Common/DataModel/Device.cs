using System.Collections.Generic;
using System.Net;
using SnmpWalk.Common.DataModel.Snmp;

namespace SnmpWalk.Common.DataModel
{
    public class Device
    {
        private IPAddress _ipAddress;
        private string _macAddress;
        private List<Oid> _oids;

        public IPAddress IpAddress
        {
            get { return _ipAddress; }
            set { _ipAddress = value; }
        }

        public string MacAddress
        {
            get { return _macAddress; }
            set { _macAddress = value; }
        }

        public List<Oid> Oids
        {
            get { return _oids; }
            set { _oids = value; }
        } 
    }
}
