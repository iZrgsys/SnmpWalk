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
using SnmpWalk.Engines.SnmpEngine.Types;
using TimeoutException = Lextm.SharpSnmpLib.Messaging.TimeoutException;

namespace SnmpWalk.Engines.SnmpEngine.Service
{
    public class SnmpEngineService : ISnmpEngine
    {
        private static readonly ILog Log = LogManager.GetLogger("application.log");
        private readonly SnmpEngineConverter _converter = new SnmpEngineConverter();
        private static readonly XmlCommonLoader XmlLoader = XmlCommonLoader.Instance;
        private int _timeOut;

        public int TimeOut
        {
            get { return _timeOut; }
            set { _timeOut = value; }
        }

        public static List<Oid> InitializeOids
        {
            get { return XmlLoader.Oids; }
        }

        private IEnumerable<SnmpResult> Walk(SnmpVersion version, IPAddress ipAddress, string octetString, Oid oid, WalkingMode walkMode)
        {
            Log.Info("SnmpEngine.Walk(): Started oid: " + oid.Value);
            var result = new List<SnmpResult>();

            foreach (var childOid in oid.ChildOids)
            {
                result.AddRange(!childOid.HasChildOids
                    ? WalkSingle(version, ipAddress, octetString, childOid, walkMode)
                    : Walk(version, ipAddress, octetString, childOid, walkMode));
            }

            Log.Info("SnmpEngine.Walk(): Finished");
            return result;
        }

        private IEnumerable<SnmpResult> WalkBulk(SnmpVersion version, IPAddress ipAddress, int maxBulkRepetitions, string octetString, Oid oid, WalkingMode walkMode)
        {
            Log.Info("SnmpEngine.WalkBulk(): Started oid: " + oid.Value);
            var result = new List<SnmpResult>();

            foreach (var childOid in oid.ChildOids)
            {
                result.AddRange(!childOid.HasChildOids ? WalkBulkSingle(version, ipAddress, maxBulkRepetitions, octetString, childOid, walkMode)
                    : WalkBulk(version, ipAddress, maxBulkRepetitions, octetString, childOid, walkMode));
            }

            Log.Info("SnmpEngine.WalkBulk(): Finished");
            return result;
        }

        private IEnumerable<SnmpResult> WalkBulkSingle(SnmpVersion version, IPAddress ipAddress, int maxBulkRepetitions, string octetString, Oid oid, WalkingMode walkMode)
        {
            Log.Info("SnmpEngine.WalkBulkSingle(): Started oid: " + oid.Value);
            var list = new List<Variable>();

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
                Log.Error("SnmpEngine.WalkBulkSingle(): Exception caught :", e);
                Log.Error("SnmpEngine.WalkBulkSingle(): Exception - Oid: " + oid.Value);
            }

            Log.Info("SnmpEngine.WalkBulkSingle(): Finished");
            return list.Select(var => new SnmpResult(new Oid(var.Id.ToString()), var.Data, _converter.ToSnmpDataType(var.Data.TypeCode)));
        }

        private IEnumerable<SnmpResult> WalkSingle(SnmpVersion version, IPAddress ipAddress, string octetString, Oid oid, WalkingMode walkMode)
        {
            Log.Info("SnmpEngine.WalkSingle(): Started oid: " + oid.Value);
            var list = new List<Variable>();

            if (oid.HasAdditionalCodes)
            {
                return WalkWithAdditionalCodes(version, ipAddress, octetString, oid, walkMode);
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
                Log.Error("SnmpEngine.WalkSingle(): Exception caught: ", e);
                Log.Error("SnmpEngine.WalkSingle(): Exception oid: " + oid.Value);
            }

            Log.Info("SnmpEngine.WalkSingle(): Finished");

            return list.Select(var => new SnmpResult(new Oid(var.Id.ToString()), var.Data, _converter.ToSnmpDataType(var.Data.TypeCode)));
        }

