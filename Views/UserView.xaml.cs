using System.Diagnostics;
using System.Management;
using System.Windows;
using System.Windows.Controls;
using WinTools.Helpers;

namespace WinTools.Views;

public partial class UserView : Page
{
    public UserView()
    {
        InitializeComponent();
        LoadUsers();
    }

    private void LoadUsers()
    {
        try
        {
            var users = new List<UserInfo>();
            
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_UserAccount WHERE LocalAccount = TRUE");
                foreach (ManagementObject user in searcher.Get())
                {
                    var name = user["Name"]?.ToString() ?? "";
                    if (!string.IsNullOrEmpty(name))
                    {
                        users.Add(new UserInfo
                        {
                            Name = name,
                            FullName = user["FullName"]?.ToString() ?? "",
                            Status = user["Status"]?.ToString() ?? "未知",
                            UserType = user["LocalAccount"]?.ToString() == "True" ? "本地账户" : "域账户",
                            LastLogon = "未知"
                        });
                    }
                }
            }
            catch { }
            
            if (users.Count == 0)
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "net.exe",
                        Arguments = "user",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                
                var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines.Skip(3))
                {
                    var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length > 0 && !parts[0].StartsWith("---"))
                    {
                        var userName = parts[0].Trim();
                        if (!string.IsNullOrEmpty(userName))
                        {
                            users.Add(new UserInfo
                            {
                                Name = userName,
                                FullName = "",
                                Status = "OK",
                                UserType = "本地账户",
                                LastLogon = "未知"
                            });
                        }
                    }
                }
            }
            
            UserGrid.ItemsSource = users.OrderBy(u => u.Name).ToList();
        }
        catch { }
    }

    private void RefreshBtn_Click(object sender, RoutedEventArgs e)
    {
        LoadUsers();
    }

    private void AddBtn_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new UserDialog();
        if (dialog.ShowDialog() == true)
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "net.exe",
                        Arguments = $"user {dialog.UserName} {dialog.Password} /add",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                process.WaitForExit();
                
                if (!string.IsNullOrEmpty(dialog.FullName))
                {
                    var process2 = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "wmic.exe",
                            Arguments = $"useraccount where \"Name='{dialog.UserName}'\" set FullName=\"{dialog.FullName}\"",
                            UseShellExecute = false,
                            CreateNoWindow = true
                        }
                    };
                    process2.Start();
                    process2.WaitForExit();
                }

                LoadUsers();
                MessageBoxWindow.Show("用户已创建", "成功", CustomMessageBoxType.Information);
            }
            catch (Exception ex)
            {
                MessageBoxWindow.Show($"无法创建用户: {ex.Message}", "错误", CustomMessageBoxType.Error);
            }
        }
    }

    private void DeleteBtn_Click(object sender, RoutedEventArgs e)
    {
        if (UserGrid.SelectedItem is UserInfo user)
        {
            var result = MessageBoxWindow.Show($"确定要删除用户 {user.Name} 吗？", "确认", CustomMessageBoxType.Question, false);
            if (result == CustomMessageBoxResult.Yes)
            {
                try
                {
                    var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "net.exe",
                            Arguments = $"user {user.Name} /delete",
                            UseShellExecute = false,
                            CreateNoWindow = true
                        }
                    };
                    process.Start();
                    process.WaitForExit();
                    
                    LoadUsers();
                    MessageBoxWindow.Show("用户已删除", "成功", CustomMessageBoxType.Information);
                }
                catch (Exception ex)
                {
                    MessageBoxWindow.Show($"无法删除用户: {ex.Message}", "错误", CustomMessageBoxType.Error);
                }
            }
        }
    }

    private void PasswordBtn_Click(object sender, RoutedEventArgs e)
    {
        if (UserGrid.SelectedItem is UserInfo user)
        {
            var dialog = new PasswordDialog();
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "net.exe",
                            Arguments = $"user {user.Name} {dialog.NewPassword}",
                            UseShellExecute = false,
                            CreateNoWindow = true
                        }
                    };
                    process.Start();
                    process.WaitForExit();
                    
                    MessageBoxWindow.Show("密码已修改", "成功", CustomMessageBoxType.Information);
                }
                catch (Exception ex)
                {
                    MessageBoxWindow.Show($"无法修改密码: {ex.Message}", "错误", CustomMessageBoxType.Error);
                }
            }
        }
    }
}

public class UserInfo
{
    public string Name { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Status { get; set; } = "";
    public string UserType { get; set; } = "";
    public string LastLogon { get; set; } = "";
}