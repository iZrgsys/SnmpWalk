using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using GalaSoft.MvvmLight;
using SnmpWalk.Client.Assets;
using SnmpWalk.Client.Assets.Enums;
using SnmpWalk.Common.DataModel.Enums;
using SnmpWalk.Engines.DiscoveryEngine;
using SnmpWalk.Engines.DiscoveryEngine.Service;
using SnmpWalk.Engines.SnmpEngine;
using SnmpWalk.Engines.SnmpEngine.Service;
using SnmpWalk.Engines.SnmpEngine.Types;
using static System.String;
using RelayCommand = GalaSoft.MvvmLight.Command.RelayCommand;
using SnmpVersion = SnmpWalk.Client.Assets.Enums.SnmpVersion;

namespace SnmpWalk.Client.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// You can also use Blend to data bind with the tool's support.
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private SnmpOperationType _currertSnmpOperation = SnmpOperationType.Get;
        private SnmpVersion _currentSnmpVersion = SnmpVersion.V1;
        private IDiscoveryEngine _discoveryEngine;
        private readonly OidTreeViewModel _oidTreeViewModel;
        private readonly ISnmpEngine _snmpEngine;
        private string _results;
        private List<SnmpResult> _snmpResults;
        private string _ipAddress;
        private bool _performActionEnabled = true;
        private bool _isLoading = false;

        public RelayCommand IfDeviceAvaliableCommand { get; private set; }
        public RelayCommand PerformActionCommand { get; private set; }
        public RelayCommand CancelActionCommand { get; private set; }

        public string IpAddress
        {
            get { return _ipAddress; }
            set
            {
                _ipAddress = value;
                RaisePropertyChanged();
                IfDeviceAvaliableCommand.RaiseCanExecuteChanged();
            }

        }

        public bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                _isLoading = value;
                RaisePropertyChanged();
            }
        }

        public bool PerformActionEnabled
        {
            get { return _performActionEnabled; }
            set
            {
                _performActionEnabled = value;
                RaisePropertyChanged();
            }
        }

        public List<SnmpResult> SnmpResults
        {
            get { return _snmpResults; }
            set
            {
                _snmpResults = value;
                RaisePropertyChanged();
            }
        } 

        public string Result
        {
            get { return _results; }
            set
            {
                _results = value;
                RaisePropertyChanged();
            }
        }

        public object CurrertSnmpOperation
        {
            get { return _currertSnmpOperation; }
            set
            {
                var val = (EnumerationExtension.EnumerationMember)value;
                if (val != null)
                {
                    _currertSnmpOperation = (SnmpOperationType)val.Value;
                }
                RaisePropertyChanged();
            }
        }

        public object CurrertSnmpVersion
        {
            get { return _currentSnmpVersion; }
            set
            {
                var val = (EnumerationExtension.EnumerationMember)value;
                if (val != null)
                {
                    _currentSnmpVersion = (SnmpVersion)val.Value;
                }
                RaisePropertyChanged();
            }
        }

        public OidTreeViewModel OidTree
        {
            get { return _oidTreeViewModel; }
        }

        public bool CanCheckDevice()
        {
            return !IsNullOrEmpty(IpAddress);
        }

        public void CheckDevice()
        {
            var devices = _discoveryEngine.PerformPinging(_ipAddress);

            if (devices.Any())
            {

            }

        }

        public bool CanPerformAction()
        {
            return true;
        }

        public async void PerformActionAsync()
        {
            if (_currertSnmpOperation == SnmpOperationType.Walk)
            {
                PerformActionEnabled = false;
                IsLoading = true;
                SnmpResults = null;

                try
                {
                    SnmpResults = await WalkAsync();
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }


                PerformActionEnabled = true;
                IsLoading = false;

            }
        }

        private Task<List<SnmpResult>> WalkAsync()
        {
            return Task.Run(() => Walk());
        }

        private List<SnmpResult> Walk()
        {
            return _snmpEngine.WalkOperation(ConvertToCommonVersion(_currentSnmpVersion), IPAddress.Parse(_ipAddress), null, _oidTreeViewModel.OidSelected, WalkingMode.WithinSubtree).ToList();
        }

        private Common.DataModel.Snmp.SnmpVersion ConvertToCommonVersion(SnmpVersion snmpVersion)
        {
            switch (snmpVersion)
            {
                case SnmpVersion.V1:
                    return Common.DataModel.Snmp.SnmpVersion.V1;
                case SnmpVersion.V2:
                    return Common.DataModel.Snmp.SnmpVersion.V2;
                case SnmpVersion.V3:
                    return Common.DataModel.Snmp.SnmpVersion.V3;
                default:
                    throw new ArgumentOutOfRangeException(nameof(snmpVersion), snmpVersion, null);
            }
        }

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            _oidTreeViewModel = new OidTreeViewModel();
            IfDeviceAvaliableCommand = new RelayCommand(CheckDevice, CanCheckDevice);
            PerformActionCommand = new RelayCommand(PerformActionAsync, CanPerformAction);
            _snmpEngine = new SnmpEngineService();
            _discoveryEngine = DiscoveryEngineService.Instance;
            ////if (IsInDesignMode)
            ////{
            ////    // Code runs in Blend --> create design time data.
            ////}
            ////else
            ////{
            ////    // Code runs "for real"
            ////}
        }
    }
}