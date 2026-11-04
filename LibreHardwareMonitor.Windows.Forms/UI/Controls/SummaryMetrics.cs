using System;
using System.Collections.Generic;
using System.Linq;
using LibreHardwareMonitor.Hardware;

namespace LibreHardwareMonitor.Windows.Forms.UI.Controls
{
    /// <summary>A headline metric shown as a card in the summary strip.</summary>
    public sealed class SummaryMetric
    {
        public SummaryMetric(string title, IHardware hardware, ISensor sensor, Func<string> detail = null)
        {
            Title = title;
            Hardware = hardware;
            Sensor = sensor;
            Detail = detail ?? (() => hardware.Name);
        }

        public string Title { get; }
        public IHardware Hardware { get; }
        public ISensor Sensor { get; }
        public Func<string> Detail { get; }
    }

    /// <summary>
    /// Picks the most representative sensors (CPU, GPU, memory, power) from the detected hardware,
    /// using well known sensor names with sensible fallbacks.
    /// </summary>
    public static class SummaryMetrics
    {
        private static readonly string[] CpuTemperatureNames = { "Core (Tctl/Tdie)", "CPU Package", "Core Max", "Core Average", "Tdie", "Tctl" };
        private static readonly string[] GpuTemperatureNames = { "GPU Core", "GPU Hot Spot" };

        public static IList<SummaryMetric> Resolve(IEnumerable<IHardware> hardware)
        {
            List<IHardware> all = hardware.ToList();
            List<SummaryMetric> metrics = new();

            IHardware cpu = all.FirstOrDefault(h => h.HardwareType == HardwareType.Cpu);
            if (cpu != null)
            {
                Add(metrics, "CPU Load", cpu, Find(cpu, SensorType.Load, "CPU Total"));
                Add(metrics, "CPU Temperature", cpu, Find(cpu, SensorType.Temperature, CpuTemperatureNames));
            }

            IHardware gpu = all.FirstOrDefault(h => h.HardwareType is HardwareType.GpuNvidia or HardwareType.GpuAmd)
                            ?? all.FirstOrDefault(h => h.HardwareType == HardwareType.GpuIntel);
            if (gpu != null)
                Add(metrics, "GPU Load", gpu, Find(gpu, SensorType.Load, "GPU Core", "D3D 3D"));

            IHardware memory = all.FirstOrDefault(h => h.HardwareType == HardwareType.Memory && h.Name.IndexOf("Virtual", StringComparison.OrdinalIgnoreCase) < 0)
                               ?? all.FirstOrDefault(h => h.HardwareType == HardwareType.Memory);
            if (memory != null)
            {
                ISensor used = FindExact(memory, SensorType.Data, "Memory Used");
                ISensor available = FindExact(memory, SensorType.Data, "Memory Available");
                Add(metrics, "Memory", memory, Find(memory, SensorType.Load, "Memory"), () => MemoryDetail(memory, used, available));
            }

            // Ordered by priority: the strip drops trailing cards when the window is narrow.
            if (gpu != null)
                Add(metrics, "GPU Temperature", gpu, Find(gpu, SensorType.Temperature, GpuTemperatureNames));

            if (cpu != null)
                Add(metrics, "CPU Power", cpu, Find(cpu, SensorType.Power, "CPU Package", "Package"));

            return metrics;
        }

        private static void Add(List<SummaryMetric> metrics, string title, IHardware hardware, ISensor sensor, Func<string> detail = null)
        {
            if (sensor != null)
                metrics.Add(new SummaryMetric(title, hardware, sensor, detail));
        }

        private static ISensor FindExact(IHardware hardware, SensorType type, string name)
        {
            return hardware.Sensors.FirstOrDefault(s => s.SensorType == type && string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>Finds a sensor by preferred names, falling back to the first sensor of the type.</summary>
        private static ISensor Find(IHardware hardware, SensorType type, params string[] names)
        {
            foreach (string name in names)
            {
                ISensor sensor = FindExact(hardware, type, name);
                if (sensor != null)
                    return sensor;
            }

            return hardware.Sensors.FirstOrDefault(s => s.SensorType == type);
        }

        private static string MemoryDetail(IHardware memory, ISensor used, ISensor available)
        {
            // Data sensors report bytes; reuse the tree's scaled formatting (KB, MB, GB...).
            if (used?.Value is float u && available?.Value is float a)
                return UnitManager.BytesToString(u) + " / " + UnitManager.BytesToString(u + a);

            return memory.Name;
        }
    }
}
