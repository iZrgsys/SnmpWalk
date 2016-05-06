using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using log4net;
using SnmpWalk.Common.DataModel;
using SnmpWalk.Engines.DiscoveryEngine.Exceptions;

namespace SnmpWalk.Engines.DiscoveryEngine.Service
{
    public class DiscoveryService : IDiscoveryService
    {
        private static ILog _log = LogManager.GetLogger("snmpWalk.log");
        private readonly List<IPAddress> _ipAddresses;
        private readonly Hashtable _devices;
        private readonly Ping _pingSender;
        private static readonly Lazy<DiscoveryService> EngineInstance = new Lazy<DiscoveryService>(() => new DiscoveryService());


        public static IDiscoveryService Instance
        {
            get
            {
                return EngineInstance.Value;
            }
        }


        public Hashtable PerformDiscovery(params string[] ipAddresses)
        {
            _log.Debug("DiscoveryService: DiscoveryService.PerformDiscovery() - Started");
            try
            {
                var checkedAdresses = PerformPinging(ipAddresses);

                if (checkedAdresses.Any())
                {
                    foreach (var adress in checkedAdresses)
                    {
                        _devices.Add(adress,new Device(adress, GetMacUsingARP(adress), GetMachineNameFromIp(adress.ToString())));
                    }
                }
            }
            catch (Exception e)
            {
                _log.Error(string.Concat("DiscoveryService: DiscoveryService.PerformDiscovery(): exception caught - ", e));
                throw new DiscoveryEngineException(e.Message);
            }
            finally
            {
                _log.Debug("DiscoveryService: DiscoveryService.PerformDiscovery() - Finished");
            }

            return _devices;
        }

        public List<IPAddress> PerformPinging(params string[] ipAddresses)
        {
            _log.Debug("DiscoveryService: DiscoveryService.PerformPinging() - Started");
            try
            {
                _ipAddresses.Clear();
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
                _log.Error(string.Concat("DiscoveryService: DiscoveryService.PerformPinging(): exception caught - ", e));
                throw new DiscoveryEngineException(e.Message);
            }
            finally
            {
                _log.Debug("DiscoveryService: DiscoveryService.PerformPinging() - Finished");
            }

            return _ipAddresses;
        }

        private string GetMachineNameFromIp(string ipAdress)
        {
            var hostEntry = Dns.GetHostEntry(ipAdress);

            var machineName = hostEntry.HostName;

            return machineName;
        }

        private string GetMacUsingARP(IPAddress ip)
        {
            byte[] macAddr = new byte[6];
            uint macAddrLen = (uint)macAddr.Length;

            if (Iphlpapi.SendARP((int) ip.Address, 0, macAddr, ref macAddrLen) != 0)
            {
                _log.Error("DiscoveryService: DiscoveryService.GetMacUsingARP(): ARP command failed!");
                throw new DiscoveryEngineException("ARP command failed");
            }
               

            string[] str = new string[(int)macAddrLen];
            for (int i = 0; i < macAddrLen; i++)
                str[i] = macAddr[i].ToString("x2");

            return string.Join(":", str);
        }

        private DiscoveryService()
        {
            _ipAddresses = new List<IPAddress>();
            _devices = new Hashtable();
            _pingSender = new Ping();
        }
    }
}
