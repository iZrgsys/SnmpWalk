using System;
using System.Collections.Generic;
using Lextm.SharpSnmpLib;
using SnmpWalk.Core.Types;

namespace SnmpWalk.Core
{
    public class CoreService : ICoreService
    {
        public IEnumerable<Variable> GetBulkOperation(string snmpVersion, IPAddress ipAddress, string octetString)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Variable> GetNextOperation(string snmpVersion, IPAddress ipAddress, string octetString)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Variable> GetOperation(string snmpVersion, IPAddress ipAddress, string octetString)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Variable> WalkBulkOperation(string snmpVersion, IPAddress ipAddress, string octetString, OID oid, WalkingMode walkMode)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Variable> WalkOperation(string snmpVersion, IPAddress ipAddress, string octetString, OID oid, WalkingMode walkMode)
        {
            throw new NotImplementedException();
        }
    }
}
