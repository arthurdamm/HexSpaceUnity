using System;
using System.Collections.Generic;
using UnityEngine;

public class SelectionManager : MonoBehaviour
{
    // Singleton
    public static SelectionManager Instance { get; private set; }

    // Current "focused" selection (last clicked)
    private ISelectable _current;
    public ISelectable Current => _current;

    // Selected set (for additive selection)
    private readonly HashSet<ISelectable> _selected = new HashSet<ISelectable>();
    public IReadOnlyCollection<ISelectable> Selected => _selected;

    // Events
    public event Action<ISelectable, ISelectable, SelectionArgs> OnSelectionChanged; // (prevCurrent, newCurrent, args)
    public event Action OnSelectionCleared;

    // Helper: Unity "alive" check (handles fake-null after Destroy)
    private static bool IsAlive(ISelectable s) => (s as UnityEngine.Object) != null;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
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
    public static void Select(ISelectable next, in SelectionArgs args) => Instance?.SelectInstance(next, in args);

    public static bool TryGet<T>(out T value) where T : class, ISelectable
    {
        value = Instance?._current as T;
        return value != null;
    }

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

    public static void Toggle(ISelectable item, in SelectionArgs args) => Instance?.ToggleInstance(item, in args);

    /// <summary> Deselect everything. </summary>
    public void Clear()
    {
        if (_selected.Count == 0 && _current == null) return;

        SafeDeselectAll();
        _selected.Clear();
        _current = null;
        OnSelectionCleared?.Invoke();
    }


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
