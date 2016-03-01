using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using log4net;
using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using SnmpWalk.Engines.SnmpEngine.Exceptions;
using SnmpWalk.Engines.SnmpEngine.Types;
using TimeoutException = Lextm.SharpSnmpLib.Messaging.TimeoutException;

namespace SnmpWalk.Engines.SnmpEngine
{
    public class SnmpEngineService : ISnmpEngine
    {
        private static ILog _log = LogManager.GetLogger("snmpWalk.log");
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

                Messenger.Walk(VersionCode.V1, new IPEndPoint(IPAddress.Parse(ipAddress.Value), SnmpHelper.SnmpServerPort), new OctetString(octetString), new ObjectIdentifier(oid.Value), list, _timeOut, WalkMode.WithinSubtree);

                result = list.Select(var => new SnmpResult(var)).ToList();
            }
            catch (Exception e)
            {
                if (e is TimeoutException)
                {
                    _log.Error("SnmpEngine.WalkOperation():Timeout Exception caught:", e);
                    throw new SnmpTimeOutException(e.Message, _timeOut);
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
