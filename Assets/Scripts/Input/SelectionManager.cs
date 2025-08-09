using System;
using System.Collections.Generic;
using UnityEngine;

public class SelectionManager : MonoBehaviour
{
    // Singleton
    public static SelectionManager I { get; private set; }

    // Current "focused" selection (last clicked)
    private ISelectable _current;
    public ISelectable Current => _current;

    // Selected set (for additive selection)
    private readonly HashSet<ISelectable> _selected = new HashSet<ISelectable>();

    // Events
    public event Action<ISelectable, ISelectable, SelectionArgs> OnSelectionChanged; // (prevCurrent, newCurrent, args)
    public event Action OnSelectionCleared;

    // Helper: Unity "alive" check (handles fake-null after Destroy)
    private static bool IsAlive(ISelectable s) => (s as UnityEngine.Object) != null;

    private void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Select an item. If args.IsAdditive is true, item is added to the selection set without deselecting others.
    /// If OnSelected returns false (consumed = false), selection is not changed.
    /// </summary>
    public void SelectInstance(ISelectable next, in SelectionArgs args)
    {
        // Sanitize existing state (remove dead references)
        PruneDead();

        // Null or dead target
        if (next == null || !IsAlive(next))
        {
            if (!args.IsAdditive) Clear();
            return;
        }

        var prev = _current;

        // ---- ADDITIVE (multi-select) ----
        if (args.IsAdditive)
        {
            // Already selected? Just focus it (and optionally refresh)
            if (_selected.Contains(next))
            {
                // Optional: re-fire selection to refresh UI/VFX
                _ = SafeSelect(next, in args); // ignored result
                Focus(next, prev, in args);
                return;
            }

            // Try to select and add to set; respect consumption
            if (SafeSelect(next, in args))
            {
                _selected.Add(next);
                Focus(next, prev, in args);
            }
            return;
        }

        // ---- SINGLE SELECT (replace) ----

        // Same object focused already? No-op (or refresh if you prefer)
        if (ReferenceEquals(prev, next))
        {
            // Optional refresh:
            _ = SafeSelect(next, in args);
            return;
        }

        // Deselect all previous items (respect consumption)
        SafeDeselectAll();

        _selected.Clear();

        // Try to select the new one; respect consumption
        if (SafeSelect(next, in args))
        {
            _selected.Add(next);
            Focus(next, prev, in args);
        }
        else
        {
            // Selection was rejected/consumed=false; no current
            _current = null;
            OnSelectionCleared?.Invoke();
        }
    }

    /// <summary> Public static convenience wrapper. </summary>
    public static void Select(ISelectable next, in SelectionArgs args) => I?.SelectInstance(next, in args);

    /// <summary> Toggle selection membership for an item (useful with Ctrl/Alt). </summary>
    public void ToggleInstance(ISelectable item, in SelectionArgs args)
    {
        PruneDead();
        if (item == null || !IsAlive(item)) return;

        if (_selected.Contains(item))
        {
            if (SafeDeselect(item))
            {
                _selected.Remove(item);
                if (ReferenceEquals(_current, item))
                {
                    _current = null;
                    OnSelectionCleared?.Invoke();
                }
            }
        }
        else
        {
            if (SafeSelect(item, in args))
            {
                _selected.Add(item);
                Focus(item, _current, in args);
            }
        }
    }

    public static void Toggle(ISelectable item, in SelectionArgs args) => I?.ToggleInstance(item, in args);

    /// <summary> Deselect everything. </summary>
    public void Clear()
    {
        if (_selected.Count == 0 && _current == null) return;

        SafeDeselectAll();
        _selected.Clear();
        _current = null;
        OnSelectionCleared?.Invoke();
    }

    public static void ClearSelection() => I?.Clear();

    // ---------------- internals ----------------

    // Call OnSelected safely; returns true if the selectable accepted/consumed the selection.
    private static bool SafeSelect(ISelectable s, in SelectionArgs args)
    {
        if (!IsAlive(s)) return false;
        try { return s.OnSelected(in args); }
        catch (MissingReferenceException) { return false; }
        catch (Exception e)
        {
            Debug.LogException(e);
            return false;
        }
    }

