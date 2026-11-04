using System.Drawing;
using LibreHardwareMonitor.Hardware;
using LibreHardwareMonitor.Windows.Forms.UI.Themes;

namespace LibreHardwareMonitor.Windows.Forms.UI;

public enum SeverityLevel
{
    None,
    Normal,
    Warm,
    Hot
}

/// <summary>
/// Classifies sensor readings into visual severity levels and normalized fill ratios,
/// shared by the sensor tree and the summary cards.
/// </summary>
public static class SensorSeverity
{
    /// <summary>Returns the severity of a raw sensor value (temperatures are always in Celsius).</summary>
    public static SeverityLevel Classify(SensorType type, float? value)
    {
        if (!value.HasValue || float.IsNaN(value.Value))
            return SeverityLevel.None;

        float v = value.Value;
        return type switch
        {
            SensorType.Temperature => v >= 85 ? SeverityLevel.Hot : v >= 70 ? SeverityLevel.Warm : SeverityLevel.Normal,
            SensorType.Load => v >= 90 ? SeverityLevel.Hot : v >= 75 ? SeverityLevel.Warm : SeverityLevel.Normal,
            _ => SeverityLevel.None
        };
    }

    public static SeverityLevel Classify(ISensor sensor)
    {
        return sensor == null ? SeverityLevel.None : Classify(sensor.SensorType, sensor.Value);
    }

    /// <summary>Returns a 0..1 fill ratio for sensors with a natural percentage scale, or null.</summary>
    public static float? Ratio(SensorType type, float? value)
    {
        if (!value.HasValue || float.IsNaN(value.Value))
            return null;

        float ratio = type switch
        {
            SensorType.Load or SensorType.Level or SensorType.Control or SensorType.Humidity => value.Value / 100f,
            SensorType.Temperature => value.Value / 100f,
            _ => float.NaN
        };

        if (float.IsNaN(ratio))
            return null;

        return ratio < 0 ? 0 : ratio > 1 ? 1 : ratio;
    }

    public static Color ColorFor(SeverityLevel level, Theme theme, Color normal)
    {
        return level switch
        {
            SeverityLevel.Warm => theme.StateWarmColor,
            SeverityLevel.Hot => theme.StateHotColor,
            _ => normal
        };
    }
}
