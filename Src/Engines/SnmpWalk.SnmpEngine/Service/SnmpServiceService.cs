using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using log4net;
using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using SnmpWalk.Common.DataModel.Enums;
using SnmpWalk.Common.DataModel.Snmp;
using SnmpWalk.Engines.SnmpEngine.ConfigurationLoader;
using SnmpWalk.Engines.SnmpEngine.Convertor;
using SnmpWalk.Engines.SnmpEngine.Exceptions;
using SnmpWalk.Engines.SnmpEngine.Opeartors;
using SnmpWalk.Engines.SnmpEngine.Types;
using TimeoutException = Lextm.SharpSnmpLib.Messaging.TimeoutException;

namespace SnmpWalk.Engines.SnmpEngine.Service
{
    public class SnmpServiceService : ISnmpService
    {
        private readonly ILog _log;
        private readonly SnmpEngineConverter _converter = new SnmpEngineConverter();
        private static readonly XmlCommonLoader XmlLoader = XmlCommonLoader.Instance;
        private int _timeOut;

        public int TimeOut
        {
            get { return _timeOut; }
            set { _timeOut = value; }
        }

        public IEnumerable<SnmpResult> Get(SnmpVersion version, string octetString, Oid oid, IPAddress ipAddress = null, string hostname = null)
        {

            if (ipAddress != null)
            {
                return GetOperation(version, ipAddress, octetString, oid);
            }
            else if (!string.IsNullOrEmpty(hostname))
            {
                return GetOperation(version, GetIpAddressFromHostName(hostname), octetString, oid);
            }

            return null;
        }

        public IEnumerable<SnmpResult> GetNext(SnmpVersion version, string octetString, Oid oid, IPAddress ipAddress = null, string hostname = null)
        {
            if (ipAddress != null)
            {
                return GetNextOperation(version, ipAddress, octetString, oid);
            }
            else if (!string.IsNullOrEmpty(hostname))
            {
                return GetNextOperation(version, GetIpAddressFromHostName(hostname), octetString, oid);
            }

            return null;
        }

        public IEnumerable<SnmpResult> GetBulk(SnmpVersion version, int maxBulkRepetiotions, string octetString, Oid oid,
            IPAddress ipAddress = null, string hostname = null)
        {
            if (ipAddress != null)
            {
                return GetBulkOperation(version, ipAddress, maxBulkRepetiotions, octetString, oid);
            }
            else if (!string.IsNullOrEmpty(hostname))
            {
                return GetBulkOperation(version, GetIpAddressFromHostName(hostname), maxBulkRepetiotions, octetString, oid);
            }

            return null;
        }

        public IEnumerable<SnmpResult> Walk(SnmpVersion version, string octetString, Oid oid, WalkingMode walkMode, IPAddress ipAddress = null,
            string hostname = null)
        {
            if (ipAddress != null)
            {
                return WalkOperation(version, ipAddress, octetString, oid, walkMode);
            }
            else if (!string.IsNullOrEmpty(hostname))
            {
                return WalkOperation(version, GetIpAddressFromHostName(hostname), octetString, oid, walkMode);
            }

            return null;
        }

        public IEnumerable<SnmpResult> WalkBulk(SnmpVersion version, int maxBulkRepetiotions, string octetString, Oid oid, WalkingMode walkMode,
            IPAddress ipAddress = null, string hostname = null)
        {
            if (ipAddress != null)
            {
                return WalkBulkOperation(version, ipAddress, maxBulkRepetiotions, octetString, oid, walkMode);
            }
            else if (!string.IsNullOrEmpty(hostname))
            {
                return WalkBulkOperation(version, GetIpAddressFromHostName(hostname), maxBulkRepetiotions, octetString, oid, walkMode);
            }

            return null;
        }

        public static List<Oid> InitializeOids
        {
            get { return XmlLoader.Oids; }
        }

