using System.Windows.Controls;
using SupermarketPos.Wpf.ViewModels;

namespace SupermarketPos.Wpf.Views;

public partial class LoginView : UserControl
{
    public LoginView()
    {
        InitializeComponent();
        // PasswordBox cannot be data-bound for security; push the value to the VM on change.
        PasswordBox.PasswordChanged += (_, _) =>
        {
            if (DataContext is LoginViewModel vm)
                vm.Password = PasswordBox.Password;
        };
    }
}
