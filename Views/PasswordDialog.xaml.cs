using System.Windows;
using Wpf.Ui.Controls;
using WinTools.Helpers;

namespace WinTools.Views;

public partial class PasswordDialog : FluentWindow
{
    public string NewPassword => PasswordBox.Password;

    public PasswordDialog()
    {
        InitializeComponent();
    }

    private void OK_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(PasswordBox.Password))
        {
            MessageBoxWindow.Show("密码不能为空", "错误", CustomMessageBoxType.Warning);
            return;
        }
        if (PasswordBox.Password != ConfirmPasswordBox.Password)
        {
            MessageBoxWindow.Show("两次输入的密码不一致", "错误", CustomMessageBoxType.Warning);
            return;
        }
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}