        private IEnumerable<SnmpResult> Walk(SnmpVersion version, IPAddress ipAddress, string octetString, Oid oid, WalkingMode walkMode)
        {
            _log.Info("SnmpEngine.Walk(): Started oid: " + oid.Value);
            var result = new List<SnmpResult>();

            foreach (var childOid in oid.ChildOids)
            {
                result.AddRange(!childOid.HasChildOids
                    ? WalkSingle(version, ipAddress, octetString, childOid, walkMode)
                    : Walk(version, ipAddress, octetString, childOid, walkMode));
            }

            _log.Info("SnmpEngine.Walk(): Finished");
            return result;
        }

        private IEnumerable<SnmpResult> WalkBulk(SnmpVersion version, IPAddress ipAddress, int maxBulkRepetitions, string octetString, Oid oid, WalkingMode walkMode)
        {
            _log.Info("SnmpEngine.WalkBulk(): Started oid: " + oid.Value);
            var result = new List<SnmpResult>();

            foreach (var childOid in oid.ChildOids)
            {
                result.AddRange(!childOid.HasChildOids ? WalkBulkSingle(version, ipAddress, maxBulkRepetitions, octetString, childOid, walkMode)
                    : WalkBulk(version, ipAddress, maxBulkRepetitions, octetString, childOid, walkMode));
            }

            _log.Info("SnmpEngine.WalkBulk(): Finished");
            return result;
        }

        private IEnumerable<SnmpResult> WalkBulkSingle(SnmpVersion version, IPAddress ipAddress, int maxBulkRepetitions, string octetString, Oid oid, WalkingMode walkMode)
        {
            _log.Info("SnmpEngine.WalkBulkSingle(): Started oid: " + oid.Value);
            var list = new List<Variable>();

            if (oid.HasAdditionalCodes)
            {
                return WalkBulkWithAdditionalCodes(version, ipAddress, maxBulkRepetitions, octetString, oid, walkMode).Result;
            }

            try
            {
                Messenger.BulkWalk(_converter.ToVersionCodeConverter(version),
                new IPEndPoint(ipAddress, SnmpHelper.SnmpServerPort),
                new OctetString(octetString),
                new ObjectIdentifier(oid.Value),
                list,
                _timeOut,
                maxBulkRepetitions,
                _converter.ToWalkModeConverter(walkMode),
                null,
                null);
            }
            catch (Exception e)
            {
                _log.Error("SnmpEngine.WalkBulkSingle(): Exception caught :", e);
                _log.Error("SnmpEngine.WalkBulkSingle(): Exception - Oid: " + oid.Value);
            }

            _log.Info("SnmpEngine.WalkBulkSingle(): Finished");
            return list.Select(var => new SnmpResult(new Oid(var.Id.ToString()), var.Data, _converter.ToSnmpDataType(var.Data.TypeCode)));
        }

        private async Task<List<SnmpResult>> WalkBulkWithAdditionalCodes(SnmpVersion version, IPAddress ipAddress, int maxBulkRepetitions, string octetString, Oid oid, WalkingMode walkMode)
        {
            _log.Info("SnmpEngine.WalkWithAdditionalCodes(): Started oid: " + oid.Value);
            var list = new List<Variable>();
            var results = new List<SnmpResult>();
            var codesTable = XmlLoader.AdditionalCodeTable;
            var codes = (Codes)codesTable[oid.Name];
            var brandNameOp = new BrandNameOperator(_log, this);
            var brandName = brandNameOp.GetProperty(ipAddress);

            if (codes != null)
            {
                await Task.Run(() =>
                {
                    foreach (var code in codes.Code)
                    {
                        try
                        {
                            if (code.Name.ToLower().Contains(brandName.ToLower()))
                            {
                                Messenger.BulkWalk(_converter.ToVersionCodeConverter(version),
                                 new IPEndPoint(ipAddress, SnmpHelper.SnmpServerPort),
                                 new OctetString(octetString),
                                 new ObjectIdentifier(string.Concat(oid.Value, ".", code.Decimal)),
                                 list,
                                 _timeOut,
                                 maxBulkRepetitions,
                                _converter.ToWalkModeConverter(walkMode),
                                null,
                                null);
                            }
                        }
                        catch (Exception e)
                        {
                            _log.Error("SnmpEngine.WalkWithAdditionalCodes(): Exception caught:", e);
                            _log.Error("SnmpEngine.WalkWithAdditionalCodes(): Exception oid: " + string.Concat(oid.Value, ".", code.Decimal));
                        }

                        if (list.Any())
                        {
                            _log.Info("SnmpEngine.WalkWithAdditionalCodes(): sucess oid: " + string.Concat(oid.Value, ".", code.Decimal));
                            _log.Info("SnmpEngine.WalkWithAdditionalCodes(): request result oids: " + list.Count);
                            results.AddRange(list.Select(var => new SnmpResult(new Oid(string.Concat(oid.Value, ".", code.Decimal),
                                code.Name, string.Concat(oid.FullName, ".", code.Name)),
                                var.Data, _converter.ToSnmpDataType(var.Data.TypeCode))));
                        }
                    }
                });
            }

            _log.Info("SnmpEngine.WalkWithAdditionalCodes(): Finished");
            return results;
        }

