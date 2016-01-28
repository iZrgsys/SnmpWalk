using Lextm.SharpSnmpLib;
using SnmpWalk.Core.Types;
using System.Collections.Generic;

namespace SnmpWalk.Core
{
    interface ICoreService
    {
        IEnumerable<Variable> GetOperation(string snmpVersion, IPAddress ipAddress, string octetString);

        IEnumerable<Variable> GetNextOperation(string snmpVersion, IPAddress ipAddress, string octetString);

        IEnumerable<Variable> GetBulkOperation(string snmpVersion, IPAddress ipAddress, string octetString);

        IEnumerable<Variable> WalkOperation(string snmpVersion, IPAddress ipAddress, string octetString, OID oid, WalkingMode walkMode);

        IEnumerable<Variable> WalkBulkOperation(string snmpVersion, IPAddress ipAddress, string octetString, OID oid, WalkingMode walkMode);
    }
}
