using UnityEngine;

using System;

public class SelectionManager : MonoBehaviour
{
    // Singleton instance
    public static SelectionManager I { get; private set; }

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

    public void _Select(ISelectable next, in SelectionArgs args)
    {
        if (next == null)
        {
            Clear();
            return;
        }

        var prev = _current;

        // Deselect previous if we're not doing additive selection
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

