using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using HardwareMonitor.App.ViewModels;
using HardwareMonitor.App.Views;
using HardwareMonitor.Core.Services;
using HardwareMonitor.Infrastructure;
using Application = System.Windows.Application;

namespace HardwareMonitor.App;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel = new();
    private readonly CancellationTokenSource _shutdown = new();
    private readonly NotifyIcon _trayIcon;
    private readonly List<HardwareMonitor.Core.Models.SensorReading> _latestReadings = [];
    private readonly MonitorRefreshService? _refreshService;
    private bool _exitRequested;

    public MainWindow()
    {
        InitializeComponent(); DataContext = _viewModel;
        try { _refreshService = new MonitorRefreshService(new LibreHardwareSensorProvider()); }
        catch (Exception ex) { _viewModel.Status = $"传感器初始化失败：{ex.Message}"; }
        _trayIcon = CreateTrayIcon(); Loaded += async (_, _) => await RefreshLoopAsync();
    }

    private NotifyIcon CreateTrayIcon()
    {
        var menu = new ContextMenuStrip();
        menu.Items.Add("打开窗口", null, (_, _) => RestoreFromTray());
        menu.Items.Add("立即刷新", null, async (_, _) => await RefreshOnceAsync());
        menu.Items.Add("设置", null, (_, _) => new SettingsWindow { Owner = this }.ShowDialog());
        menu.Items.Add("退出", null, (_, _) => ExitApplication());
        var icon = new NotifyIcon { Icon = SystemIcons.Application, Text = "硬件监控", Visible = true, ContextMenuStrip = menu };
        icon.DoubleClick += (_, _) => RestoreFromTray(); return icon;
    }

    private async Task RefreshLoopAsync()
    {
        if (_refreshService is null) return;
        try { await foreach (var snapshot in _refreshService.StreamAsync(TimeSpan.FromSeconds(1), _shutdown.Token)) await Dispatcher.InvokeAsync(() => { _latestReadings.Clear(); _latestReadings.AddRange(snapshot.Readings); _viewModel.Apply(snapshot); }); }
        catch (OperationCanceledException) when (_shutdown.IsCancellationRequested) { }
        catch (Exception ex) { await Dispatcher.InvokeAsync(() => _viewModel.Status = $"监控服务异常：{ex.Message}"); }
    }

    private async Task RefreshOnceAsync()
    {
        if (_refreshService is null) return;
        var snapshot = await Task.Run(() => _refreshService.ReadOnceAsync(_shutdown.Token), _shutdown.Token);
        _latestReadings.Clear(); _latestReadings.AddRange(snapshot.Readings); _viewModel.Apply(snapshot);
    }
    private async void Refresh_Click(object sender, RoutedEventArgs e) { _viewModel.IsRefreshing = true; try { await RefreshOnceAsync(); } catch (Exception ex) when (ex is not OperationCanceledException) { _viewModel.Status = $"手动刷新失败：{ex.Message}"; } finally { _viewModel.IsRefreshing = false; } }
    private void Settings_Click(object sender, RoutedEventArgs e) => new SettingsWindow { Owner = this }.ShowDialog();
    private void Detail_Click(object sender, RoutedEventArgs e) { if (sender is Button { Tag: string name }) new HardwareDetailWindow(name, _latestReadings.Where(x => x.HardwareName == name || (name == "内存" && x.HardwareName.Contains("Memory", StringComparison.OrdinalIgnoreCase)) || (name == "网络" && x.Kind is HardwareMonitor.Core.Models.SensorKind.Load or HardwareMonitor.Core.Models.SensorKind.Throughput))).ShowDialog(); }
    private void Window_Closing(object? sender, System.ComponentModel.CancelEventArgs e) { if (!_exitRequested) { e.Cancel = true; Hide(); _trayIcon.ShowBalloonTip(1200, "硬件监控", "已最小化到托盘。", ToolTipIcon.Info); } }
    private void RestoreFromTray() { Show(); WindowState = WindowState.Normal; Activate(); }
    private void Exit_Click(object sender, RoutedEventArgs e) => ExitApplication();
    private void ExitApplication() { _exitRequested = true; _shutdown.Cancel(); _trayIcon.Visible = false; _trayIcon.Dispose(); _refreshService?.DisposeAsync().AsTask().GetAwaiter().GetResult(); Application.Current.Shutdown(); }
}
