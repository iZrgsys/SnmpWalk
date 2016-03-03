using Lextm.SharpSnmpLib;
using System.Collections.Generic;
using SnmpWalk.Common.DataModel.Connection;
using SnmpWalk.Common.DataModel.Enums;
using SnmpWalk.Common.DataModel.Snmp;
using SnmpWalk.Engines.SnmpEngine.Types;

namespace SnmpWalk.Engines.SnmpEngine
{
    public interface ISnmpEngine
    {
        IEnumerable<Variable> GetOperation(SnmpVersion version, IpAddress ipAddress, string octetString);

        IEnumerable<Variable> GetNextOperation(SnmpVersion version, IpAddress ipAddress, string octetString);

        IEnumerable<Variable> GetBulkOperation(SnmpVersion version, IpAddress ipAddress, string octetString);

        IEnumerable<SnmpResult> WalkOperation(SnmpVersion version, IpAddress ipAddress, string octetString, OID oid, WalkingMode walkMode);

        IEnumerable<SnmpResult> WalkBulkOperation(SnmpVersion version, IpAddress ipAddress, string octetString, OID oid, WalkingMode walkMode);
    }
}
