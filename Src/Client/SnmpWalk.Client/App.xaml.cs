using System;
using System.Windows;
using SnmpWalk.Client.ViewModels;
using SnmpWalk.Client.Views;

namespace SnmpWalk.Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                var mainViewModel = new MainViewModel();

                var mainWindow = new MainWindow(mainViewModel);
                mainWindow.ShowDialog();
            }
            catch (Exception)
            {
                
                
            }

        }
    }
}
