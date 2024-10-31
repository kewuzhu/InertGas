using InertGas.Common.UI;
using System.ComponentModel;
using System.Windows;

namespace InertGas.Application.UI.Dialog
{
    /// <summary>
    /// FitGaussianDialog.xaml 的交互逻辑
    /// </summary>
    public partial class AddUserDialog : DialogBase
    {
        public AddUserDialog()
        {
            InitializeComponent();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }
    }
}
