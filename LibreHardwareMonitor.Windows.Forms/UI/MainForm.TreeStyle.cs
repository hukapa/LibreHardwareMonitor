using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Aga.Controls.Tree;
using Aga.Controls.Tree.NodeControls;
using LibreHardwareMonitor.Hardware;
using LibreHardwareMonitor.Windows.Forms.UI.Themes;

namespace LibreHardwareMonitor.Windows.Forms.UI;

/// <summary>
/// Fluent styling of the sensor tree: hover highlight, group typography, severity colored
/// values and inline progress bars for percentage based sensors.
/// </summary>
public sealed partial class MainForm
{
    private TreeNodeAdv _hoverNode;

    private void InitializeTreeStyle()
    {
        treeView.RowDraw += TreeView_RowDraw;
        treeView.MouseMove += TreeView_HoverMove;
        treeView.MouseLeave += TreeView_HoverLeave;
    }

    private void TreeView_HoverMove(object sender, MouseEventArgs e)
    {
        TreeNodeAdv node = treeView.GetNodeAt(e.Location);
        if (node == _hoverNode)
            return;

        _hoverNode = node;
        treeView.Invalidate();
    }

    private void TreeView_HoverLeave(object sender, EventArgs e)
    {
        if (_hoverNode == null)
            return;

        _hoverNode = null;
        treeView.Invalidate();
    }

    /// <summary>Applies typography and severity colors before the plot/hidden overrides run.</summary>
    private void ApplyTreeTextStyle(object sender, DrawEventArgs e)
    {
        Theme theme = Theme.Current;
        object tag = e.Node.Tag;

        if (sender == nodeTextBoxText)
        {
            if (tag is HardwareNode || tag == _root)
            {
                e.Font = FluentStyle.UiFontSemibold;
            }
            else if (tag is TypeNode)
            {
                e.Font = FluentStyle.UiFontSemibold;
                e.TextColor = theme.MutedForegroundColor;
            }

            return;
        }

        if (tag is not SensorNode sensorNode || sensorNode.Sensor == null)
            return;

        if (sender == nodeTextBoxValue)
        {
            SeverityLevel level = SensorSeverity.Classify(sensorNode.Sensor);
            e.TextColor = SensorSeverity.ColorFor(level, theme, e.TextColor);
        }
        else
        {
            e.TextColor = theme.MutedForegroundColor;
        }
    }

    private void TreeView_RowDraw(object sender, TreeViewRowDrawEventArgs e)
    {
        Theme theme = Theme.Current;
        Rectangle row = e.RowRect;

        if (e.Node == _hoverNode && !e.Node.IsSelected)
        {
            using Brush hover = new SolidBrush(theme.HoverBackgroundColor);
            e.Graphics.FillRectangle(hover, treeView.OffsetX, row.Y, treeView.ClientRectangle.Width, row.Height);
        }

        if (e.Node.Tag is SensorNode sensorNode && sensorNode.Sensor != null && treeView.Columns[1].IsVisible)
            DrawValueBar(e.Graphics, sensorNode.Sensor, row, theme);
    }

    private void DrawValueBar(Graphics g, ISensor sensor, Rectangle row, Theme theme)
    {
        float? ratio = SensorSeverity.Ratio(sensor.SensorType, sensor.Value);
        if (ratio == null)
            return;

        int x = treeView.Columns[0].IsVisible ? treeView.Columns[0].Width : 0;
        // Reserve room for the widest percentage text ("100,0 %") and place the bar after it.
        int textReserve = LogicalToDeviceUnits(58);
        int rightPadding = LogicalToDeviceUnits(12);
        int width = Math.Min(treeView.Columns[1].Width - textReserve - rightPadding, LogicalToDeviceUnits(120));
        if (width < LogicalToDeviceUnits(20))
            return;

        float height = Math.Max(3f, LogicalToDeviceUnits(4));
        RectangleF track = new(x + textReserve, row.Y + ((row.Height - height) / 2f), width, height);
        Color fill = SensorSeverity.ColorFor(SensorSeverity.Classify(sensor), theme, theme.AccentColor);

        SmoothingMode previous = g.SmoothingMode;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        FluentStyle.FillRounded(g, FluentStyle.Blend(theme.SurfaceColor, theme.ForegroundColor, 0.1), track, height / 2);
        if (ratio.Value > 0)
        {
            RectangleF value = new(track.X, track.Y, Math.Max(height, track.Width * ratio.Value), height);
            FluentStyle.FillRounded(g, fill, value, height / 2);
        }

        g.SmoothingMode = previous;
    }
}
