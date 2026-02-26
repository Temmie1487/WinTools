using System.Windows;
using Wpf.Ui.Controls;
using WinTools.Helpers;

namespace WinTools.Views;

public partial class UserDialog : FluentWindow
{
    public string UserName => NameBox.Text;
    public string FullName => FullNameBox.Text;
    public string Password => PasswordBox.Password;

    public UserDialog()
    {
        InitializeComponent();
    }

    private void OK_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameBox.Text))
        {
            MessageBoxWindow.Show("用户名不能为空", "错误", CustomMessageBoxType.Warning);
            return;
        }
        if (string.IsNullOrWhiteSpace(PasswordBox.Password))
        {
            MessageBoxWindow.Show("密码不能为空", "错误", CustomMessageBoxType.Warning);
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