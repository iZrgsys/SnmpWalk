using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices;

namespace SnmpWalk.Engines.DiscoveryEngine
{
    public interface IDiscoveryService
    {
        Hashtable PerformDiscovery(params string[] ipAddresses);

        List<IPAddress> PerformPinging(params string[] ipAddresses);
    }

    internal static class Iphlpapi
    {
        [DllImport("iphlpapi.dll", ExactSpelling = true)]
        public static extern int SendARP(int DestIP, int SrcIP, byte[] pMacAddr, ref uint PhyAddrLen);
    }
}
