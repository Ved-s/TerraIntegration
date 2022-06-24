using System.Collections.Generic;

namespace TerraIntegration.DataStructures
{
    public class RingArray<T>
    {
        public T[] Values;
        int Index, Count;

        public int Length => Count;

        public RingArray(int capacity)
        {
            Values = new T[capacity];
        }

        int ConvertIndex(int index)
        {
            index += Index;
            index %= Values.Length;
            if (index < 0)
                index += Values.Length;
            return index;
        }

        public T this[int i]
        {
            get => Values[ConvertIndex(i)];
            set => Values[ConvertIndex(i)] = value;
        }

        public void Push(T value)
        {
            Values[Index] = value;
            Index = (Index + 1) % Values.Length;
            if (Count < Values.Length)
                Count++;
        }

        public T Pop()
        {
            if (Count <= 0)
                return default;

            Index = ConvertIndex(-1);
            Count--;
            return Values[Index];
        }

        public IEnumerable<T> Enumerate()
        {
            for (int i = -(Count - 1); i <= 0; i++)
                yield return Values[ConvertIndex(i)];
        }

        public IEnumerable<T> EnumerateBackwards()
        {
            for (int i = 1; i <= Count; i++)
                yield return Values[ConvertIndex(-i)];
        }
    }
}
