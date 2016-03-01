using System;
using System.Collections.Generic;
using System.Net;
using log4net;
using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using SnmpWalk.Engines.DiscoveryEngine;
using SnmpWalk.Engines.SnmpEngine.Exceptions;
using SnmpWalk.Engines.SnmpEngine.Types;
using TimeoutException = Lextm.SharpSnmpLib.Messaging.TimeoutException;

namespace SnmpWalk.Engines.SnmpEngine
{
    public class SnmpEngineService : ISnmpEngine
    {
        private static ILog _log = LogManager.GetLogger("snmpWalk.log");
        private IDiscoveryEngine _discoveryEngine = DiscoveryEngineService.Instance;
        private int _timeOut = 0;

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

        public IEnumerable<Variable> WalkBulkOperation(SnmpVersion version, IpAddress ipAddress, string octetString, OID oid, WalkingMode walkMode)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Variable> WalkOperation(SnmpVersion version, IpAddress ipAddress, string octetString, OID oid, WalkingMode walkMode)
        {
            _log.Debug("SnmpEngine.WalkOperation(): Started");
            var list = new List<Variable>();

            try
            {
                if (_timeOut == 0)
                {
                    _timeOut = SnmpHelper.DefaultTimeOut;                  
                }

                Messenger.Walk(VersionCode.V2, new IPEndPoint(IPAddress.Parse(ipAddress.Value), SnmpHelper.SnmpServerPort), new OctetString(SnmpHelper.DefaultOctetString), new ObjectIdentifier(oid.Value), list, _timeOut, WalkMode.WithinSubtree);
            }
            catch (Exception e)
            {
                if (e is TimeoutException)
                {
                    _log.Error("SnmpEngine.WalkOperation():Timeout Exception caught:", e);
                    throw new SnmpTimeOutException(e.Message);
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

            return list;
        }
    }
}
