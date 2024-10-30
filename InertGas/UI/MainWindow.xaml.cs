using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace InertGas.Application.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void HandlePasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext != null)
            {
                ((dynamic)DataContext).SecurePassword = ((PasswordBox)sender).SecurePassword;
            }
        }

        private void OnAnimationCompleted(object sender, EventArgs e)
        {
            var dataContext = DataContext as MainWindowViewModel;

            dataContext.IsPageSwitchPlaying = false;
        }
    }
}