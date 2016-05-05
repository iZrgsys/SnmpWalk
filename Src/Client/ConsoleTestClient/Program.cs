using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SnmpWalk.Engines.DiscoveryEngine.Service;

namespace ConsoleTestClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var discveryService = DiscoveryService.Instance;
            var devices = discveryService.PerformDiscovery("192.168.50.60");
        }
    }
}
