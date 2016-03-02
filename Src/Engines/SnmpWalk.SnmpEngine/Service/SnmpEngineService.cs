using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using log4net;
using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using SnmpWalk.Engines.SnmpEngine.Convertor;
using SnmpWalk.Engines.SnmpEngine.Exceptions;
using SnmpWalk.Engines.SnmpEngine.Types;
using SnmpWalk.Engines.SnmpEngine.Types.Enums;
using TimeoutException = Lextm.SharpSnmpLib.Messaging.TimeoutException;

namespace SnmpWalk.Engines.SnmpEngine.Service
{
    public class SnmpEngineService : ISnmpEngine
    {
        private static ILog _log = LogManager.GetLogger("snmpWalk.log");
        private SnmpEngineConverter _converter = new SnmpEngineConverter();
        private int _timeOut;

        public int TimeOut
        {
            get { return _timeOut; }
            set { _timeOut = value; }
        }

        public IEnumerable<Variable> GetBulkOperation(SnmpVersion version, IpAddress ipAddress, string octetString)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Variable> GetNextOperation(SnmpVersion version, IpAddress ipAddress, string octetString)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Variable> GetOperation(SnmpVersion version, IpAddress ipAddress, string octetString)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<SnmpResult> WalkBulkOperation(SnmpVersion version, IpAddress ipAddress, string octetString, OID oid, WalkingMode walkMode)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<SnmpResult> WalkOperation(SnmpVersion version, IpAddress ipAddress, string octetString, OID oid, WalkingMode walkMode)
        {
            _log.Debug("SnmpEngine.WalkOperation(): Started");

            var list = new List<Variable>();
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

                Messenger.Walk(_converter.ToVersionCodeConverter(version), new IPEndPoint(IPAddress.Parse(ipAddress.Value), SnmpHelper.SnmpServerPort), new OctetString(octetString), new ObjectIdentifier(oid.Value), list, _timeOut, _converter.ToWalkModeConverter(walkMode));

                result = list.Select(var => new SnmpResult(var)).ToList();
            }
            catch (Exception e)
            {
                if (e is TimeoutException)
                {
                    _log.Error("SnmpEngine.WalkOperation():Timeout Exception caught:", e);
                    throw new SnmpTimeOutException(e.Message, _timeOut);
                }
                else if (e is ArgumentOutOfRangeException)
                {
                    _log.Error("SnmpEngine.WalkOperation():Argument Out Of Range Exception caught:", e);
                    throw new SnmpEngineConvertorException((ArgumentOutOfRangeException)e);
                }
                else
                {
                    _log.Error("SnmpEngine.WalkOperation():Exception caught:", e);
                    throw new SnmpEngineException(e.Message);
                }
            }
            finally
            {
                _log.Debug("SnmpEngine.WalkOperation(): Finished");
            }

            return result;
        }
    }
}
