using System.Windows;
using GalaSoft.MvvmLight.Messaging;

namespace SnmpWalk.Client.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Messenger.Default.Register<NotificationMessage>(this, NotificationMessageReceived);
        }


        private void NotificationMessageReceived(NotificationMessage msg)
        {
            if (msg.Notification == "ShowDeviceManagementWindow")
            {
                var managentWindow = new DeviceManagentWindow {Owner = this};
                managentWindow.ShowDialog();
            }
        }
    }
}