        private IEnumerable<SnmpResult> WalkSingle(SnmpVersion version, IPAddress ipAddress, string octetString, Oid oid, WalkingMode walkMode)
        {
            _log.Info("SnmpEngine.WalkSingle(): Started oid: " + oid.Value);
            var list = new List<Variable>();

            if (oid.HasAdditionalCodes)
            {
                return WalkWithAdditionalCodesTask(version, ipAddress, octetString, oid, walkMode).Result;
            }

            try
            {
                Messenger.Walk(
                _converter.ToVersionCodeConverter(version),
                new IPEndPoint(ipAddress, SnmpHelper.SnmpServerPort),
                new OctetString(octetString),
                new ObjectIdentifier(oid.Value),
                list,
                _timeOut,
                _converter.ToWalkModeConverter(walkMode));
            }
            catch (Exception e)
            {
                _log.Error("SnmpEngine.WalkSingle(): Exception caught: ", e);
                _log.Error("SnmpEngine.WalkSingle(): Exception oid: " + oid.Value);
            }

            _log.Info("SnmpEngine.WalkSingle(): Finished");

            return list.Select(var => new SnmpResult(new Oid(var.Id.ToString()), var.Data, _converter.ToSnmpDataType(var.Data.TypeCode)));
        }

        private async Task<List<SnmpResult>> WalkWithAdditionalCodesTask(SnmpVersion version, IPAddress ipAddress, string octetString, Oid oid, WalkingMode walkMode)
        {
            _log.Info("SnmpEngine.WalkWithAdditionalCodes(): Started oid: " + oid.Value);
            var list = new List<Variable>();
            var results = new List<SnmpResult>();
            var codesTable = XmlLoader.AdditionalCodeTable;
            var codes = (Codes)codesTable[oid.Name];
            var brandNameOp = new BrandNameOperator(_log, this);
            var brandName = brandNameOp.GetProperty(ipAddress);

            if (codes != null)
            {
                await Task.Run(() =>
                {
                    foreach (var code in codes.Code)
                    {
                        try
                        {
                            if (code.Name.ToLower().Contains(brandName.ToLower()))
                            {
                                Messenger.Walk(_converter.ToVersionCodeConverter(version),
                                new IPEndPoint(ipAddress, SnmpHelper.SnmpServerPort), new OctetString(octetString),
                                new ObjectIdentifier(string.Concat(oid.Value, ".", code.Decimal)), list, _timeOut,
                                _converter.ToWalkModeConverter(walkMode));
                            }
                        }
                        catch (Exception e)
                        {
                            _log.Error("SnmpEngine.WalkWithAdditionalCodes(): Exception caught:", e);
                            _log.Error("SnmpEngine.WalkWithAdditionalCodes(): Exception oid: " + string.Concat(oid.Value, ".", code.Decimal));
                        }

                        if (list.Any())
                        {
                            _log.Info("SnmpEngine.WalkWithAdditionalCodes(): sucess oid: " + string.Concat(oid.Value, ".", code.Decimal));
                            _log.Info("SnmpEngine.WalkWithAdditionalCodes(): request result oids: " + list.Count);
                            results.AddRange(list.Select(var => new SnmpResult(new Oid(string.Concat(oid.Value, ".", code.Decimal),
                                code.Name, string.Concat(oid.FullName, ".", code.Name)),
                                var.Data, _converter.ToSnmpDataType(var.Data.TypeCode))));
                        }
                    }
                });
            }

            _log.Info("SnmpEngine.WalkWithAdditionalCodes(): Finished");
            return results;
        }

