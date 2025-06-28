using System;
using System.Collections.Generic;

/// <summary>Minimal binary min-heap (O(log n) push/pop).</summary>
public class MinHeap<T>
{
    private readonly List<(T item, float prio)> _data = new();
    private readonly IComparer<float> _cmp = Comparer<float>.Default;

    public int Count => _data.Count;

    public void Clear() => _data.Clear();

    public void Enqueue(T item, float priority)
    {
        _data.Add((item, priority));
        SiftUp(_data.Count - 1);
    }

    public T Dequeue()
    {
        int last = _data.Count - 1;
        (T it, _) = _data[0];
        _data[0] = _data[last];
        _data.RemoveAt(last);
        SiftDown(0);
        return it;
    }

    private void SiftUp(int i)
    {
        while (i > 0)
        {
            int p = (i - 1) >> 1;
            if (_cmp.Compare(_data[i].prio, _data[p].prio) >= 0) break;
            (_data[i], _data[p]) = (_data[p], _data[i]);
            i = p;
        }
    }

    private void SiftDown(int i)
    {
        int n = _data.Count;
        while (true)
        {
            int l = (i << 1) + 1;
            int r = l + 1;
            int smallest = i;

            if (l < n && _cmp.Compare(_data[l].prio, _data[smallest].prio) < 0) smallest = l;
            if (r < n && _cmp.Compare(_data[r].prio, _data[smallest].prio) < 0) smallest = r;
            if (smallest == i) break;
            (_data[i], _data[smallest]) = (_data[smallest], _data[i]);
            i = smallest;
        }
    }
}
