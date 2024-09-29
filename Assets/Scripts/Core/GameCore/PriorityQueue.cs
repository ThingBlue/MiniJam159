using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MiniJam159.GameCore
{
    // Priority queue implementation using a min heap
    public class PriorityQueue<T>
    {
        public List<KeyValuePair<float, T>> heap;

        public PriorityQueue()
        {
            heap = new List<KeyValuePair<float, T>>();
        }

        public int count() { return heap.Count; }

        /*
        public KeyValuePair<float, T> this[int index]
        {
            get
            {
                if (index < 0 || index >= heap.Count) throw new IndexOutOfRangeException("Index out of range.");
                return heap[index];
            }
            set
            {
                if (index < 0 || index >= heap.Count) throw new IndexOutOfRangeException("Index out of range.");
                heap[index] = value;
            }
        }
        */

        public void add(float priority, T value)
        {
            // Add to the end
            heap.Add(new KeyValuePair<float, T>(priority, value));

            // Swap upwards until priority is not less than parent
            int i = heap.Count - 1;
            while (i != 0 && heap[i].Key < heap[getParentIndex(i)].Key)
            {
                swap(i, getParentIndex(i));
                i = getParentIndex(i);
            }
        }

        public T pop()
        {
            // Return default value if heap is empty
            if (heap.Count == 0) return default(T);

            T result = heap[0].Value;

            // Swap last item into first, and remove first item
            swap(0, heap.Count - 1);
            heap.RemoveAt(heap.Count - 1);

            // Restore heap order
            minHeapify(0);

            return result;
        }

        public void clear() { heap.Clear(); }

        public bool contains(T item)
        {
            foreach (KeyValuePair<float, T> pair in heap)
            {
                if (pair.Value.Equals(item)) return true;
            }
            return false;
        }

        private void minHeapify(int index)
        {
            if (index > heap.Count - 1) return;

            // Get both children indices
            int leftIndex = getLeftChildIndex(index);
            int rightIndex = getRightChildIndex(index);

            // Find the index between the 3 that has the smallest key (If the indices exist)
            int smallestKeyIndex = index;
            if (leftIndex < heap.Count && heap[leftIndex].Key < heap[smallestKeyIndex].Key) smallestKeyIndex = leftIndex;
            if (rightIndex < heap.Count && heap[rightIndex].Key < heap[smallestKeyIndex].Key) smallestKeyIndex = rightIndex;

            // Swap with smallest and recurse
            if (smallestKeyIndex != index)
            {
                swap(index, smallestKeyIndex);
                minHeapify(smallestKeyIndex);
            }
        }

        private void swap(int index1, int index2)
        {
            KeyValuePair<float, T> temp = heap[index1];
            heap[index1] = heap[index2];
            heap[index2] = temp;
        }
        private int getParentIndex(int index) { return (index - 1) / 2; }
        private int getLeftChildIndex(int index) { return 2 * index + 1; }
        private int getRightChildIndex(int index) { return 2 * index + 2; }
    }
}
