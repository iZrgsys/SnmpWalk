using System.ComponentModel;

namespace SnmpWalk.Client.Assets.Enums
{
    public enum IpVersions
    {
        [Description("ip_v4")]
        Ipv4 = 0,

        [Description("ip_v6")]
        Ipv6 = 1
    }
}