        private IPAddress GetIpAddressFromHostName(string hostName)
        {
            var addresses = Dns.GetHostAddresses(hostName);
            if (addresses.Length == 0)
            {
                _log.Error("Unable to retrieve address from specified host name.");
                throw new ArgumentException("Unable to retrieve address from specified host name.", nameof(hostName));
            }
            if (addresses.Length > 1)
            {
                _log.Error("There is more that one IP address to the specified host.");
                throw new ArgumentException("There is more that one IP address to the specified host.", nameof(hostName));
            }
            return addresses[0];
        }

        private IEnumerable<SnmpResult> GetBulkOperation(SnmpVersion version, IPAddress ipAddress, int maxBulkRepetiotions, string octetString, Oid oid)
        {
            _log.Info("SnmpEngine.GetNextOperation() : Started oid: " + oid.Value);
            List<SnmpResult> results;
            var variable = new List<Variable>
            {
                new Variable(new ObjectIdentifier(oid.Value))
            };

            try
            {
                var getNextRequest = new GetBulkRequestMessage(0,
                    _converter.ToVersionCodeConverter(version),
                    new OctetString(octetString),
                    0,
                    maxBulkRepetiotions,
                    variable);
                var responce = getNextRequest.GetResponse(SnmpHelper.DefaultTimeOut, new IPEndPoint(ipAddress, SnmpHelper.SnmpServerPort));

                if (responce.Pdu().ErrorStatus.ToInt32() != 0)
                {
                    throw new SnmpEngineException("SnmpEngine.GetNextOperation() error status = 0; oid = " + oid.Value);
                }

                results = responce.Pdu().Variables.Select(var => new SnmpResult(new Oid(var.Id.ToString()), var.Data, _converter.ToSnmpDataType(var.Data.TypeCode))).ToList();
            }
            catch (Exception e)
            {
                if (e is TimeoutException)
                {
                    _log.Error("SnmpEngine.GetOperation():Timeout Exception caught:", e);
                    throw new SnmpTimeOutException(e.Message, _timeOut);
                }
                else if (e is ArgumentOutOfRangeException)
                {
                    _log.Error("SnmpEngine.GetOperation():Argument Out Of Range Exception caught:", e);
                    throw new SnmpEngineConvertorException((ArgumentOutOfRangeException)e);
                }
                else
                {
                    _log.Error("SnmpEngine.GetOperation():Exception caught:", e);
                    throw new SnmpEngineException(e.Message);
                }
            }
            finally
            {
                _log.Info("SnmpEngine.GetNextOperation(): Finished");
            }

            return results;
        }

        private IEnumerable<SnmpResult> GetNextOperation(SnmpVersion version, IPAddress ipAddress, string octetString, Oid oid)
        {
            _log.Info("SnmpEngine.GetNextOperation() : Started oid: " + oid.Value);
            List<SnmpResult> results;
            var variable = new List<Variable>
            {
                new Variable(new ObjectIdentifier(oid.Value))
            };

            try
            {
                var getNextRequest = new GetNextRequestMessage(0, _converter.ToVersionCodeConverter(version), new OctetString(octetString), variable);
                var responce = getNextRequest.GetResponse(SnmpHelper.DefaultTimeOut, new IPEndPoint(ipAddress, SnmpHelper.SnmpServerPort));

                if (responce.Pdu().ErrorStatus.ToInt32() != 0)
                {
                    throw new SnmpEngineException("SnmpEngine.GetNextOperation() error status = 0; oid = " + oid.Value);
                }

                results = responce.Pdu().Variables.Select(var => new SnmpResult(new Oid(var.Id.ToString()), var.Data, _converter.ToSnmpDataType(var.Data.TypeCode))).ToList();
            }
            catch (Exception e)
            {
                if (e is TimeoutException)
                {
                    _log.Error("SnmpEngine.GetOperation():Timeout Exception caught:", e);
                    throw new SnmpTimeOutException(e.Message, _timeOut);
                }
                else if (e is ArgumentOutOfRangeException)
                {
                    _log.Error("SnmpEngine.GetOperation():Argument Out Of Range Exception caught:", e);
                    throw new SnmpEngineConvertorException((ArgumentOutOfRangeException)e);
                }
                else
                {
                    _log.Error("SnmpEngine.GetOperation():Exception caught:", e);
                    throw new SnmpEngineException(e.Message);
                }
            }
            finally
            {
                _log.Info("SnmpEngine.GetNextOperation(): Finished");
            }

            return results;
        }

