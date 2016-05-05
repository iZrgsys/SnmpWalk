using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices;
using SnmpWalk.Common.DataModel;

namespace SnmpWalk.Engines.DiscoveryEngine
{
    public interface IDiscoveryService
    {
        List<Device> PerformDiscovery(params string[] ipAddresses);

        List<IPAddress> PerformPinging(params string[] ipAddresses);
    }

    internal static class Iphlpapi
    {
        [DllImport("iphlpapi.dll", ExactSpelling = true)]
        public static extern int SendARP(int DestIP, int SrcIP, byte[] pMacAddr, ref uint PhyAddrLen);
    }
}
