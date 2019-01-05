using System;

namespace Tello.Video
{
    //https://stackoverflow.com/questions/590069/how-would-you-code-an-efficient-circular-buffer-in-java-or-c-sharp
    //todo: make threadsafe
    internal class RingBuffer<T>
    {
        public RingBuffer(int size)
        {
            _buffer = new T[size];
        }

        private readonly T[] _buffer;
        private readonly object _gate = new object();
        private int _tail = 0;
        private int _head = 0;

        public bool IsEmpty
        {
            get
            {
                lock (_gate)
                {
                    return _head == _tail;
                }
            }
        }

        public void Push(T item)
        {
            lock (_gate)
            {
                _buffer[_tail] = item;
                _tail = (_tail + 1) % _buffer.Length;
            }
        }

        public bool TryPop(out T result)
        {
            if (IsEmpty)
            {
                result = default(T);
                return false;
            }

            lock (_gate)
            {
                result = _buffer[_head];
                _buffer[_head] = default(T);

                var canMoveHead = _head < _tail || _buffer.Length - _head < _tail;
                if (canMoveHead)
                {
                    _head = (_head + 1) % _buffer.Length;
                }
            }
            return true;
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
                result = _buffer[_head];
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
                _tail = 0;
                _head = 0;
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
                var result = new T[_buffer.Length];
                Array.Copy(_buffer, result, _buffer.Length);
                return result;
            }
        }
    }
}
