// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// Copyright (C) LibreHardwareMonitor and Contributors.
// Partial Copyright (C) Michael Möller <mmoeller@openhardwaremonitor.org> and Contributors.
// All Rights Reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Aga.Controls.Tree;

namespace LibreHardwareMonitor.Windows.Forms.UI;

public class TreeModel : ITreeModel
{
    private readonly Node _root;
    private bool _forceVisible;
    private Node _scope;

    public TreeModel()
    {
        _root = new Node { Model = this };
    }

    public TreePath GetPath(Node node)
    {
        if (node == _root)
            return TreePath.Empty;


        Stack<object> stack = new Stack<object>();
        while (node != _root)
        {
            stack.Push(node);
            node = node.Parent;
        }
        return new TreePath(stack.ToArray());
    }

    public Collection<Node> Nodes
    {
        get { return _root.Nodes; }
    }

    private Node GetNode(TreePath treePath)
    {
        Node parent = _root;
        foreach (object obj in treePath.FullPath)
        {
            if (!(obj is Node node) || node.Parent != parent)
                return null;
            parent = node;
        }
        return parent;
    }

    public IEnumerable GetChildren(TreePath treePath)
    {
        Node node = GetNode(treePath);
        if (node != null)
        {
            return node.Nodes.Where(n => (_forceVisible || n.IsVisible) && !IsOutOfScope(node, n)).ToList();
        }

        return Enumerable.Empty<Node>();
    }

    /// <summary>
    /// Restricts the siblings of <see cref="Scope" /> to the scope node itself, so the view
    /// shows a single hardware subtree. Null shows every node.
    /// </summary>
    public Node Scope
    {
        get { return _scope; }
        set
        {
            if (value != _scope)
            {
                _scope = value;
                OnStructureChanged(_root);
            }
        }
    }

    private bool IsOutOfScope(Node parent, Node node)
    {
        return _scope != null && _scope.Parent == parent && node != _scope;
    }

    public bool IsLeaf(TreePath treePath)
    {
        return false;
    }

    public bool ForceVisible
    {
        get
        {
            return _forceVisible;
        }
        set
        {
            if (value != _forceVisible)
            {
                _forceVisible = value;
                OnStructureChanged(_root);
            }
        }
    }

#pragma warning disable 67
    public event EventHandler<TreeModelEventArgs> NodesChanged;
    public event EventHandler<TreePathEventArgs> StructureChanged;
    public event EventHandler<TreeModelEventArgs> NodesInserted;
    public event EventHandler<TreeModelEventArgs> NodesRemoved;
#pragma warning restore 67

    /// <summary>
    /// Visibility events carry indices computed over all visible siblings; while a scope filters
    /// those siblings the indices do not match the view, so the parent is reloaded instead.
    /// </summary>
    private bool ReloadIfScoped(Node parent)
    {
        if (_scope == null || parent == null || _scope.Parent != parent)
            return false;

        OnStructureChanged(parent);
        return true;
    }

    public void OnNodeChanged(Node parent, int index, Node node)
    {
        if (ReloadIfScoped(parent))
            return;

        if (parent != null)
        {
            TreePath path = GetPath(parent);
            if (path != null)
                NodesChanged?.Invoke(this, new TreeModelEventArgs(path, new[] { index }, new object[] { node }));
        }
    }

    public void OnStructureChanged(Node node)
    {
        StructureChanged?.Invoke(this, new TreeModelEventArgs(GetPath(node), new object[0]));
    }

    public void OnNodeInserted(Node parent, int index, Node node)
    {
        if (ReloadIfScoped(parent))
            return;

        TreeModelEventArgs args = new TreeModelEventArgs(GetPath(parent), new[] { index }, new object[] { node });
        NodesInserted?.Invoke(this, args);
    }

    public void OnNodeRemoved(Node parent, int index, Node node)
    {
        if (ReloadIfScoped(parent))
            return;

        TreeModelEventArgs args = new TreeModelEventArgs(GetPath(parent), new[] { index }, new object[] { node });
        NodesRemoved?.Invoke(this, args);
    }

}
