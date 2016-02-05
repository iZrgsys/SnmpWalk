using Lextm.SharpSnmpLib;
using SnmpWalk.Core.Types;
using System.Collections.Generic;

namespace SnmpWalk.Core
{
    public interface ICoreService
    {
        IEnumerable<Variable> GetOperation(string snmpVersion, IpAddress ipAddress, string octetString);

        IEnumerable<Variable> GetNextOperation(string snmpVersion, IpAddress ipAddress, string octetString);

        IEnumerable<Variable> GetBulkOperation(string snmpVersion, IpAddress ipAddress, string octetString);

        IEnumerable<Variable> WalkOperation(string snmpVersion, IpAddress ipAddress, string octetString, OID oid, WalkingMode walkMode);

        IEnumerable<Variable> WalkBulkOperation(string snmpVersion, IpAddress ipAddress, string octetString, OID oid, WalkingMode walkMode);
    }
}
