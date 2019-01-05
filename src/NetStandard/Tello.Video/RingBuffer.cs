using System;

namespace Tello.Video
{
    internal class RingBuffer<T>
    {
        public RingBuffer(int size)
        {
            _buffer = new T[size];
        }

        private readonly T[] _buffer;
        private readonly object _gate = new object();
        private int _head = 0;
        private int _tail = 0;

        public bool IsEmpty
        {
            get
            {
                lock (_gate)
                {
                    return _tail == _head;
                }
            }
        }

        public void Push(T item)
        {
            lock (_gate)
            {
                _buffer[_head] = item;
                _head = (_head + 1) % _buffer.Length;
            }
        }

        public bool TryPop(out T result)
        {
            result = default(T);
            lock (_gate)
            {
                if (_tail != _head)
                {
                    result = _buffer[_tail];
                    _buffer[_tail] = default(T);
                    _tail = (_tail + 1) % _buffer.Length;
                }
            }
            return result != null;
        }

        public bool TryPeek(out T result)
        {
            if (IsEmpty)
            {
                result = default(T);
                return false;
            }

            lock (_gate)
            {
                result = _buffer[_tail];
            }
            return true;
        }

        public bool TryClear()
        {
            if (IsEmpty)
            {
                return false;
            }

            lock (_gate)
            {
                Array.Clear(_buffer, 0, _buffer.Length);
                _head = 0;
                _tail = 0;
            }
            return true;
        }

        public T[] ToArray()
        {
            if (IsEmpty)
            {
                return new T[0];
            }

            lock (_gate)
            {
                var inverted = _tail > _head;
                var size = inverted
                    ? _buffer.Length - _tail + _head
                    : _head - _tail;

                var result = new T[size];
                if (inverted)
                {
                    var tailToEnd = _buffer.Length - _tail;
                    Array.Copy(_buffer, _tail, result, 0, tailToEnd);
                    Array.Copy(_buffer, 0, result, tailToEnd, _head);
                }
                else
                {
                    Array.Copy(_buffer, _tail, result, 0, size);
                }

                return result;
            }
        }
    }
}
