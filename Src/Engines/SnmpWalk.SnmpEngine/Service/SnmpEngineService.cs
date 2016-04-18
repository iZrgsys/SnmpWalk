using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using log4net;
using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using SnmpWalk.Common.DataModel.Enums;
using SnmpWalk.Common.DataModel.Snmp;
using SnmpWalk.Engines.SnmpEngine.ConfigurationLoader;
using SnmpWalk.Engines.SnmpEngine.Convertor;
using SnmpWalk.Engines.SnmpEngine.Exceptions;
using SnmpWalk.Engines.SnmpEngine.Types;
using TimeoutException = Lextm.SharpSnmpLib.Messaging.TimeoutException;

namespace SnmpWalk.Engines.SnmpEngine.Service
{
    public class SnmpEngineService : ISnmpEngine
    {
        private static readonly ILog Log = LogManager.GetLogger("snmpWalk.log");
        private readonly SnmpEngineConverter _converter = new SnmpEngineConverter();
        private static XmlCommonLoader _xmlCommonLoader = XmlCommonLoader.Instance;
        private int _timeOut;

        public int TimeOut
        {
            get { return _timeOut; }
            set { _timeOut = value; }
        }

        public static List<Oid> InitializeOids
        {
            get { return _xmlCommonLoader.Oids; }
        }

        private IEnumerable<SnmpResult> Walk(SnmpVersion version, IPAddress ipAddress, string octetString, Oid oid, WalkingMode walkMode)
        {
            var result = new List<SnmpResult>();

            foreach (var childOid in oid.ChildOids)
            {
                result.AddRange(!childOid.HasChildOids
                    ? WalkSingle(version, ipAddress, octetString, childOid, walkMode)
                    : Walk(version, ipAddress, octetString, childOid, walkMode));
            }

            return result;
        }

        private IEnumerable<SnmpResult> WalkSingle(SnmpVersion version, IPAddress ipAddress, string octetString, Oid oid, WalkingMode walkMode)
        {
            var list = new List<Variable>();

            if (oid.HasAdditionalCodes)
            {
                return WalkWithAdditionalCodes(version, ipAddress, octetString, oid, walkMode);
            }

            Messenger.Walk(_converter.ToVersionCodeConverter(version), new IPEndPoint(ipAddress, SnmpHelper.SnmpServerPort), new OctetString(octetString), new ObjectIdentifier(oid.Value), list, _timeOut, _converter.ToWalkModeConverter(walkMode));

            return list.Select(var => new SnmpResult(new Oid(var.Id.ToString()), var.Data, _converter.ToSnmpDataType(var.Data.TypeCode)));
        }

        private IEnumerable<SnmpResult> WalkWithAdditionalCodes(SnmpVersion version, IPAddress ipAddress, string octetString, Oid oid, WalkingMode walkMode)
        {
            var list = new List<Variable>();
            var results = new List<SnmpResult>();
            var codesTable = _xmlCommonLoader.AdditionalCodeTable;
            var codes = (Codes)codesTable[oid.Name];

            if (codes != null)
            {
                foreach (var code in codes.Code)
                {
                    Messenger.Walk(_converter.ToVersionCodeConverter(version), new IPEndPoint(ipAddress, SnmpHelper.SnmpServerPort), new OctetString(octetString), new ObjectIdentifier(string.Concat(oid.Value, ".", code.Decimal)), list, _timeOut, _converter.ToWalkModeConverter(walkMode));

                    if (list.Any())
                    {
                        results.AddRange(list.Select(var => new SnmpResult(new Oid(string.Concat(oid.Value, ".", code.Decimal), code.Name, string.Concat(oid.FullName, ".", code.Name)), var.Data, _converter.ToSnmpDataType(var.Data.TypeCode))));
                    }
                }
            }

            return results;
        }

        private IPAddress GetIpEndPointFromHostName(string hostName)
        {
            var addresses = Dns.GetHostAddresses(hostName);
            if (addresses.Length == 0)
            {
                throw new ArgumentException("Unable to retrieve address from specified host name.", nameof(hostName));
            }
            if (addresses.Length > 1)
            {
                throw new ArgumentException("There is more that one IP address to the specified host.", nameof(hostName));
            }
            return addresses[0];
        }

        public IEnumerable<Variable> GetBulkOperation(SnmpVersion version, IPAddress ipAddress, string octetString)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Variable> GetNextOperation(SnmpVersion version, IPAddress ipAddress, string octetString)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Variable> GetOperation(SnmpVersion version, IPAddress ipAddress, string octetString)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<SnmpResult> WalkOperation(SnmpVersion version, string hostName, string octetString, Oid oid, WalkingMode walkMode)
        {
            Log.Debug("SnmpEngine.WalkOperation(): Started");

            List<SnmpResult> result;

            try
            {
                if (_timeOut == 0)
                {
                    _timeOut = SnmpHelper.DefaultTimeOut;
                }

                if (string.IsNullOrEmpty(octetString))
                {
                    octetString = SnmpHelper.DefaultOctetString;
                }

                result = Walk(version, GetIpEndPointFromHostName(hostName), octetString, oid, walkMode).ToList();
            }
            catch (Exception e)
            {
                if (e is TimeoutException)
                {
                    Log.Error("SnmpEngine.WalkOperation():Timeout Exception caught:", e);
                    throw new SnmpTimeOutException(e.Message, _timeOut);
                }
                else if (e is ArgumentOutOfRangeException)
                {
                    Log.Error("SnmpEngine.WalkOperation():Argument Out Of Range Exception caught:", e);
                    throw new SnmpEngineConvertorException((ArgumentOutOfRangeException)e);
                }
                else
                {
                    Log.Error("SnmpEngine.WalkOperation():Exception caught:", e);
                    throw new SnmpEngineException(e.Message);
                }
            }
            finally
            {
                Log.Debug("SnmpEngine.WalkOperation(): Finished");
            }

            return result;
        }

        public IEnumerable<SnmpResult> WalkBulkOperation(SnmpVersion version, IPAddress ipAddress, string octetString, Oid oid, WalkingMode walkMode)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<SnmpResult> WalkOperation(SnmpVersion version, IPAddress ipAddress, string octetString, Oid oid, WalkingMode walkMode)
        {
            Log.Debug("SnmpEngine.WalkOperation(): Started");

            List<SnmpResult> result;

            try
            {
                if (_timeOut == 0)
                {
                    _timeOut = SnmpHelper.DefaultTimeOut;
                }

                if (string.IsNullOrEmpty(octetString))
                {
                    octetString = SnmpHelper.DefaultOctetString;
                }

                result = oid.HasChildOids ? Walk(version, ipAddress, octetString, oid, walkMode).ToList() : WalkSingle(version, ipAddress, octetString, oid, walkMode).ToList();

            }
            catch (Exception e)
            {
                if (e is TimeoutException)
                {
                    Log.Error("SnmpEngine.WalkOperation():Timeout Exception caught:", e);
                    throw new SnmpTimeOutException(e.Message, _timeOut);
                }
                else if (e is ArgumentOutOfRangeException)
                {
                    Log.Error("SnmpEngine.WalkOperation():Argument Out Of Range Exception caught:", e);
                    throw new SnmpEngineConvertorException((ArgumentOutOfRangeException)e);
                }
                else
                {
                    Log.Error("SnmpEngine.WalkOperation():Exception caught:", e);
                    throw new SnmpEngineException(e.Message);
                }
            }
            finally
            {
                Log.Debug("SnmpEngine.WalkOperation(): Finished");
            }

            return result;
        }
    }
}
