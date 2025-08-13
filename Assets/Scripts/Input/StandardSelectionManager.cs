using System;
using UnityEngine;

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