using InertGas.Common.Utility;
using System.Windows;
using static InertGas.Application.ApplicationConstants;

namespace InertGas.Application.UI
{
    /// <summary>
    /// SplashScreen.xaml 的交互逻辑
    /// </summary>
    public partial class SplashScreen : Window
    {
        public SplashScreen()
        {
            InitializeComponent();

            this.Loaded += OnSplashScreenLoaded;
        }

        private void OnSplashScreenLoaded(object sender, RoutedEventArgs e)
        {
            LogUtils.ScanAndClearAppConfigFilesInAllDrivers(CONFIG_DIRECTORY);
        }
    }
}
