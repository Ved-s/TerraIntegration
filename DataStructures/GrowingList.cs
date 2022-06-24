using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerraIntegration.DataStructures
{
    public class GrowingList<T> : IEnumerable<T>
    {
        T[] Array;

        public int Count { get; private set; }
        public int Capacity => Array.Length;

        public T this[int index] 
        {
            get 
            {
                if (index >= Count)
                    return default;
                return Array[index];
            }
            set 
            {
                if (index >= Capacity)
                {
                    Resize(index + index / 10 + 1);
                }
                Array[index] = value;
                Count = Math.Max(Count, index + 1);
            }
        }

        public GrowingList(int initialCapacity = 10)
        {
            initialCapacity = Math.Max(initialCapacity, 1);
            Array = new T[initialCapacity];
        }

        public void Add(T obj)
        {
            if (Count == Capacity)
            {
                Resize(Count + Count / 10 + 1);
            }
            Array[Count] = obj;
            Count++;
        }

        public void Clear()
        {
            for (int i = 0; i < Count; i++)
                Array[i] = default(T);
            Count = 0;
        }

        void Resize(int size)
        {
            System.Array.Resize(ref Array, size);
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < Count; i++) 
                yield return Array[i];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<T>).GetEnumerator();
        }
    }
}
