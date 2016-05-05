using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using log4net;
using SnmpWalk.Client.Assets;
using SnmpWalk.Client.Assets.Enums;
using SnmpWalk.Common.DataModel.Enums;
using SnmpWalk.Common.DataModel.Snmp;
using SnmpWalk.Engines.SnmpEngine;
using SnmpWalk.Engines.SnmpEngine.Service;
using SnmpWalk.Engines.SnmpEngine.Types;
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
    public class MainViewModel : ViewModelBase, IDataErrorInfo
    {
        private readonly OidTreeViewModel _oidTreeViewModel;
        private readonly ISnmpService _snmpService;
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private SnmpOperationType _currertSnmpOperation = SnmpOperationType.Get;
        private SnmpVersion _currentSnmpVersion = SnmpVersion.V1;
        private List<SnmpResult> _snmpResults;
        private Visibility __stopCancelToolBarVisibility = Visibility.Collapsed;

        private string _ipAddress;
        private string _hostName;
        private string _readCommunity = "public";
        private string _writeCommunity = "public";
        private string _loadingMsg;
        private bool _performActionEnabled = true;
        private bool _isLoading;
        private int _maxBulkReps;
        private bool _isMaxBulkReptEnabled;

        public RelayCommand IfDeviceAvaliableCommand { get; private set; }
        public RelayCommand PerformActionCommand { get; private set; }
        //public RelayCommand CancelActionCommand { get; private set; }

        public string IpAddress
        {
            get { return _ipAddress; }
            set
            {
                _ipAddress = value;

                //if (Common.CommonHelper.HostNameRegex.IsMatch(value))
                //{
                //    _ipAddress = null;
                //    _hostName = value;
                //}

                RaisePropertyChanged();
                IfDeviceAvaliableCommand.RaiseCanExecuteChanged();
            }

        }

        public Visibility StopCancelToolBarVisibility
        {
            get { return __stopCancelToolBarVisibility; }
            set
            {
                __stopCancelToolBarVisibility = value;
                RaisePropertyChanged();
            }
        }

        public int MaxBulkRept
        {
            get { return _maxBulkReps; }
            set
            {
                _maxBulkReps = value;
                RaisePropertyChanged();
            }
        }

        public bool IsMaxBulkReptEnabled
        {
            get { return _isMaxBulkReptEnabled; }
            set
            {
                _isMaxBulkReptEnabled = value; 
                RaisePropertyChanged();
            }
        }

        public string WriteCommunity
        {
            get { return _writeCommunity; }
            set
            {
                _writeCommunity = value;
                RaisePropertyChanged();
            }
        }

        public string ReadCommunity
        {
            get { return _readCommunity; }
            set
            {
                _readCommunity = value;
                RaisePropertyChanged();
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

        public object CurrertSnmpOperation
        {
            get { return _currertSnmpOperation; }
            set
            {
                var val = (EnumerationExtension.EnumerationMember)value;
                if (val != null)
                {
                    _currertSnmpOperation = (SnmpOperationType)val.Value;

                    if (_currertSnmpOperation == SnmpOperationType.GetBulk || _currertSnmpOperation == SnmpOperationType.WalkBulk)
                    {
                        IsMaxBulkReptEnabled = true;
                    }
                    else
                    {
                        IsMaxBulkReptEnabled = false;
                    }
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
            return !string.IsNullOrEmpty(IpAddress) && Common.CommonHelper.IpRegex.IsMatch(IpAddress);
        }

        public void CheckDevice()
        {
            Messenger.Default.Send(new NotificationMessage("ShowDeviceManagementWindow"));
        }

        public bool CanPerformAction()
        {
            return true;
        }

        public async void PerformActionAsync()
        {
            PerformActionEnabled = false;
            IsLoading = true;
            SnmpResults = null;

            if (_currertSnmpOperation == SnmpOperationType.Walk)
            {
                try
                {
                    SnmpResults = await WalkAsync();
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            if (_currertSnmpOperation == SnmpOperationType.Get)
            {
                try
                {
                    SnmpResults = await GetAsync();
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            if (_currertSnmpOperation == SnmpOperationType.GetNext)
            {
                try
                {
                    SnmpResults = await GetNextAsync();
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            if (_currertSnmpOperation == SnmpOperationType.WalkBulk && (_currentSnmpVersion == SnmpVersion.V2 || _currentSnmpVersion == SnmpVersion.V3))
            {
                try
                {
                    SnmpResults = await WalkBulkAsync();
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            if (_currertSnmpOperation == SnmpOperationType.GetBulk && (_currentSnmpVersion == SnmpVersion.V2 || _currentSnmpVersion == SnmpVersion.V3))
            {
                try
                {
                    SnmpResults = await GetBulkAsync();
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            PerformActionEnabled = true;
            IsLoading = false;
        }

        private Task<List<SnmpResult>> WalkAsync()
        {
            return Task.Run(() => Walk());
        }

        private Task<List<SnmpResult>> WalkBulkAsync()
        {
            return Task.Run(() => WalkBulk());
        }

        private Task<List<SnmpResult>> GetAsync()
        {
            return Task.Run(() => Get());
        }

        private Task<List<SnmpResult>> GetNextAsync()
        {
            return Task.Run(() => GetNext());
        }

        private Task<List<SnmpResult>> GetBulkAsync()
        {
            return Task.Run(() => GetBulk());
        }

        private List<SnmpResult> WalkBulk()
        {
            Log.Info("Client:Walk operation started!...");
            return _snmpService.WalkBulk(ConvertToCommonVersion(_currentSnmpVersion), _maxBulkReps, _readCommunity, _oidTreeViewModel.OidSelected, WalkingMode.WithinSubtree, IPAddress.Parse(_ipAddress), _hostName).ToList();
        }

        private List<SnmpResult> GetBulk()
        {
            Log.Info("Client:GetBulk operation started!...");
            return _snmpService.GetBulk(ConvertToCommonVersion(_currentSnmpVersion), _maxBulkReps, _readCommunity, _oidTreeViewModel.OidSelected, IPAddress.Parse(_ipAddress), _hostName).ToList();
        }

        private List<SnmpResult> Walk()
        {
            Log.Info("Client:Walk operation started!...");
            return _snmpService.Walk(ConvertToCommonVersion(_currentSnmpVersion), _readCommunity, _oidTreeViewModel.OidSelected, WalkingMode.WithinSubtree, IPAddress.Parse(_ipAddress), _hostName).ToList();
        }

        private List<SnmpResult> Get()
        {
            Log.Info("Client:Get operation started!...");
            return _snmpService.Get(ConvertToCommonVersion(_currentSnmpVersion), _readCommunity, _oidTreeViewModel.OidSelected, IPAddress.Parse(_ipAddress), _hostName).ToList();
        }

        private List<SnmpResult> GetNext()
        {
            Log.Info("Client:GetNext operation started!...");
            return _snmpService.GetNext(ConvertToCommonVersion(_currentSnmpVersion), _readCommunity, _oidTreeViewModel.OidSelected, IPAddress.Parse(_ipAddress), _hostName).ToList();
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
            Log.Info("Client: Main View Model initializing...");
            _oidTreeViewModel = new OidTreeViewModel();
            IfDeviceAvaliableCommand = new RelayCommand(CheckDevice, CanCheckDevice);
            PerformActionCommand = new RelayCommand(PerformActionAsync, CanPerformAction);
            _snmpService = new SnmpServiceService();

            ////if (IsInDesignMode)
            ////{
            ////    // Code runs in Blend --> create design time data.
            ////}
            ////else
            ////{
            ////    // Code runs "for real"
            ////}
        }

        public string this[string columnName]
        {
            get
            {
                string result = null;

                if (columnName == "IpAddress" && _ipAddress != null)
                {
                    if (!Common.CommonHelper.IpRegex.IsMatch(_ipAddress) || !Common.CommonHelper.HostNameRegex.IsMatch(_ipAddress))
                    {
                        result = "IP Field contained invalid IP address or hostname.";
                    }
                }

                if (columnName == "MaxBulkRept" && (_currertSnmpOperation == SnmpOperationType.GetBulk || _currertSnmpOperation == SnmpOperationType.WalkBulk))
                {
                    if (_maxBulkReps < 0 || _maxBulkReps > 20)
                    {
                        result = "Max Bulk Repetitions can't be less then 0.";
                    }
                }

                return result;
            }
        }

        public string Error
        {
            get { return null; }
        }
    }

    public class OidTreeViewModel : ViewModelBase
    {
        private static readonly List<Oid> _oids;
        private Oid _oidCurrent = new Oid();
        private bool _showMode;

        public bool ShowMode
        {
            get { return _showMode; }
            set { _showMode = value; }
        }

        public Oid OidSelected
        {
            get { return _oidCurrent; }
            set
            {
                var oid = value as Oid;
                if (oid != null)
                {
                    _oidCurrent = oid;
                    OidCurrent = oid.Value;
                    RaisePropertyChanged();
                }
            }
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

        static OidTreeViewModel()
        {
            _oids = SnmpServiceService.InitializeOids;
        }

        private void ChangeSelection(object context)
        {
            if (context is Oid)
            {
                _oidCurrent = (Oid)context;
            }
        }

        public List<Oid> Oids
        {
            get { return _oids; }
        }

        public OidTreeViewModel()
        {

        }
    }
}