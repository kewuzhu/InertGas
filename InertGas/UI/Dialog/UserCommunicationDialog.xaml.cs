using InertGas.Common.UI;
using System.Windows;

namespace InertGas.Application.UI.Dialog
{
    public partial class UserCommunicationDialog : DialogBase
    {
        public UserCommunicationDialog()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            AffirmativeButton.Focus();
        }
    }
}
