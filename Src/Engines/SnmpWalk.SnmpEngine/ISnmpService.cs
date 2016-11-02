using System.Collections.Generic;
using System.Net;
using SnmpWalk.Common.DataModel.Enums;
using SnmpWalk.Common.DataModel.Snmp;
using SnmpWalk.Engines.SnmpEngine.Types;

namespace SnmpWalk.Engines.SnmpEngine
{
    public interface ISnmpService
    {
        int TimeOut { get; set; }

        IEnumerable<SnmpResult> Get(SnmpVersion version, string octetString, Oid oid, IPAddress ipAddress = null, string hostname = null, string userLoginV3 = null, string passwordV3 = null);

        IEnumerable<SnmpResult> GetNext(SnmpVersion version, string octetString, Oid oid, IPAddress ipAddress = null, string hostname = null, string userLoginV3 = null, string passwordV3 = null);

        IEnumerable<SnmpResult> GetBulk(SnmpVersion version, int maxBulkRepetiotions, string octetString, Oid oid, IPAddress ipAddress = null, string hostname = null, string userLoginV3 = null, string passwordV3 = null);

        IEnumerable<SnmpResult> Walk(SnmpVersion version, string octetString, Oid oid, WalkingMode walkMode, IPAddress ipAddress = null, string hostname = null, string userLoginV3 = null, string passwordV3 = null);

        IEnumerable<SnmpResult> WalkBulk(SnmpVersion version, int maxBulkRepetiotions, string octetString, Oid oid, WalkingMode walkMode, IPAddress ipAddress = null, string hostname = null, string userLoginV3 = null, string passwordV3 = null);
    }
}
