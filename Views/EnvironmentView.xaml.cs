using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using WinTools.Helpers;

namespace WinTools.Views;

public partial class EnvironmentView : Page
{
    public EnvironmentView()
    {
        InitializeComponent();
        LoadEnvironmentVariables();
    }

    private void LoadEnvironmentVariables()
    {
        try
        {
            var searchText = SearchBox?.Text?.ToLower() ?? "";
            
            var userVars = Environment.GetEnvironmentVariables(EnvironmentVariableTarget.User)
                .Cast<System.Collections.DictionaryEntry>()
                .Where(e => string.IsNullOrEmpty(searchText) || 
                           e.Key.ToString()!.ToLower().Contains(searchText))
                .Select(e => new EnvVarInfo
                {
                    Name = e.Key?.ToString() ?? "",
                    Value = e.Value?.ToString() ?? ""
                })
                .OrderBy(v => v.Name)
                .ToList();

            var systemVars = Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Machine)
                .Cast<System.Collections.DictionaryEntry>()
                .Where(e => string.IsNullOrEmpty(searchText) || 
                           e.Key.ToString()!.ToLower().Contains(searchText))
                .Select(e => new EnvVarInfo
                {
                    Name = e.Key?.ToString() ?? "",
                    Value = e.Value?.ToString() ?? ""
                })
                .OrderBy(v => v.Name)
                .ToList();

            UserEnvGrid.ItemsSource = userVars;
            SystemEnvGrid.ItemsSource = systemVars;
        }
        catch { }
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        LoadEnvironmentVariables();
    }

    private void RefreshBtn_Click(object sender, RoutedEventArgs e)
    {
        LoadEnvironmentVariables();
    }

    private void AddBtn_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new EnvVarDialog();
        if (dialog.ShowDialog() == true)
        {
            try
            {
                Environment.SetEnvironmentVariable(dialog.VarName, dialog.VarValue, EnvironmentVariableTarget.User);
                LoadEnvironmentVariables();
                MessageBoxWindow.Show("环境变量已添加", "成功", CustomMessageBoxType.Information);
            }
            catch (Exception ex)
            {
                MessageBoxWindow.Show($"无法添加环境变量: {ex.Message}", "错误", CustomMessageBoxType.Error);
            }
        }
    }

    private void EditBtn_Click(object sender, RoutedEventArgs e)
    {
        if (UserEnvGrid.SelectedItem is EnvVarInfo userVar)
        {
            var dialog = new EnvVarDialog(userVar.Name, userVar.Value);
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    Environment.SetEnvironmentVariable(userVar.Name, null, EnvironmentVariableTarget.User);
                    Environment.SetEnvironmentVariable(dialog.VarName, dialog.VarValue, EnvironmentVariableTarget.User);
                    LoadEnvironmentVariables();
                    MessageBoxWindow.Show("环境变量已编辑", "成功", CustomMessageBoxType.Information);
                }
                catch (Exception ex)
                {
                    MessageBoxWindow.Show($"无法编辑环境变量: {ex.Message}", "错误", CustomMessageBoxType.Error);
                }
            }
        }
        else if (SystemEnvGrid.SelectedItem is EnvVarInfo systemVar)
        {
            MessageBoxWindow.Show("系统变量需要管理员权限才能修改", "提示", CustomMessageBoxType.Warning);
        }
    }

    private void DeleteBtn_Click(object sender, RoutedEventArgs e)
    {
        if (UserEnvGrid.SelectedItem is EnvVarInfo userVar)
        {
            var result = MessageBoxWindow.Show($"确定要删除环境变量 {userVar.Name} 吗？", "确认", CustomMessageBoxType.Question, false);
            if (result == CustomMessageBoxResult.Yes)
            {
                try
                {
                    Environment.SetEnvironmentVariable(userVar.Name, null, EnvironmentVariableTarget.User);
                    LoadEnvironmentVariables();
                    MessageBoxWindow.Show("环境变量已删除", "成功", CustomMessageBoxType.Information);
                }
                catch (Exception ex)
                {
                    MessageBoxWindow.Show($"无法删除环境变量: {ex.Message}", "错误", CustomMessageBoxType.Error);
                }
            }
        }
        else if (SystemEnvGrid.SelectedItem is EnvVarInfo)
        {
            MessageBoxWindow.Show("系统变量需要管理员权限才能删除", "提示", CustomMessageBoxType.Warning);
        }
    }
}

public class EnvVarInfo
{
    public string Name { get; set; } = "";
    public string Value { get; set; } = "";
}