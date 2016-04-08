using GalaSoft.MvvmLight;
using SnmpWalk.Client.Assets;
using SnmpWalk.Client.Assets.Enums;
using SnmpWalk.Common.DataModel.Snmp;
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
        private SnmpOperationType _currertEnumerationMemberSnmpOperation = SnmpOperationType.Get;
        private SnmpVersion _currentSnmpVersion = SnmpVersion.V1;
        private readonly OidTreeViewModel _oidTreeViewModel;
        private string _ipAddress;
        private bool _isSelected;
        private bool _isExpanded;

        public RelayCommand IfDeviceAvaliableCommand { get; private set; }
        public RelayCommand PerformActionCommand { get; private set; }

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

        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                _isExpanded = value;
                RaisePropertyChanged();
            }
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                RaisePropertyChanged();
            }
        }

        public object SelectedOid
        {
            get { return _oidTreeViewModel.OidSelected; }
            set
            {
                var oid = value as Oid;
                if (oid != null)
                {
                    _oidTreeViewModel.OidSelected = oid;
                    RaisePropertyChanged();
                }
            }
        }

        public object CurrertEnumerationMemberSnmpOperation
        {
            get { return _currertEnumerationMemberSnmpOperation; }
            set
            {
                var val = (EnumerationExtension.EnumerationMember)value;
                if (val != null)
                {
                    _currertEnumerationMemberSnmpOperation = (SnmpOperationType)val.Value;
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
            return !string.IsNullOrEmpty(IpAddress);
        }

        public void CheckDevice()
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            _oidTreeViewModel = new OidTreeViewModel();
            IfDeviceAvaliableCommand = new RelayCommand(CheckDevice, CanCheckDevice);

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