using System;
using System.Collections.Generic;
using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using System.Net;
using SnmpWalk.Core.Types;

namespace SnmpWalk.Core
{
    public class CoreService : ICoreService
    {
        public IEnumerable<Variable> GetBulkOperation(string snmpVersion, IpAddress ipAddress, string octetString)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Variable> GetNextOperation(string snmpVersion, IpAddress ipAddress, string octetString)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Variable> GetOperation(string snmpVersion, IpAddress ipAddress, string octetString)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Variable> WalkBulkOperation(string snmpVersion, IpAddress ipAddress, string octetString, OID oid, WalkingMode walkMode)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Variable> WalkOperation(string snmpVersion, IpAddress ipAddress, string octetString, OID oid, WalkingMode walkMode)
        {
            //IList<Variable> list = new List<Variable>();

            //var res = Messenger.Walk(VersionCode.V2, new IPEndPoint(IPAddress.Parse("192.168.50.55"), 161 ), new OctetString("public"), new ObjectIdentifier("1.3.6.1"), list, 100000, WalkMode.WithinSubtree);

            throw new NotImplementedException();
        }
    }
}
