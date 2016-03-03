using System;
using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using SnmpWalk.Common.DataModel.Enums;
using SnmpWalk.Common.DataModel.Snmp;

namespace SnmpWalk.Engines.SnmpEngine.Convertor
{
    public class SnmpEngineConverter
    {
        public VersionCode ToVersionCodeConverter(SnmpVersion version)
        {
            switch (version)
            {
                case SnmpVersion.V1:
                    return VersionCode.V1;
                case SnmpVersion.V2:
                    return VersionCode.V2;
                case SnmpVersion.V3:
                     return VersionCode.V3;
                default:
                    throw new ArgumentOutOfRangeException(nameof(version), version, null);
            }
        }

        public WalkMode ToWalkModeConverter(WalkingMode walkingMode)
        {
            switch (walkingMode)
            {
                case WalkingMode.Default:
                    return WalkMode.Default;
                case WalkingMode.WithinSubtree:
                    return WalkMode.WithinSubtree; 
                default:
                    throw new ArgumentOutOfRangeException(nameof(walkingMode), walkingMode, null);
            }
        }

        public SnmpDataType ToSnmpDataType(SnmpType snmpType)
        {
            switch (snmpType)
            {
                case SnmpType.EndMarker:
                    return SnmpDataType.EndMarker;
                case SnmpType.Integer32:
                    return SnmpDataType.Integer32;
                case SnmpType.OctetString:
                    return SnmpDataType.OctetString;
                case SnmpType.Null:
                    return SnmpDataType.Null;
                case SnmpType.ObjectIdentifier:
                    return SnmpDataType.ObjectIdentifier;
                case SnmpType.Sequence:
                    return SnmpDataType.Sequence;
                case SnmpType.IPAddress:
                    return SnmpDataType.IPAddress;
                case SnmpType.Counter32:
                    return SnmpDataType.Counter32;
                case SnmpType.Gauge32:
                    return SnmpDataType.Gauge32;
                case SnmpType.TimeTicks:
                    return SnmpDataType.TimeTicks;
                case SnmpType.Opaque:
                    return SnmpDataType.Opaque;
                case SnmpType.NetAddress:
                    return SnmpDataType.NetAddress;
                case SnmpType.Counter64:
                    return SnmpDataType.Counter64;
                case SnmpType.Unsigned32:
                    return SnmpDataType.Unsigned32;
                case SnmpType.NoSuchObject:
                    return SnmpDataType.NoSuchObject;
                case SnmpType.NoSuchInstance:
                    return SnmpDataType.NoSuchInstance;
                case SnmpType.EndOfMibView:
                    return SnmpDataType.EndOfMibView;
                case SnmpType.GetRequestPdu:
                    return SnmpDataType.GetRequestPdu;
                case SnmpType.GetNextRequestPdu:
                    return SnmpDataType.GetNextRequestPdu;
                case SnmpType.ResponsePdu:
                    return SnmpDataType.ResponsePdu;
                case SnmpType.SetRequestPdu:
                    return SnmpDataType.SetRequestPdu;
                case SnmpType.TrapV1Pdu:
                    return SnmpDataType.TrapV1Pdu;
                case SnmpType.GetBulkRequestPdu:
                    return SnmpDataType.GetBulkRequestPdu;
                case SnmpType.InformRequestPdu:
                    return SnmpDataType.InformRequestPdu;
                case SnmpType.TrapV2Pdu:
                    return SnmpDataType.TrapV2Pdu;
                case SnmpType.ReportPdu:
                    return SnmpDataType.ReportPdu;
                case SnmpType.Unknown:
                    return SnmpDataType.Unknown;
                default:
                    throw new ArgumentOutOfRangeException(nameof(snmpType), snmpType, null);
            }
        }
    }
}
