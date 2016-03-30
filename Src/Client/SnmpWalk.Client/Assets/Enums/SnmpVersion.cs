using System.ComponentModel;

namespace SnmpWalk.Client.Assets.Enums
{
    public enum SnmpVersion
    {
        [Description("v_1")]
        V1=0,

        [Description("v_2")]
        V2=1,

        [Description("v_3")]
        V3=2
    }
}