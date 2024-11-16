using InertGas.Application.Model;
using System.Windows;

namespace InertGas.Application.Themes
{
    internal class HardwareControlGroupBox : System.Windows.Controls.GroupBox
    {
        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register(nameof(ImageSource), typeof(object), typeof(HardwareControlGroupBox));

        public static readonly DependencyProperty ValveNameProperty =
            DependencyProperty.Register(nameof(ValveName), typeof(string), typeof(HardwareControlGroupBox));

        public static readonly DependencyProperty ValveNumberProperty =
            DependencyProperty.Register(nameof(ValveNumber), typeof(string), typeof(HardwareControlGroupBox));

        public static readonly DependencyProperty TextColorProperty =
            DependencyProperty.Register(nameof(TextColor), typeof(System.Windows.Media.Brush), typeof(HardwareControlGroupBox));

        public static readonly DependencyProperty HardwareControlProperty =
                DependencyProperty.Register(nameof(HardwareControl), typeof(HardwareControl), typeof(HardwareControlGroupBox));

        public object ImageSource
        {
            get => (string)GetValue(ImageSourceProperty);
            set => SetValue(ImageSourceProperty, value);
        }

        public string ValveName
        {
            get => (string)GetValue(ValveNameProperty);
            set => SetValue(ValveNameProperty, value);
        }

        public string ValveNumber
        {
            get => (string)GetValue(ValveNumberProperty);
            set => SetValue(ValveNumberProperty, value);
        }

        public System.Windows.Media.Brush TextColor
        {
            get => (System.Windows.Media.Brush)GetValue(TextColorProperty);
            set => SetValue(TextColorProperty, value);
        }

        public HardwareControl HardwareControl
        {
            get => (HardwareControl)GetValue(HardwareControlProperty);
            set => SetValue(HardwareControlProperty, value);
        }
    }
}