// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// Copyright (C) LibreHardwareMonitor and Contributors.
// Partial Copyright (C) Michael Möller <mmoeller@openhardwaremonitor.org> and Contributors.
// All Rights Reserved.

using LibreHardwareMonitor.Hardware;
using LibreHardwareMonitor.Windows.Forms.Utilities;

namespace LibreHardwareMonitor.Windows.Forms.UI;

public sealed class TypeNode : Node, IExpandPersistNode
{
    private readonly PersistentSettings _settings;
    private readonly string _expandedIdentifier;
    private bool _expanded;

    public TypeNode(SensorType sensorType, Identifier parentId, PersistentSettings settings)
    {
        SensorType = sensorType;
        _expandedIdentifier = new Identifier(parentId, SensorType.ToString(), ".expanded").ToString();
        _settings = settings;

        Image = Icons.FluentIcons.ForSensorType(sensorType);

        switch (sensorType)
        {
            case SensorType.Voltage:
                Text = "Voltages";
                break;
            case SensorType.Current:
                Text = "Currents";
                break;
            case SensorType.Energy:
                Text = "Capacities";
                break;
            case SensorType.Clock:
                Text = "Clocks";
                break;
            case SensorType.Load:
                Text = "Load";
                break;
            case SensorType.Temperature:
                Text = "Temperatures";
                break;
            case SensorType.Fan:
                Text = "Fans";
                break;
            case SensorType.Flow:
                Text = "Flows";
                break;
            case SensorType.Control:
                Text = "Controls";
                break;
            case SensorType.Level:
                Text = "Levels";
                break;
            case SensorType.Power:
                Text = "Powers";
                break;
            case SensorType.Data:
                Text = "Data";
                break;
            case SensorType.Factor:
                Text = "Factors";
                break;
            case SensorType.Frequency:
                Text = "Frequencies";
                break;
            case SensorType.Throughput:
                Text = "Throughput";
                break;
            case SensorType.TimeSpan:
                Text = "Times";
                break;
            case SensorType.Timing:
                Text = "Timings";
                break;
            case SensorType.Noise:
                Text = "Noise Levels";
                break;
            case SensorType.Conductivity:
                Text = "Conductivities";
                break;
            case SensorType.Humidity:
                Text = "Humidity Levels";
                break;
        }

        NodeAdded += TypeNode_NodeAdded;
        NodeRemoved += TypeNode_NodeRemoved;
        _expanded = settings.GetValue(_expandedIdentifier, true);
    }

    private void TypeNode_NodeRemoved(Node node)
    {
        node.IsVisibleChanged -= Node_IsVisibleChanged;
        Node_IsVisibleChanged(null);
    }

    private void TypeNode_NodeAdded(Node node)
    {
        node.IsVisibleChanged += Node_IsVisibleChanged;
        Node_IsVisibleChanged(null);
    }

    private void Node_IsVisibleChanged(Node node)
    {
        foreach (Node n in Nodes)
        {
            if (n.IsVisible)
            {
                IsVisible = true;
                return;
            }
        }
        IsVisible = false;
    }

    public SensorType SensorType { get; }

    public bool Expanded
    {
        get => _expanded;
        set
        {
            _expanded = value;
            _settings.SetValue(_expandedIdentifier, _expanded);
        }
    }
}
