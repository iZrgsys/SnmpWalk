using System.Windows;

namespace SnmpWalk.ClientWindow
{
    public class SnmpWalkWindow : Window
    {
        protected void MinimizeClick(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        protected void RestoreClick(object sender, RoutedEventArgs e)
        {
            WindowState = (WindowState == WindowState.Normal) ? WindowState.Maximized : WindowState.Normal;
        }

        protected void CloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        static SnmpWalkWindow() 
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SnmpWalkWindow), new FrameworkPropertyMetadata(typeof(SnmpWalkWindow)));
        }
    }
}
