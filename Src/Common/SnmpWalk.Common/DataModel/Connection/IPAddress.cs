using SnmpWalk.Common.DataModel.Enums;

namespace SnmpWalk.Common.DataModel.Connection
{
    public class IpAddress
    {
        private string _ipAddress;
        private IpVersion _ipVersion;

        public string Value 
        {
            get { return _ipAddress; }
            set { _ipAddress = value; }
        }

        public IpVersion IpVersion
        {
            get { return _ipVersion; }
            set { _ipVersion = value; }
        }

        public IpAddress()
        {
            _ipAddress = string.Empty;
            _ipVersion = IpVersion.Default;
        }

        public IpAddress(string ipAddress)
        {
            _ipAddress = ipAddress;
            _ipVersion = IpVersion.Default;
        }

        public IpAddress(string ipAddress, IpVersion version)
        {
            _ipAddress = ipAddress;
            _ipVersion = version;
        }
    }
}
