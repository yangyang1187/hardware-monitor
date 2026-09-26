using System.IO;
using System.Windows.Controls;
using WpfClipboard = System.Windows.Clipboard;
using CsvDialog = Microsoft.Win32.SaveFileDialog;
using System.Text;
using System.Windows;
using HardwareMonitor.App.ViewModels;
using Microsoft.Win32;

namespace HardwareMonitor.App.Views;

public partial class HardwareDetailWindow : Window
{
    private readonly HardwareDetailViewModel _vm;
    public HardwareDetailWindow(string name, IEnumerable<HardwareMonitor.Core.Models.SensorReading> readings)
    {
        InitializeComponent(); _vm = new(name, readings); DataContext = _vm; _vm.Refresh();
    }
    private void FilterChanged(object sender, RoutedEventArgs e) { if (IsLoaded) { _vm.Filter = ((ComboBoxItem)FilterBox.SelectedItem).Content.ToString() ?? "全部"; _vm.Search = SearchBox.Text; _vm.Refresh(); } }
    private void Copy_Click(object sender, RoutedEventArgs e) { WpfClipboard.SetText(string.Join(Environment.NewLine, _vm.Readings.Select(x => $"{x.HardwareName},{x.SensorName},{x.DisplayValue}"))); }
    private void Export_Click(object sender, RoutedEventArgs e) { var d = new CsvDialog { Filter = "CSV 文件|*.csv", FileName = "sensors.csv" }; if (d.ShowDialog() == true) File.WriteAllText(d.FileName, "硬件,传感器,类型,读数,状态\n" + string.Join("\n", _vm.Readings.Select(x => $"{x.HardwareName},{x.SensorName},{x.KindName},{x.DisplayValue},{x.Status}")), Encoding.UTF8); }
}
