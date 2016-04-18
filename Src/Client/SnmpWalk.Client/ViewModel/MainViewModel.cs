using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using SnmpWalk.Client.Assets;
using SnmpWalk.Client.Assets.Enums;
using SnmpWalk.Common.DataModel.Enums;
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
    public class MainViewModel : ViewModelBase
    {
        private SnmpOperationType _currertSnmpOperation = SnmpOperationType.Get;
        private SnmpVersion _currentSnmpVersion = SnmpVersion.V1;
        private readonly OidTreeViewModel _oidTreeViewModel;
        private readonly ISnmpEngine _snmpEngine;
        private string _results;
        private List<SnmpResult> _snmpResults;  
        private string _ipAddress;
        private bool _isSelected;
        private bool _isExpanded;
        private bool _canStart = false;

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

        public bool CanStart
        {
            get { return _canStart; }

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
            return !string.IsNullOrEmpty(IpAddress);
        }

        public void CheckDevice()
        {

        }

        public bool CanPerformAction()
        {
            return true;
        }

        public void PerformAction()
        {
            if (_currertSnmpOperation == SnmpOperationType.Walk)
            {
                //ThreadPool.QueueUserWorkItem(Walk);
                _snmpResults = _snmpEngine.WalkOperation(Common.DataModel.Snmp.SnmpVersion.V1, IPAddress.Parse(_ipAddress), null, _oidTreeViewModel.OidSelected, WalkingMode.WithinSubtree).ToList();
                var sb = new StringBuilder();

                Parallel.ForEach(_snmpResults, result =>
                {
                    sb.AppendLine(result.ToString());
                });

                Result = sb.ToString();
            }
        }

        private void Walk(object state)
        {
            _snmpResults = _snmpEngine.WalkOperation(Common.DataModel.Snmp.SnmpVersion.V1, IPAddress.Parse(_ipAddress), null, _oidTreeViewModel.OidSelected, WalkingMode.WithinSubtree).ToList();
        }

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            _oidTreeViewModel = new OidTreeViewModel();
            IfDeviceAvaliableCommand = new RelayCommand(CheckDevice, CanCheckDevice);
            PerformActionCommand = new RelayCommand(PerformAction, CanPerformAction);
            _snmpResults = new List<SnmpResult>();
            _snmpEngine = new SnmpEngineService();
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