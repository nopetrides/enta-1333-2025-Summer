using System;
using System.Collections.Generic;

/// <summary>
/// Min-heap implementation with DecreaseKey support.
/// </summary>
public class MinHeap<T>
{
    private readonly List<(T item, float prio)> _data = new();
    private readonly IComparer<float> _cmp = Comparer<float>.Default;
    private readonly Dictionary<T, int> _positions = new();

    /// <summary>Number of elements in the heap.</summary>
    public int Count => _data.Count;

    /// <summary>Clears the heap.</summary>
    public void Clear()
    {
        _data.Clear();
        _positions.Clear();
    }

    /// <summary>
    /// Inserts an item with the given priority. If the item already exists, decreases its key.
    /// </summary>
    public void Enqueue(T item, float priority)
    {
        if (_positions.TryGetValue(item, out int index))
        {
            DecreaseKey(item, priority);
            return;
        }
        int i = _data.Count;
        _data.Add((item, priority));
        _positions[item] = i;
        SiftUp(i);
    }

    /// <summary>
    /// Decreases the priority of an existing item.
    /// If newPriority is not less, does nothing.
    /// </summary>
    public void DecreaseKey(T item, float newPriority)
    {
        if (!_positions.TryGetValue(item, out int i))
            throw new InvalidOperationException("Item not found in heap.");

        var (currentItem, currentPrio) = _data[i];
        if (_cmp.Compare(newPriority, currentPrio) >= 0)
            return;

        _data[i] = (currentItem, newPriority);
        SiftUp(i);
    }

    /// <summary>
    /// Removes and returns the item with the smallest priority.
    /// </summary>
    public T Dequeue()
    {
        if (_data.Count == 0)
            throw new InvalidOperationException("Heap is empty.");

        var (minItem, minPrio) = _data[0];
        int last = _data.Count - 1;
        var lastNode = _data[last];

        _data[0] = lastNode;
        _positions[lastNode.item] = 0;

        _data.RemoveAt(last);
        _positions.Remove(minItem);

        if (_data.Count > 0)
            SiftDown(0);

        return minItem;
    }

    private void SiftUp(int i)
    {
        while (i > 0)
        {
            int parent = (i - 1) >> 1;
            if (_cmp.Compare(_data[i].prio, _data[parent].prio) >= 0)
                break;

            Swap(i, parent);
            i = parent;
        }
    }

    private void SiftDown(int i)
    {
        int count = _data.Count;
        while (true)
        {
            int left = (i << 1) + 1;
            if (left >= count) break;
            int right = left + 1;

            int smallest = left;
            if (right < count && _cmp.Compare(_data[right].prio, _data[left].prio) < 0)
            {
                smallest = right;
            }

            if (_cmp.Compare(_data[smallest].prio, _data[i].prio) >= 0)
                break;

            Swap(i, smallest);
            i = smallest;
        }
    }

    private void Swap(int i, int j)
    {
        var tmp = _data[i];
        _data[i] = _data[j];
        _data[j] = tmp;
        _positions[_data[i].item] = i;
        _positions[_data[j].item] = j;
    }
}
