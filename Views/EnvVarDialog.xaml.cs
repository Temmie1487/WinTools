using System.Windows;
using Wpf.Ui.Controls;
using WinTools.Helpers;

namespace WinTools.Views;

public partial class EnvVarDialog : FluentWindow
{
    public string VarName => NameBox.Text;
    public string VarValue => ValueBox.Text;

    public EnvVarDialog(string name = "", string value = "")
    {
        InitializeComponent();
        NameBox.Text = name;
        ValueBox.Text = value;
    }

    private void OK_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameBox.Text))
        {
            MessageBoxWindow.Show("变量名不能为空", "错误", CustomMessageBoxType.Warning);
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