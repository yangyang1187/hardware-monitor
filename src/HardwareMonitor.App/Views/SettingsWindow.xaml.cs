using System.Windows.Controls;
using System.Windows;
using HardwareMonitor.Core.Models;
using HardwareMonitor.Infrastructure;

namespace HardwareMonitor.App.Views;

public partial class SettingsWindow : Window
{
    private readonly SettingsStore _store = new();
    private readonly MonitorSettings _settings;
    public SettingsWindow()
    {
        InitializeComponent(); _settings = _store.Load(); IntervalBox.SelectedValue = _settings.RefreshIntervalSeconds.ToString(); TrayBox.IsChecked = _settings.MinimizeOnClose; CoreTempBox.IsChecked = _settings.UseCoreTempSharedMemory; ThemeBox.SelectedIndex = _settings.Theme switch { "Light" => 1, "Dark" => 2, _ => 0 };
    }
    private void Cancel_Click(object sender, RoutedEventArgs e) => Close();
    private void Save_Click(object sender, RoutedEventArgs e) { _settings.RefreshIntervalSeconds = double.TryParse(((ComboBoxItem)IntervalBox.SelectedItem).Content.ToString(), out var i) ? i : 1; _settings.MinimizeOnClose = TrayBox.IsChecked == true; _settings.UseCoreTempSharedMemory = CoreTempBox.IsChecked == true; _settings.Theme = ThemeBox.SelectedIndex switch { 1 => "Light", 2 => "Dark", _ => "System" }; _store.Save(_settings); DialogResult = true; }
}
