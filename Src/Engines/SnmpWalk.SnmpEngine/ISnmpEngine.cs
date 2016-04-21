using Lextm.SharpSnmpLib;
using System.Collections.Generic;
using System.Net;
using SnmpWalk.Common.DataModel.Enums;
using SnmpWalk.Common.DataModel.Snmp;
using SnmpWalk.Engines.SnmpEngine.Types;

namespace SnmpWalk.Engines.SnmpEngine
{
    public interface ISnmpEngine
    {
        int TimeOut { get; set; }

        IEnumerable<SnmpResult> GetOperation(SnmpVersion version, string hostName, string octetString, Oid oid);

        IEnumerable<SnmpResult> GetOperation(SnmpVersion version, IPAddress ipAddress, string octetString, Oid oid);

        IEnumerable<SnmpResult> GetNextOperation(SnmpVersion version, IPAddress ipAddress, string octetString, Oid oid);

        IEnumerable<SnmpResult> GetBulkOperation(SnmpVersion version, IPAddress ipAddress, string octetString, Oid oid);

        IEnumerable<SnmpResult> WalkOperation(SnmpVersion version, IPAddress ipAddress, string octetString, Oid oid, WalkingMode walkMode);

        IEnumerable<SnmpResult> WalkOperation(SnmpVersion version, string hostName, string octetString, Oid oid, WalkingMode walkMode);

        IEnumerable<SnmpResult> WalkBulkOperation(SnmpVersion version, IPAddress ipAddress, int maxBulkRepetiotions, string octetString, Oid oid, WalkingMode walkMode);
    }
}