        private IEnumerable<SnmpResult> WalkWithAdditionalCodes(SnmpVersion version, IPAddress ipAddress, string octetString, Oid oid, WalkingMode walkMode)
        {
            Log.Info("SnmpEngine.WalkWithAdditionalCodes(): Started oid: " + oid.Value);
            var list = new List<Variable>();
            var results = new List<SnmpResult>();
            var codesTable = XmlLoader.AdditionalCodeTable;
            var codes = (Codes)codesTable[oid.Name];

            if (codes != null)
            {
                Parallel.ForEach(codes.Code, code =>
                {
                    try
                    {
                        Messenger.Walk(_converter.ToVersionCodeConverter(version),
                            new IPEndPoint(ipAddress, SnmpHelper.SnmpServerPort), new OctetString(octetString),
                            new ObjectIdentifier(string.Concat(oid.Value, ".", code.Decimal)), list, _timeOut,
                            _converter.ToWalkModeConverter(walkMode));
                    }
                    catch (Exception e)
                    {
                        Log.Error("SnmpEngine.WalkWithAdditionalCodes(): Exception caught:", e);
                        Log.Error("SnmpEngine.WalkWithAdditionalCodes(): Exception oid: " + string.Concat(oid.Value, ".", code.Decimal));
                    }

                    if (list.Any())
                    {
                        Log.Info("SnmpEngine.WalkWithAdditionalCodes(): request result oids: " + list.Count);
                        results.AddRange(list.Select(var => new SnmpResult(new Oid(string.Concat(oid.Value, ".", code.Decimal),
                            code.Name, string.Concat(oid.FullName, ".", code.Name)),
                            var.Data, _converter.ToSnmpDataType(var.Data.TypeCode))));
                    }
                });
            }

            Log.Info("SnmpEngine.WalkWithAdditionalCodes(): Finished");
            return results;
        }

        private IPAddress GetIpAddressFromHostName(string hostName)
        {
            var addresses = Dns.GetHostAddresses(hostName);
            if (addresses.Length == 0)
            {
                Log.Error("Unable to retrieve address from specified host name.");
                throw new ArgumentException("Unable to retrieve address from specified host name.", nameof(hostName));
            }
            if (addresses.Length > 1)
            {
                Log.Error("There is more that one IP address to the specified host.");
                throw new ArgumentException("There is more that one IP address to the specified host.", nameof(hostName));
            }
            return addresses[0];
        }

