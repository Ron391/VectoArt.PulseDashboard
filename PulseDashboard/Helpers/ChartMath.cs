using System.Globalization;
using System.Text;

namespace PulseDashboard.Helpers;

/// <summary>
/// Turns a list of numbers into SVG coordinates. Two Razor components share it (Sparkline + LineChart),
/// so "how do I draw a chart without a chart library?" lives in exactly one place.
/// </summary>
public static class ChartMath
{
    /// <summary>
    /// ALWAYS format numbers with InvariantCulture before they go into SVG.
    /// On a PC set to Danish, (1.5).ToString() gives "1,5" and the polyline silently breaks.
    /// </summary>
    public static string Num(double value) => value.ToString("0.##", CultureInfo.InvariantCulture);

    public static string ToPoints(IReadOnlyList<double> values, double width, double height, double padding = 2)
    {
        if (values.Count < 2) return string.Empty;

        var min = values.Min();
        var max = values.Max();
        var range = max - min;
        if (range < 1e-9) range = 1; // flat line: avoid dividing by zero

        var stepX = width / (values.Count - 1);
        var sb = new StringBuilder();
        for (var i = 0; i < values.Count; i++)
        {
            var x = i * stepX;
            // SVG's Y axis points DOWN, so we flip: high value = small y.
            var y = padding + (height - 2 * padding) * (1 - (values[i] - min) / range);
            sb.Append(Num(x)).Append(',').Append(Num(y)).Append(' ');
        }
        return sb.ToString().TrimEnd();
    }

    /// <summary>Same line, closed along the bottom, so it can be filled with a gradient.</summary>
    public static string ToAreaPoints(IReadOnlyList<double> values, double width, double height, double padding = 2)
    {
        var line = ToPoints(values, width, height, padding);
        return line.Length == 0 ? string.Empty : $"0,{Num(height)} {line} {Num(width)},{Num(height)}";
    }
}
