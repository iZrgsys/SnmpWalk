using System.Collections.Generic;
using SnmpWalk.Engines.SnmpEngine.Service;

namespace SnmpWalk.Client.Models
{
    public class OidTreeModel
    {
        private List<OidModel> _oidModels;

        public OidTreeModel()
        {
            _oidModels = new List<OidModel>();
        }

        private void Init()
        {
            
        }
    }
}
