using System.ComponentModel;

namespace SnmpWalk.Client.Assets.Enums
{
    public enum SnmpOperationType
    {
        [Description("get_operation")]
        Get = 0,

        [Description("get_next_operation")]
        GetNext = 1,

        [Description("get_bulk_operation")]
        GetBulk = 2,

        [Description("set_operation")]
        Set = 3,

        [Description("walk_operation")]
        Walk = 4,

        [Description("walk_bulk_operation")]
        WalkBulk = 5
    }
}