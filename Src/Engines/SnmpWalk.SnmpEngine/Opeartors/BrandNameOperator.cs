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
        private const char Dot = '.';
        private const int brIndex = 0;

        private ILog _log;
        private ISnmpService _snmpService;

        public string GetProperty(IPAddress ipAddress)
        {
            string result = string.Empty;

            try
            {
                _log.Info("BrandNameOperator.GetProperty(): Started");

                var mibres = _snmpService.GetNext(SnmpVersion.V1, SnmpHelper.DefaultOctetString, new Oid(SnmpHelper.HrDevice),ipAddress);
                var mib = mibres.First().Data.ToString();
                mib = mib.Remove(0, SnmpHelper.Enterprise.Length + 1);
                var mibParts = mib.Split(Dot);

                var indexMib = mibParts[brIndex];
                var brandNameTable = XmlCommonLoader.Instance.BrandNameTable;
                result = brandNameTable[int.Parse(indexMib)].ToString();
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