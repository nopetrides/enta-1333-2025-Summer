using System;
using System.Collections.Generic;

/// <summary>
/// Generic min-heap (priority queue) with efficient DecreaseKey support.
/// Tracks each item's index for O(log n) key updates.
/// </summary>
public class MinHeap<T>
{
    // Internal data: list of (item, priority) pairs representing the heap tree
    private readonly List<(T item, float prio)> _data = new();
    // Comparer for priorities (default: ascending/MinHeap)
    private readonly IComparer<float> _cmp = Comparer<float>.Default;
    // Tracks the index of each item for fast DecreaseKey and membership test
    private readonly Dictionary<T, int> _positions = new();

    /// <summary>
    /// Gets the number of items in the heap.
    /// </summary>
    public int Count => _data.Count;

    /// <summary>
    /// Clears all data in the heap. Safe to call for reuse.
    /// </summary>
    public void Clear()
    {
        _data.Clear();
        _positions.Clear();
    }

    /// <summary>
    /// Adds an item with the given priority.
    /// If the item already exists, calls DecreaseKey if new priority is lower.
    /// </summary>
    public void Enqueue(T item, float priority)
    {
        // If already in heap, possibly lower its priority
        if (_positions.TryGetValue(item, out int index))
        {
            DecreaseKey(item, priority);
            return;
        }
        // Add at end and sift up to restore heap property
        int i = _data.Count;
        _data.Add((item, priority));
        _positions[item] = i;
        SiftUp(i);
    }

    /// <summary>
    /// Decreases the priority of an item if the new priority is less.
    /// Fast O(log n) due to _positions tracking.
    /// </summary>
    public void DecreaseKey(T item, float newPriority)
    {
        if (!_positions.TryGetValue(item, out int i))
            throw new InvalidOperationException("Item not found in heap.");

        var (currentItem, currentPrio) = _data[i];
        // Only update if new priority is actually lower
        if (_cmp.Compare(newPriority, currentPrio) >= 0)
            return;

        _data[i] = (currentItem, newPriority);
        SiftUp(i);
    }

    /// <summary>
    /// Removes and returns the item with the lowest priority.
    /// Throws if the heap is empty.
    /// </summary>
    public T Dequeue()
    {
        if (_data.Count == 0)
            throw new InvalidOperationException("Heap is empty.");

        // The min item is always at index 0
        var (minItem, minPrio) = _data[0];
        int last = _data.Count - 1;
        var lastNode = _data[last];

        // Move the last node to the root and shrink the heap
        _data[0] = lastNode;
        _positions[lastNode.item] = 0;

        _data.RemoveAt(last);
        _positions.Remove(minItem);

        // Restore heap property from the root
        if (_data.Count > 0)
            SiftDown(0);

        return minItem;
    }

    /// <summary>
    /// Moves an item up the tree to restore heap property after insert or key decrease.
    /// </summary>
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

    /// <summary>
    /// Moves an item down the tree to restore heap property after removal.
    /// </summary>
    private void SiftDown(int i)
    {
        int count = _data.Count;
        while (true)
        {
            int left = (i << 1) + 1;
            if (left >= count) break;
            int right = left + 1;

            int smallest = left;
            // Select smaller child
            if (right < count && _cmp.Compare(_data[right].prio, _data[left].prio) < 0)
            {
                smallest = right;
            }

            // If heap property is satisfied, stop
            if (_cmp.Compare(_data[smallest].prio, _data[i].prio) >= 0)
                break;

            Swap(i, smallest);
            i = smallest;
        }
    }

    /// <summary>
    /// Swaps two nodes in the heap and updates their positions in the tracking dictionary.
    /// </summary>
    private void Swap(int i, int j)
    {
        var tmp = _data[i];
        _data[i] = _data[j];
        _data[j] = tmp;
        _positions[_data[i].item] = i;
        _positions[_data[j].item] = j;
    }
}
