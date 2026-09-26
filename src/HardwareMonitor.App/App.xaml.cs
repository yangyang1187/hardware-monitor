using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace HardwareMonitor.App;

public partial class App : System.Windows.Application
{
    private static readonly string LogPath = Path.Combine(Path.GetTempPath(), "HardwareMonitor.App.log");

    public static void Log(string message)
    {
        try { File.AppendAllText(LogPath, $"{DateTime.Now:O} {message}{Environment.NewLine}"); } catch { }
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        Log("OnStartup");
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        Exit += (_, _) => Log("Application.Exit");
        base.OnStartup(e);
        var window = new MainWindow();
        MainWindow = window;
        window.Show();
        Log("Window.Show completed");
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Log($"Unhandled: {e.Exception}");
        e.Handled = true;
    }
}
