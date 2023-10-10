namespace SpieleSammlung.Model.Util
{
    public class RingBufferFifo<T>
    {
        private readonly int _capacity;
        private int _size;
        private Node _first;
        private Node _last;

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
            _first.next = next;
            _first = next;
            if (_size >= _capacity)
            {
                _last = _last.next;
            }
            else
            {
                ++_size;
            }
        }

        public T Peek()
        {
            return _last.value;
        }

        private class Node
        {
            public readonly T value;
            public Node next;

            public Node(T value)
            {
                this.value = value;
            }
        }
    }
}