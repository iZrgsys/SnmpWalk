using System.Collections.Generic;
using System.Net;

namespace SnmpWalk.Engines.DiscoveryEngine
{
    public interface IDiscoveryService
    {
        List<IPAddress> PerformDiscovery(params string[] ipAddresses);

        List<IPAddress> PerformPinging(params string[] ipAddresses);
    }
}
