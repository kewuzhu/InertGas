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

        private async void OnSplashScreenLoaded(object sender, RoutedEventArgs e)
        {
            await LogUtils.ScanAndClearAppConfigFilesInAllDriversAsync(APP_CONFIG_FILE_NAME);
        }
    }
}
