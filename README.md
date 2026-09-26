# Windows 硬件监控

一个基于 .NET 8 WPF 的 Windows 只读硬件监控工具。当前 MVP 每秒采集一次并展示可用的温度、负载、风扇转速、频率、电压、功耗、容量和吞吐量传感器。

## 当前功能

- CPU、GPU、主板、内存、存储、网络等硬件传感器枚举
- 温度、使用率、风扇、频率、电压、功耗等读数
- 传感器无读数时显示“不可用”，不使用假数据
- 后台自动刷新和手动刷新
- Windows 系统托盘：关闭窗口时隐藏，托盘菜单可恢复或退出
- 统一硬件数据接口，便于后续替换数据源

## 构建与运行

需要 Windows、.NET 8 SDK 和可访问 NuGet 的网络环境：

```powershell
dotnet restore HardwareMonitor.sln
dotnet build HardwareMonitor.sln --configuration Release
dotnet test HardwareMonitor.sln --configuration Release
dotnet run --project src/HardwareMonitor.App/HardwareMonitor.App.csproj
```

部分硬件传感器需要管理员权限、厂商驱动或特定主板支持。程序当前请求管理员权限运行，以便读取 Core Temp 高完整性进程创建的 Shared Memory；读取不到的传感器会保留为空状态。

## 项目结构

- `src/HardwareMonitor.Core`：模型、采集接口、刷新调度
- `src/HardwareMonitor.Infrastructure`：LibreHardwareMonitor 适配器
- `src/HardwareMonitor.App`：WPF 界面、MVVM 展示模型、系统托盘
- `tests/HardwareMonitor.Core.Tests`：核心逻辑测试

## 许可证与第三方依赖

本项目当前没有单独声明最终发行许可证。硬件采集使用 `LibreHardwareMonitorLib`，版本由 `src/HardwareMonitor.Infrastructure/HardwareMonitor.Infrastructure.csproj` 指定。LibreHardwareMonitor 使用 **MPL-2.0**，发布包含其代码或二进制时请保留相应版权和许可证声明，并核对最新上游条款：

<https://github.com/LibreHardwareMonitor/LibreHardwareMonitor>

## 后续计划

历史曲线、温度/使用率告警、开机启动、任务栏简报和 CSV 导出将在 MVP 验证稳定后加入。风扇控制、超频、远程监控和云同步不属于当前范围。

## Xeon/特殊 CPU 温度

部分 Haswell-EP Xeon（例如 E5-2690 v3）会被 LibreHardwareMonitor 识别出温度传感器名称，但返回空值。本程序已支持 Core Temp 官方 Shared Memory：启动 Core Temp 后，程序会在下一次刷新自动读取每核心温度并显示在 CPU 卡片中。Core Temp 下载：<https://www.alcpu.com/CoreTemp/>。
## UI 与交互

- 首屏核心摘要卡片：CPU、内存、GPU、磁盘、网络
- CPU 温度支持 Core Temp Shared Memory，特殊 Xeon 可正常显示
- 内存与网络数据自动合并，隐藏不适用指标
- 每个硬件卡片支持进入详情、按传感器类型筛选、搜索、复制和 CSV 导出
- 设置窗口支持刷新间隔、托盘行为、主题和 Core Temp 开关
- 托盘菜单支持打开、立即刷新、设置和退出
- 核心层提供 5 分钟历史缓冲和温度告警冷却服务，便于后续趋势图/通知扩展
