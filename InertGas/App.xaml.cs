using InertGas.Application.Model;
using InertGas.Application.Themes;
using InertGas.Application.UI.Dialog;
using InertGas.Application.UI;
using InertGas.Application.Utility;
using InertGas.Common.Utility;
using System.Configuration;
using System.Data;
using System.Text.Json;
using System.Windows;

namespace InertGas
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                base.OnStartup(e);

                mainWindowViewModel_ = new MainWindowViewModel();
                MainWindow = new MainWindow { DataContext = mainWindowViewModel_ };

                MainWindow.Show();
            }
            catch (Exception ex)
            {
                UserCommunication.ShowMessage($"{Theme.GetString(Strings.Error)}", $"Message:{ex.Message}\nStackTrace:{ex.StackTrace}", MessageType.Critical);
            }
        }

        private MainWindowViewModel mainWindowViewModel_;
    }
}