        private IEnumerable<SnmpResult> GetOperation(SnmpVersion version, IPAddress ipAddress, string octetString, Oid oid)
        {
            _log.Info("SnmpEngine.GetOperation(): Started");
            var variables = new List<Variable>
            {
                new Variable(new ObjectIdentifier(oid.Value))
            };

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

                Messenger.Get(_converter.ToVersionCodeConverter(version), new IPEndPoint(ipAddress, SnmpHelper.SnmpServerPort), new OctetString(octetString), variables, _timeOut);
            }
            catch (Exception e)
            {
                if (e is TimeoutException)
                {
                    _log.Error("SnmpEngine.GetOperation():Timeout Exception caught:", e);
                    throw new SnmpTimeOutException(e.Message, _timeOut);
                }
                else if (e is ArgumentOutOfRangeException)
                {
                    _log.Error("SnmpEngine.GetOperation():Argument Out Of Range Exception caught:", e);
                    throw new SnmpEngineConvertorException((ArgumentOutOfRangeException)e);
                }
                else
                {
                    _log.Error("SnmpEngine.GetOperation():Exception caught:", e);
                    throw new SnmpEngineException(e.Message);
                }
            }
            finally
            {
                _log.Info("SnmpEngine.GetOperation(): Finished");
            }

            return variables.Select(var => new SnmpResult(new Oid(var.Id.ToString()), var.Data, _converter.ToSnmpDataType(var.Data.TypeCode)));
        }

        private IEnumerable<SnmpResult> WalkBulkOperation(SnmpVersion version, IPAddress ipAddress, int maxBulkRepetiotions, string octetString, Oid oid, WalkingMode walkMode)
        {
            _log.Info("SnmpEngine.WalkBulkOperation(): Started");
            List<SnmpResult> results;

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

                results = oid.HasChildOids ? WalkBulk(version, ipAddress, maxBulkRepetiotions, octetString, oid, walkMode).ToList() : WalkBulkSingle(version, ipAddress, maxBulkRepetiotions, octetString, oid, walkMode).ToList();

            }
            catch (Exception e)
            {
                if (e is TimeoutException)
                {
                    _log.Error("SnmpEngine.WalkBulkOperation():Timeout Exception caught:", e);
                    throw new SnmpTimeOutException(e.Message, _timeOut);
                }
                else if (e is ArgumentOutOfRangeException)
                {
                    _log.Error("SnmpEngine.WalkBulkOperation():Argument Out Of Range Exception caught:", e);
                    throw new SnmpEngineConvertorException((ArgumentOutOfRangeException)e);
                }
                else
                {
                    _log.Error("SnmpEngine.WalkBulkOperation():Exception caught:", e);
                    throw new SnmpEngineException(e.Message);
                }
            }
            finally
            {
                _log.Info("SnmpEngine.WalkBulkOperation(): Finished");
            }

            return results;
        }

        private IEnumerable<SnmpResult> WalkOperation(SnmpVersion version, IPAddress ipAddress, string octetString, Oid oid, WalkingMode walkMode)
        {
            _log.Info("SnmpEngine.WalkOperation(): Started");

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
                _log.Info("SnmpEngine.WalkOperation(): Finished");
            }

            return result;
        }

        public SnmpServiceService(ILog logger)
        {
            _log = logger;
        }
    }
}
