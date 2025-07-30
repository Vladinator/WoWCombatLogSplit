namespace WoWCombatLogSplit.src
{
    internal class SlidingBuffer<T>(int capacity)
    {
        public readonly int Capacity = capacity;
        private readonly T[] Buffer = new T[capacity];
        private int Count;
        private int Position;
        public int Length { get { return Count; } }
        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }
                return Buffer[(Position + index) % Capacity];
            }
        }
        public void Push(T item)
        {
            if (Count == Capacity)
            {
                Buffer[Position] = item;
                Position = (Position + 1) % Capacity;
                return;
            }
            int index = (Position + Count) % Capacity;
            Buffer[index] = item;
            Count++;
        }
        public char[] ToCharArray()
        {
            var chars = new char[Count];
            if (Count == 0)
            {
                return chars;
            }
            var len = Capacity - Position;
            if (len > Count)
            {
                len = Count;
            }
            Array.Copy(Buffer, Position, chars, 0, len);
            if (Count > len)
            {
                Array.Copy(Buffer, 0, chars, len, Count - len);
            }
            return chars;
        }
    }
}
