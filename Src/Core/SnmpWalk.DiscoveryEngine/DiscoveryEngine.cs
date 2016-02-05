using System;
using log4net;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using SnmpWalk.Core.DiscoveryEngine.Exceptions;

namespace SnmpWalk.Core.DiscoveryEngine
{
    public class DiscoveryEngine : IDiscoveryEngine
    {
        private static ILog _log = LogManager.GetLogger("snmpWalk.log");
        private readonly List<IPAddress> _ipAddresses;
        private readonly Ping _pingSender;
        private static readonly Lazy<DiscoveryEngine> _instance = new Lazy<DiscoveryEngine>(() => new DiscoveryEngine());


        public static IDiscoveryEngine Instance
        {
            get
            {
                return _instance.Value;
            }
        }


        public IEnumerable<IPAddress> PerformDiscovery(params string[] ipAddresses)
        {
            _log.Debug("DiscoveryEngine: DiscoveryEngine.PerformDiscovery() - Started");
            try
            {
                Parallel.ForEach(ipAddresses, address =>
                {
                    var ipAddr = IPAddress.Parse(address);
                    var reply = _pingSender.Send(address);

                    if (reply.Status == IPStatus.Success)
                    {
                        _ipAddresses.Add(ipAddr);
                    }
                });
            }
            catch (Exception e)
            {
                _log.Error(string.Concat("DiscoveryEngine: DiscoveryEngine.PerformDiscovery() exception caught -", e.Message));
                throw new DiscoveryEngineException(e.Message);
            }
            finally
            {
                _log.Debug("DiscoveryEngine: DiscoveryEngine.PerformDiscovery() - Finished");
            }

            return _ipAddresses;
        }

        private DiscoveryEngine()
        {
            _ipAddresses = new List<IPAddress>();
            _pingSender = new Ping();
        }
    }
}
