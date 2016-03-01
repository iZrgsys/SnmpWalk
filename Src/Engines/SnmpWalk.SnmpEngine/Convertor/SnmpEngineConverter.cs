using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lextm.SharpSnmpLib;
using SnmpWalk.Engines.SnmpEngine.Types;

namespace SnmpWalk.Engines.SnmpEngine.Convertor
{
    public class SnmpEngineConverter
    {
        public static VersionCode VersionCodeConverter(SnmpVersion version)
        {
            switch (version)
            {
                case SnmpVersion.V1:
                    return VersionCode.V1;
                case SnmpVersion.V2:
                    return VersionCode.V2;
                case SnmpVersion.V3:
                     return VersionCode.V3;
                default:
                    throw new ArgumentOutOfRangeException(nameof(version), version, null);
            }
        }
    }
}
