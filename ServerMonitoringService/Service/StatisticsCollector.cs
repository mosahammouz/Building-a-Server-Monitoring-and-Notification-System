using System.Diagnostics;
using ServerMonitoringService.Models;

namespace ServerMonitoringService.Service;

public class StatisticsCollector
{
    private readonly Process _process;
    public StatisticsCollector()
    {
        _process = Process.GetCurrentProcess();
    }

    public ServerStatistics Collect()
    {
        return new ServerStatistics
        {
            MemoryUsage = GetMemoryUsage(),
            AvailableMemory = GetAvailableMemory(),
            CpuUsage = GetCpuUsage(),
            Timestamp = DateTime.UtcNow
        };
    }
    
    private double GetMemoryUsage()
    {
        return _process.WorkingSet64 / (1024.0 * 1024.0);
    }

    private double GetAvailableMemory()
    {
        var memoryInfo = new ProcessStartInfo
        {
            FileName = "free",
            Arguments = "-m",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(memoryInfo);

        if (process == null)
            return 0;

        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        var memoryLine = output
            .Split('\n')
            .FirstOrDefault(line => line.TrimStart().StartsWith("Mem:"));

        if (memoryLine == null)
            return 0;

        var parts = memoryLine
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);

        // free -m output:
        // Mem: total used free shared buff/cache available
        return double.Parse(parts[6]);
    }

    private double GetCpuUsage()
    {
        var startCpuTime = _process.TotalProcessorTime;
        var startTime = DateTime.UtcNow;

        Thread.Sleep(500);

        var endCpuTime = _process.TotalProcessorTime;
        var endTime = DateTime.UtcNow;

        var cpuUsedMilliseconds =
            (endCpuTime - startCpuTime).TotalMilliseconds;

        var elapsedMilliseconds =
            (endTime - startTime).TotalMilliseconds;

        var cpuUsage =
            cpuUsedMilliseconds /
            (elapsedMilliseconds * Environment.ProcessorCount) * 100;

        return Math.Round(cpuUsage, 2);
    }
}