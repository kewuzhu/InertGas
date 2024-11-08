using InertGas.Application.Model;
using System.Windows;

namespace InertGas.Application.Themes
{
    internal class ValveControlGroupBox : System.Windows.Controls.GroupBox
    {
        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register(nameof(ImageSource), typeof(object), typeof(ValveControlGroupBox));

        public static readonly DependencyProperty ValveNameProperty =
            DependencyProperty.Register(nameof(ValveName), typeof(string), typeof(ValveControlGroupBox));

        public static readonly DependencyProperty ValveNumberProperty =
            DependencyProperty.Register(nameof(ValveNumber), typeof(string), typeof(ValveControlGroupBox));

        public static readonly DependencyProperty TextColorProperty =
            DependencyProperty.Register(nameof(TextColor), typeof(System.Windows.Media.Brush), typeof(ValveControlGroupBox));

        public static readonly DependencyProperty ValveControlProperty =
                DependencyProperty.Register(nameof(ValveControl), typeof(ValveControl), typeof(ValveControlGroupBox));

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

        public ValveControl ValveControl
        {
            get => (ValveControl)GetValue(ValveControlProperty);
            set => SetValue(ValveControlProperty, value);
        }
    }
}