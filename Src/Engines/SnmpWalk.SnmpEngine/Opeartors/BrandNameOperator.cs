using System;
using System.Linq;
using System.Net;
using log4net;
using SnmpWalk.Common.DataModel.Snmp;
using SnmpWalk.Engines.SnmpEngine.ConfigurationLoader;

namespace SnmpWalk.Engines.SnmpEngine.Opeartors
{
    internal class BrandNameOperator
    {
        private ILog _log;
        private ISnmpService _snmpService;

        public string GetProperty(IPAddress ipAddress)
        {
            string result = string.Empty;

            try
            {
                _log.Info("BrandNameOperator.GetProperty(): Started");

                var mibres = _snmpService.GetNext(SnmpVersion.V1, SnmpHelper.DefaultOctetString, new Oid(SnmpHelper.HrDevice),ipAddress);

                var mib = mibres.First().OidValue;

                var brandNameHash = XmlCommonLoader.Instance.BrandNameTable;

                foreach (var key in brandNameHash.Keys)
                {
                    if (mib.Contains(key.ToString()))
                    {
                        result = brandNameHash[key].ToString();
                    }
                }


            }
            catch (Exception e)
            {
                _log.Error("BrandNameOperator.GetProperty(): Exception :", e);
            }
            finally
            {
                _log.Info("BrandNameOperator.GetProperty(): Finished");
            }

            return result;
        }

        public BrandNameOperator(ILog logger, ISnmpService snmpService)
        {
            _log = logger;
            _snmpService = snmpService;
        }
    }
}