using SnmpWalk.Client.ViewModels;

namespace SnmpWalk.Client.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private MainViewModel _mainViewModel;

        public MainWindow()
        {
            InitializeComponent();
        }

        public MainWindow(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            InitializeComponent();
        }
    }
}
