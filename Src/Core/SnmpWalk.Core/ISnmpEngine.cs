using Lextm.SharpSnmpLib;
using System.Collections.Generic;
using SnmpWalk.Engines.SnmpEngine.Types;

namespace SnmpWalk.Engines.SnmpEngine
{
    public interface ISnmpEngine
    {
        IEnumerable<Variable> GetOperation(string snmpVersion, IpAddress ipAddress, string octetString);

        IEnumerable<Variable> GetNextOperation(string snmpVersion, IpAddress ipAddress, string octetString);

        IEnumerable<Variable> GetBulkOperation(string snmpVersion, IpAddress ipAddress, string octetString);

        IEnumerable<Variable> WalkOperation(string snmpVersion, IpAddress ipAddress, string octetString, OID oid, WalkingMode walkMode);

        IEnumerable<Variable> WalkBulkOperation(string snmpVersion, IpAddress ipAddress, string octetString, OID oid, WalkingMode walkMode);
    }
}
