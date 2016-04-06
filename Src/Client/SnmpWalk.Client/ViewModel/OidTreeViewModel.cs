using System.Collections.Generic;
using GalaSoft.MvvmLight;
using SnmpWalk.Common.DataModel.Snmp;
using SnmpWalk.Engines.SnmpEngine.Service;

namespace SnmpWalk.Client.ViewModel
{
    public class OidTreeViewModel:ViewModelBase
    {
        private static List<Oid> _oids;
        private Oid _oidCurrent = new Oid();
        private bool _showMode;

        public bool ShowMode
        {
            get { return _showMode; }
            set { _showMode = value; }
        }

        public string OidCurrent
        {
            get
            {
                if (ShowMode)
                {
                    return _oidCurrent.FullName ?? _oidCurrent.Value;
                }
                return _oidCurrent.Value;
            }
            set
            {
                if (value == null)
                {
                    return;
                }

                _oidCurrent.Value = value;
                RaisePropertyChanged();
            }
        }

        public List<Oid> Oids
        {
            get { return _oids; }
        } 

        static OidTreeViewModel()
        {
            _oids = SnmpEngineService.InitializeOids;
        }
    }
}