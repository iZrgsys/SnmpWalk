namespace SnmpWalk.Engines.SnmpEngine.Types
{
    public class IpAddress
    {
        private string _ipAddress;

        public string Value 
        {
            get { return _ipAddress; }
            set { _ipAddress = value; }
        }

        public IpAddress()
        {
            _ipAddress = string.Empty;
        }

        public IpAddress(string ipAddress)
        {
            _ipAddress = ipAddress;
        }
    }
}
