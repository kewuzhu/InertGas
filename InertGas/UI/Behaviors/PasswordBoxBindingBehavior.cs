using Microsoft.Xaml.Behaviors;
using System.Reflection;
using System.Security;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace InertGas.Application.UI.Behaviors
{
    // https://stackoverflow.com/questions/42315356/how-to-clear-wpf-passwordbox-bound-to-viewmodel-via-behavior
    internal class PasswordBoxBindingBehavior : Behavior<PasswordBox>
    {
        protected override void OnAttached()
        {
            AssociatedObject.PasswordChanged += OnPasswordBoxValueChanged;
        }

        public SecureString Password
        {
            get { return (SecureString)GetValue(PasswordProperty); }
            set { SetValue(PasswordProperty, value); }
        }

        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.Register("Password", typeof(SecureString), typeof(PasswordBoxBindingBehavior), new PropertyMetadata(OnSourcePropertyChanged));

        private static void OnSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null)
            {
                PasswordBoxBindingBehavior behavior = d as PasswordBoxBindingBehavior;
                behavior.AssociatedObject.PasswordChanged -= OnPasswordBoxValueChanged;
                behavior.AssociatedObject.Password = string.Empty;
                behavior.AssociatedObject.PasswordChanged += OnPasswordBoxValueChanged;
            }
        }

        private static void OnPasswordBoxValueChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox passwordBox = sender as PasswordBox;
            var behavior = Interaction.GetBehaviors(passwordBox).OfType<PasswordBoxBindingBehavior>().FirstOrDefault();
            if (behavior != null)
            {
                var binding = BindingOperations.GetBindingExpression(behavior, PasswordProperty);
                if (binding != null)
                {
                    PropertyInfo property = binding.DataItem.GetType().GetProperty(binding.ParentBinding.Path.Path);
                    if (property != null)
                        property.SetValue(binding.DataItem, passwordBox.SecurePassword, null);
                }
            }
        }
    }
}
