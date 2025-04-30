namespace SpieleSammlung.Model.Util;

public class RingBufferFifo<T>
{
    private readonly int _capacity;
    private Node _first;
    private Node _last;
    private int _size;

    public RingBufferFifo(int size, T initialValue)
    {
        _size = 1;
        _capacity = size;
        _first = new Node(initialValue);
        _last = _first;
    }

    public void Insert(T value)
    {
        Node next = new Node(value);
        _first.Next = next;
        _first = next;
        if (_size >= _capacity)
            _last = _last.Next;
        else
            ++_size;
    }

    public T Peek() => _last.Value;

    private class Node(T value)
    {
        public readonly T Value = value;
        public Node Next;
    }
}