    // Call OnDeselected safely; returns true if it completed.
    private static bool SafeDeselect(ISelectable s)
    {
        if (!IsAlive(s)) return false;
        try { return s.OnDeselected(); }
        catch (MissingReferenceException) { return false; }
        catch (Exception e)
        {
            Debug.LogException(e);
            return false;
        }
    }

    private void SafeDeselectAll()
    {
        // Copy to temp list to avoid modifying while iterating
        _temp.Clear();
        foreach (var s in _selected) _temp.Add(s);

        foreach (var s in _temp)
            SafeDeselect(s);

        _temp.Clear();
    }

    private void Focus(ISelectable next, ISelectable prev, in SelectionArgs args)
    {
        _current = next;
        OnSelectionChanged?.Invoke(prev, _current, args);
    }

    private void PruneDead()
    {
        if (_current != null && !IsAlive(_current)) _current = null;

        _temp.Clear();
        foreach (var s in _selected) if (!IsAlive(s)) _temp.Add(s);
        foreach (var s in _temp) _selected.Remove(s);
        _temp.Clear();
    }

    // Reusable temp list to avoid allocs
    private readonly List<ISelectable> _temp = new List<ISelectable>(8);
}

/// <summary>
/// /////////////////////////////////////////////
/// </summary>
/// 
public class StandardSelectionManager : MonoBehaviour
{
    // Singleton instance
    public static StandardSelectionManager I { get; private set; }

    // Order early so it's ready before other scripts (optional)
    // [DefaultExecutionOrder(-100)]

    private ISelectable _current;
    public ISelectable Current => _current;

    // Optional: subscribe if you want UI to react to selection changes
    public event Action<ISelectable, ISelectable, SelectionArgs> OnSelectionChanged;
    public event Action OnSelectionCleared;

    private void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }
        I = this;
        DontDestroyOnLoad(gameObject);
    }

    // var consumed = next.OnSelected(in args);
    // if (!consumed && prev != null && !args.IsAdditive)
    //     prev.OnDeselected();

    public void _Select(ISelectable next, in SelectionArgs args)
    {
        if (next == null)
        {
            Clear();
            return;
        }

        var prev = _current;

        // Deselect previous if we're not doing additive selection
        // Unity overloads == with potential false nulls on destroy, etc, use RefEq
        if (prev != null && !ReferenceEquals(prev, next) && !args.IsAdditive)
            prev.OnDeselected();

        _current = next;
        next.OnSelected(in args);

        OnSelectionChanged?.Invoke(prev, _current, args);
    }

    public void Clear()
    {
        if (_current != null)
        {
            _current.OnDeselected();
            _current = null;
            OnSelectionCleared?.Invoke();
        }
    }

    // Convenience wrappers so callers can do SelectionManager.Select(...)
    public static void Select(ISelectable next, in SelectionArgs args) => I?._Select(next, in args);
    public static void ClearSelection() => I?.Clear();
}


public class SimpleSelectionManager : MonoBehaviour
{
    private ISelectable _current;

    public void Select(ISelectable next, in SelectionArgs args)
    {
        if (_current != null && !args.IsAdditive) _current.OnDeselected();
        _current = next;
        next.OnSelected(in args);
    }

    public void Clear()
    {
        _current?.OnDeselected();
        _current = null;
    }
}

public static class StaticSelectionManager
{
    private static ISelectable _current;
    public static ISelectable Current => _current;

    public static event Action<ISelectable, ISelectable, SelectionArgs> OnSelectionChanged;
    public static event Action OnSelectionCleared;

    public static void Select(ISelectable next, in SelectionArgs args)
    {
        if (next == null)
        {
            Clear();
            return;
        }

        var prev = _current;

        if (prev != null && !ReferenceEquals(prev, next) && !args.IsAdditive)
            prev.OnDeselected();

        _current = next;
        next.OnSelected(in args);

        OnSelectionChanged?.Invoke(prev, _current, args);
    }

    public static void Clear()
    {
        if (_current != null)
        {
            _current.OnDeselected();
            _current = null;
            OnSelectionCleared?.Invoke();
        }
    }
}