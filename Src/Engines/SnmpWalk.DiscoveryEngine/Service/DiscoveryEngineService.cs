using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using log4net;
using SnmpWalk.Engines.DiscoveryEngine.Exceptions;

namespace SnmpWalk.Engines.DiscoveryEngine.Service
{
    public class DiscoveryEngineService : IDiscoveryEngine
    {
        private static ILog _log = LogManager.GetLogger("snmpWalk.log");
        private readonly List<IPAddress> _ipAddresses;
        private readonly Ping _pingSender;
        private static readonly Lazy<DiscoveryEngineService> EngineInstance = new Lazy<DiscoveryEngineService>(() => new DiscoveryEngineService());


        public static IDiscoveryEngine Instance
        {
            get
            {
                return EngineInstance.Value;
            }
        }


        public List<IPAddress> PerformDiscovery(params string[] ipAddresses)
        {
            return _ipAddresses;
        }

        public List<IPAddress> PerformPinging(params string[] ipAddresses)
        {
            _log.Debug("DiscoveryEngineService: DiscoveryEngineService.PerformPinging() - Started");
            try
            {
                Parallel.ForEach(ipAddresses, address =>
                {
                    var ipAddr = IPAddress.Parse(address);
                    var reply = _pingSender.Send(address);

                    if (reply != null && reply.Status == IPStatus.Success)
                    {
                        _ipAddresses.Add(ipAddr);
                    }
                });
            }
            catch (Exception e)
            {
                _log.Error(string.Concat("DiscoveryEngineService: DiscoveryEngineService.PerformPinging(): exception caught - ", e));
                throw new DiscoveryEngineException(e.Message);
            }
            finally
            {
                _log.Debug("DiscoveryEngineService: DiscoveryEngineService.PerformPinging() - Finished");
            }

            return _ipAddresses;
        }

        private DiscoveryEngineService()
        {
            _ipAddresses = new List<IPAddress>();
            _pingSender = new Ping();
        }
    }
}