        public IEnumerable<SnmpResult> GetBulkOperation(SnmpVersion version, IPAddress ipAddress, string octetString, Oid oid)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<SnmpResult> GetNextOperation(SnmpVersion version, IPAddress ipAddress, string octetString, Oid oid)
        {
            Log.Info("SnmpEngine.GetNextOperation() : Started oid: " + oid.Value);
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
                    Log.Error("SnmpEngine.GetOperation():Timeout Exception caught:", e);
                    throw new SnmpTimeOutException(e.Message, _timeOut);
                }
                else if (e is ArgumentOutOfRangeException)
                {
                    Log.Error("SnmpEngine.GetOperation():Argument Out Of Range Exception caught:", e);
                    throw new SnmpEngineConvertorException((ArgumentOutOfRangeException)e);
                }
                else
                {
                    Log.Error("SnmpEngine.GetOperation():Exception caught:", e);
                    throw new SnmpEngineException(e.Message);
                }
            }
            finally
            {
                Log.Info("SnmpEngine.GetNextOperation(): Finished");
            }

            return results;
        }

        public IEnumerable<SnmpResult> GetOperation(SnmpVersion version, string hostName, string octetString, Oid oid)
        {
            Log.Info("SnmpEngine.GetOperation(): Started");

            var variable = new List<Variable>
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

                Messenger.Get(_converter.ToVersionCodeConverter(version), new IPEndPoint(GetIpAddressFromHostName(hostName), SnmpHelper.SnmpServerPort), new OctetString(octetString), variable, _timeOut);
            }
            catch (Exception e)
            {
                if (e is TimeoutException)
                {
                    Log.Error("SnmpEngine.GetOperation():Timeout Exception caught:", e);
                    throw new SnmpTimeOutException(e.Message, _timeOut);
                }
                else if (e is ArgumentOutOfRangeException)
                {
                    Log.Error("SnmpEngine.GetOperation():Argument Out Of Range Exception caught:", e);
                    throw new SnmpEngineConvertorException((ArgumentOutOfRangeException)e);
                }
                else
                {
                    Log.Error("SnmpEngine.GetOperation():Exception caught:", e);
                    throw new SnmpEngineException(e.Message);
                }
            }
            finally
            {
                Log.Info("SnmpEngine.GetOperation(): Finished");
            }

            return variable.Select(var => new SnmpResult(new Oid(var.Id.ToString()), var.Data, _converter.ToSnmpDataType(var.Data.TypeCode)));
        }

        public IEnumerable<SnmpResult> GetOperation(SnmpVersion version, IPAddress ipAddress, string octetString, Oid oid)
        {
            Log.Info("SnmpEngine.GetOperation(): Started");
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
                    Log.Error("SnmpEngine.GetOperation():Timeout Exception caught:", e);
                    throw new SnmpTimeOutException(e.Message, _timeOut);
                }
                else if (e is ArgumentOutOfRangeException)
                {
                    Log.Error("SnmpEngine.GetOperation():Argument Out Of Range Exception caught:", e);
                    throw new SnmpEngineConvertorException((ArgumentOutOfRangeException)e);
                }
                else
                {
                    Log.Error("SnmpEngine.GetOperation():Exception caught:", e);
                    throw new SnmpEngineException(e.Message);
                }
            }
            finally
            {
                Log.Info("SnmpEngine.GetOperation(): Finished");
            }

            return variables.Select(var => new SnmpResult(new Oid(var.Id.ToString()), var.Data, _converter.ToSnmpDataType(var.Data.TypeCode)));
        }

        public IEnumerable<SnmpResult> WalkOperation(SnmpVersion version, string hostName, string octetString, Oid oid, WalkingMode walkMode)
        {
            Log.Info("SnmpEngine.WalkOperation(): Started");

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

                result = Walk(version, GetIpAddressFromHostName(hostName), octetString, oid, walkMode).ToList();
            }
            catch (Exception e)
            {
                if (e is TimeoutException)
                {
                    Log.Error("SnmpEngine.WalkOperation():Timeout Exception caught:", e);
                    throw new SnmpTimeOutException(e.Message, _timeOut);
                }
                else if (e is ArgumentOutOfRangeException)
                {
                    Log.Error("SnmpEngine.WalkOperation():Argument Out Of Range Exception caught:", e);
                    throw new SnmpEngineConvertorException((ArgumentOutOfRangeException)e);
                }
                else
                {
                    Log.Error("SnmpEngine.WalkOperation():Exception caught:", e);
                    throw new SnmpEngineException(e.Message);
                }
            }
            finally
            {
                Log.Info("SnmpEngine.WalkOperation(): Finished");
            }

            return result;
        }

        public IEnumerable<SnmpResult> WalkBulkOperation(SnmpVersion version, IPAddress ipAddress, int maxBulkRepetiotions, string octetString, Oid oid, WalkingMode walkMode)
        {
            Log.Info("SnmpEngine.WalkBulkOperation(): Started");
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
                    Log.Error("SnmpEngine.WalkBulkOperation():Timeout Exception caught:", e);
                    throw new SnmpTimeOutException(e.Message, _timeOut);
                }
                else if (e is ArgumentOutOfRangeException)
                {
                    Log.Error("SnmpEngine.WalkBulkOperation():Argument Out Of Range Exception caught:", e);
                    throw new SnmpEngineConvertorException((ArgumentOutOfRangeException)e);
                }
                else
                {
                    Log.Error("SnmpEngine.WalkBulkOperation():Exception caught:", e);
                    throw new SnmpEngineException(e.Message);
                }
            }
            finally
            {
                Log.Info("SnmpEngine.WalkBulkOperation(): Finished");
            }

            return results;
        }

        public IEnumerable<SnmpResult> WalkOperation(SnmpVersion version, IPAddress ipAddress, string octetString, Oid oid, WalkingMode walkMode)
        {
            Log.Info("SnmpEngine.WalkOperation(): Started");

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
                    Log.Error("SnmpEngine.WalkOperation():Timeout Exception caught:", e);
                    throw new SnmpTimeOutException(e.Message, _timeOut);
                }
                else if (e is ArgumentOutOfRangeException)
                {
                    Log.Error("SnmpEngine.WalkOperation():Argument Out Of Range Exception caught:", e);
                    throw new SnmpEngineConvertorException((ArgumentOutOfRangeException)e);
                }
                else
                {
                    Log.Error("SnmpEngine.WalkOperation():Exception caught:", e);
                    throw new SnmpEngineException(e.Message);
                }
            }
            finally
            {
                Log.Info("SnmpEngine.WalkOperation(): Finished");
            }

            return result;
        }
    }
}
