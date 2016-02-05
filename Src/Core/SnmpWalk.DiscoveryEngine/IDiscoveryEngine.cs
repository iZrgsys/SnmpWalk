using System.Collections.Generic;
using System.Net;

namespace SnmpWalk.Core.DiscoveryEngine
{
    public interface IDiscoveryEngine
    {
        IEnumerable<IPAddress> PerformDiscovery(params string[] ipAddresses);
    }
}
