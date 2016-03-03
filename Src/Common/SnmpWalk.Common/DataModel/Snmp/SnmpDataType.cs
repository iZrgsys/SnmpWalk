namespace SnmpWalk.Common.DataModel.Snmp
{
    public enum SnmpDataType
    {
        EndMarker = 0,
        Integer32 = 2,
        OctetString = 4,
        Null = 5,
        ObjectIdentifier = 6,
        Sequence = 48,
        IPAddress = 64,
        Counter32 = 65,
        Gauge32 = 66,
        TimeTicks = 67,
        Opaque = 68,
        NetAddress = 69,
        Counter64 = 70,
        Unsigned32 = 71,
        NoSuchObject = 128,
        NoSuchInstance = 129,
        EndOfMibView = 130,
        GetRequestPdu = 160,
        GetNextRequestPdu = 161,
        ResponsePdu = 162,
        SetRequestPdu = 163,
        TrapV1Pdu = 164,
        GetBulkRequestPdu = 165,
        InformRequestPdu = 166,
        TrapV2Pdu = 167,
        ReportPdu = 168,
        Unknown = 65535
    }
}