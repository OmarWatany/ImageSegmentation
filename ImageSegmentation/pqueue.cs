using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageTemplate
{
    public class PriorityQueue<T>
    {
        public List<T> heap;

        public int Count => heap.Count;
        Func<T, T, int> compare;

        public PriorityQueue(Func<T,T,int> compare,int n) {
            this.compare = compare;
            heap = new List<T>(n);
        }

        public PriorityQueue(Func<T,T,int> compare) {
            this.compare = compare;
            heap = new List<T>(1024);
        }

        public void Enqueue(T item)
        {
            heap.Add(item);
            int i = heap.Count - 1;
            while (i > 0)
            {
                int parent = (i - 1) / 2;
                if (this.compare(heap[parent],heap[i]) <= 0) break;
                Swap(parent, i);
                i = parent;
            }
        }

        public T Dequeue()
        {
            T first = heap[0];
            heap[0] = heap[heap.Count - 1];
            heap.RemoveAt(heap.Count - 1);
            Heapify(0);
            return first;
        }

        private void Heapify(int i)
        {
            int left = 2 * i + 1;
            int right = 2 * i + 2;
            int smallest = i;
            if (left < heap.Count && this.compare(heap[left], heap[smallest]) < 0)
                smallest = left;
            if (right < heap.Count && this.compare(heap[right], heap[smallest]) < 0)
                smallest = right;
            if (smallest != i)
            {
                Swap(i, smallest);
                Heapify(smallest);
            }
        }

        private void Swap(int i, int j) => (heap[i], heap[j]) = (heap[j], heap[i]);
    }
}
