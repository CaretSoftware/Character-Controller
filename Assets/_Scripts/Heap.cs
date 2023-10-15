using System;
using System.Collections.Generic;

// Original Java implementation by @Author Mark Allen Weiss - Data Structures and Algorithm Analysis in Java
// @Author Patrik Bergsten - Translated to C#, added Comparer and duplicate check functionality
public class Heap<T> {
    private const int DefaultCapacity = 12;
    
    public bool DisallowDuplicates { get; set; } = false;
    
    private int _currentSize;
    private T[] _array;
    private IComparer<T> _comparer;
    private HashSet<T> _duplicateCheckSet;
    
    /// <summary>
    /// Min Heap. Supply comparator if default CompareTo() not desirable.
    /// </summary>
    /// <param name="comparer">Optional parameter. Tells the heap how to evaluate <T>.</param>
    /// <param name="capacity">The desired initial size of heap array. Capacity enlarges as heap gets full.</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="UnderflowException"></exception>
    public Heap(IComparer<T> comparer = null, int capacity = DefaultCapacity) {
        _currentSize = 0;
        _array = new T[capacity + 1];
        _duplicateCheckSet = new HashSet<T>();
        
        if (comparer == null)
            if (typeof(IComparable).IsAssignableFrom(typeof(T)) ||
                typeof(IComparable<T>).IsAssignableFrom(typeof(T)))
                comparer = Comparer<T>.Default;

        if (comparer == null)
            throw new ArgumentNullException(
                nameof(comparer),
                $"There's no default comparer for {typeof(T).Name} class, you should provide one explicitly.");
        
        _comparer = comparer;
    }

    public Heap() : this(null, DefaultCapacity){ }

    public Heap(int capacity) : this(null, capacity) { }

    public Heap(T[] items) {
        _currentSize = items.Length;
        _array = new T[(_currentSize + 2) * 11 / 10];
        _duplicateCheckSet = new HashSet<T>(items);
        int i = 1;
        foreach (T item in items)
            _array[i++] = item;
        BuildHeap();
    }

    /// <summary>
    /// Insert into the priority queue, maintaining heap order.
    /// </summary>
    /// <remarks>
    /// Does not throw exception if insertion attempt of duplicate. 
    /// Duplicates are allowed if not property <c>DisallowDuplicates</c> explicitly set to true.
    /// </remarks>
    /// <returns>False if item already in heap and <c>DisallowDuplicates</c> is true.</returns>
    /// <param name="x">The item to insert</param>
    public bool Insert(T x) {
        if (!_duplicateCheckSet.Add(x) && DisallowDuplicates) // adds x, checks for duplicates
            return false; // item was not inserted, duplicate value!

        if (_currentSize == _array.Length - 1)
            EnlargeArray(_array.Length * 2 + 1);
        
        // percolate up
        int hole = ++_currentSize;
        for (_array[0] = x; _comparer.Compare(x, _array[hole / 2]) < 0; hole /= 2) {
            _array[hole] = _array[hole / 2];
        }   
        _array[hole] = x;

        return true;
    }

    /// <summary>
    /// Preview lowest item without removing.
    /// </summary>
    /// <returns>Lowest T item.</returns>
    /// <exception cref="UnderflowException"></exception>
    public T Peek() {
        if(Empty())
            throw new UnderflowException( "Heap is empty" );
        return _array[1];
    }

    /// <summary>
    /// Removes and returns lowest item.
    /// </summary>
    /// <returns>Lowest T item.</returns>
    /// <exception cref="UnderflowException"></exception>
    public T DeleteMin() {
        if (Empty())
            throw new UnderflowException("Cannot perform Delete operation on an empty Heap");

        T minItem = Peek();
        _duplicateCheckSet.Remove(minItem);
        _array[1] = _array[_currentSize--];
        PercolateDown(1);

        return minItem;
    }

    /// <summary>
    /// Check if items are present or not.
    /// </summary>
    /// <returns>Boolean.</returns>
    public bool Empty() {
        return _currentSize == 0;
    }

    /// <summary>
    /// Clear heap.
    /// </summary>
    public void MakeEmpty() {
        _duplicateCheckSet.Clear();;
        _currentSize = 0;
    }

    /// <summary>
    /// Check if item present in heap.
    /// </summary>
    /// <param name="item">T item.</param>
    /// <returns>Boolean.</returns>
    public bool Contains(T item) {
        return _duplicateCheckSet.Contains(item);
    }

    private void PercolateDown(int hole) {
        int child;
        T tmp = _array[hole];

        for ( ; hole * 2 <= _currentSize; hole = child) {
            child = hole * 2;
            if (child != _currentSize && _comparer.Compare(_array[child + 1], _array[child]) < 0)
                child++;
            if (_comparer.Compare(_array[child], tmp) < 0)
                _array[hole] = _array[child];
            else
                break;
        }

        _array[hole] = tmp;
    }

    private void BuildHeap() {
        for(int i = _currentSize / 2; i > 0; i--)
            PercolateDown(i);
    }

    private void EnlargeArray(int newSize) {
        T [] old = _array;
        _array = new T[ newSize ];
        for( int i = 0; i < old.Length; i++ )
            _array[ i ] = old[ i ];
    }
}

public class UnderflowException : Exception {
    public UnderflowException(string message): base(message) { }
}
