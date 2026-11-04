using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Aga.Controls.Tree;
using LibreHardwareMonitor.Hardware;
using LibreHardwareMonitor.Windows.Forms.UI.Controls;
using LibreHardwareMonitor.Windows.Forms.UI.Icons;

namespace LibreHardwareMonitor.Windows.Forms.UI;

/// <summary>
/// Fluent shell layout: hardware navigation sidebar, KPI summary strip and the sensor tree
/// hosted in a rounded card. Selecting a device scopes the tree to that hardware.
/// </summary>
public sealed partial class MainForm
{
    private const string ScopeSettingKey = "navigation.scope";
    private const int SidebarLogicalWidth = 232;
    private const int SidebarCompactLogicalWidth = 56;
    private const int CompactBreakpointLogical = 860;
    private const int SummaryResolveTicks = 5;
    private const int SearchLogicalWidth = 300;

    private SidebarPanel _sidebar;
    private SummaryPanel _summary;
    private Panel _content;
    private CardPanel _treeCard;
    private SearchBox _search;
    private int _summaryResolveCountdown = SummaryResolveTicks;

    private void InitializeShell()
    {
        SuspendLayout();

        Controls.Remove(splitContainer);
        _treeCard = new CardPanel { Dock = DockStyle.Fill };
        _treeCard.Controls.Add(splitContainer);

        _summary = new SummaryPanel { Dock = DockStyle.Top, Visible = false };
        _summary.UseFahrenheit = () => _unitManager.TemperatureUnit == TemperatureUnit.Fahrenheit;
        _summary.CardClicked += Summary_CardClicked;

        int gutter = LogicalToDeviceUnits(12);
        _search = new SearchBox { Dock = DockStyle.Left, Width = LogicalToDeviceUnits(SearchLogicalWidth) };
        _search.SearchChanged += Search_SearchChanged;
        Panel toolbar = new() { Dock = DockStyle.Top, Height = _search.Height + gutter, Padding = new Padding(0, 0, 0, gutter) };
        toolbar.Controls.Add(_search);

        // Docking runs from the back of the z-order: summary, then toolbar, then the tree card.
        _content = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, LogicalToDeviceUnits(6), gutter, gutter) };
        _content.Controls.Add(_treeCard);
        _content.Controls.Add(toolbar);
        _content.Controls.Add(_summary);

        _sidebar = new SidebarPanel { Dock = DockStyle.Left, Width = LogicalToDeviceUnits(SidebarLogicalWidth) };
        _sidebar.SelectionChanged += Sidebar_SelectionChanged;

        // Docking runs from the back of the z-order: menu first, then sidebar, then content.
        Controls.Add(_content);
        Controls.Add(_sidebar);
        _content.BringToFront();
        mainMenu.SendToBack();

        Resize += delegate { UpdateShellDensity(); };
        UpdateShellDensity();
        ResumeLayout(true);
    }

    private void UpdateShellDensity()
    {
        if (_sidebar == null)
            return;

        bool compact = ClientSize.Width < LogicalToDeviceUnits(CompactBreakpointLogical);
        _sidebar.Compact = compact;
        _sidebar.Width = LogicalToDeviceUnits(compact ? SidebarCompactLogicalWidth : SidebarLogicalWidth);
    }

    /// <summary>Rebuilds the device list and summary cards after hardware was added or removed.</summary>
    private void RefreshShell()
    {
        if (_sidebar == null)
            return;

        HardwareNode scope = TreeScope;
        if (scope != null && !_root.Nodes.Contains(scope))
            scope = null;

        string savedScope = _settings.GetValue(ScopeSettingKey, string.Empty);
        List<SidebarItem> items = new() { new SidebarItem("All devices", FluentIcons.Computer, null) };
        foreach (HardwareNode node in _root.Nodes.OfType<HardwareNode>())
        {
            items.Add(new SidebarItem(node.Text, node.Image, node));
            if (scope == null && savedScope.Length > 0 && node.Hardware.Identifier.ToString() == savedScope)
                scope = node;
        }

        _sidebar.SetItems(items, scope);
        ApplyScope(scope, false);
        ResolveSummary();
    }

    private void ResolveSummary()
    {
        _summary.SetMetrics(SummaryMetrics.Resolve(_computer.Hardware));
        _summary.Visible = _summary.HasMetrics;
    }

    /// <summary>Sensors may appear a few update cycles after the hardware; re-resolve briefly after startup.</summary>
    private void UpdateShell()
    {
        if (_summary == null)
            return;

        if (_summaryResolveCountdown > 0 && --_summaryResolveCountdown % 2 == 0)
            ResolveSummary();

        _summary.Invalidate();
    }

    private HardwareNode TreeScope => (treeView.Model as TreeModel)?.Scope as HardwareNode;

    private void Sidebar_SelectionChanged(object sender, EventArgs e)
    {
        ApplyScope(_sidebar.SelectedItem?.Tag as HardwareNode, true);
    }

    private void Summary_CardClicked(object sender, IHardware hardware)
    {
        HardwareNode node = _root.Nodes.OfType<HardwareNode>().FirstOrDefault(n => n.Hardware == hardware);
        if (node != null)
            _sidebar.SelectTag(node);
    }

    private void ApplyScope(HardwareNode scope, bool persist)
    {
        if (treeView.Model is not TreeModel model)
            return;

        if (model.Scope != scope)
        {
            model.Scope = scope;
            RestoreExpandedState();
        }

        if (persist)
            _settings.SetValue(ScopeSettingKey, scope?.Hardware.Identifier.ToString() ?? string.Empty);
    }

    private void Search_SearchChanged(object sender, EventArgs e)
    {
        if (treeView.Model is not TreeModel model)
            return;

        model.Filter = _search.Query;
        RestoreExpandedState();
    }

    /// <summary>
    /// Re-applies the expand state to nodes reloaded by a scope or filter change: everything is
    /// expanded while searching (without persisting), otherwise the persisted state is used.
    /// </summary>
    private void RestoreExpandedState()
    {
        bool searching = (treeView.Model as TreeModel)?.Filter.Length > 0;
        treeView.BeginUpdate();
        foreach (TreeNodeAdv node in treeView.AllNodes.ToList())
            node.IsExpanded = searching || node.Tag is not IExpandPersistNode persist || persist.Expanded;

        treeView.EndUpdate();
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == (Keys.Control | Keys.F) && _search != null)
        {
            _search.FocusSearch();
            return true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }
}
