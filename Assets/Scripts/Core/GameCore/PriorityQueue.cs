using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MiniJam159.GameCore
{
    public class PriorityQueueBase<T>
    {
        public List<KeyValuePair<float, T>> heap = new List<KeyValuePair<float, T>>();

        public int count() { return heap.Count; }
        public void clear() { heap.Clear(); }
        public bool contains(T item)
        {
            foreach (KeyValuePair<float, T> pair in heap)
            {
                if (pair.Value.Equals(item)) return true;
            }
            return false;
        }

        // Helper functions
        protected void swap(int index1, int index2)
        {
            KeyValuePair<float, T> temp = heap[index1];
            heap[index1] = heap[index2];
            heap[index2] = temp;
        }
        protected int getParentIndex(int index) { return (index - 1) / 2; }
        protected int getLeftChildIndex(int index) { return 2 * index + 1; }
        protected int getRightChildIndex(int index) { return 2 * index + 2; }
    }

    // Priority queue implementation using heaps
    public class MinPriorityQueue<T> : PriorityQueueBase<T>
    {
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

        protected void minHeapify(int index)
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

    }

    public class MaxPriorityQueue<T>: PriorityQueueBase<T>
    {
        public void add(float priority, T value)
        {
            // Add to the end
            heap.Add(new KeyValuePair<float, T>(priority, value));

            // Swap upwards until priority is not larger than parent
            int i = heap.Count - 1;
            while (i != 0 && heap[i].Key > heap[getParentIndex(i)].Key)
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
            maxHeapify(0);

            return result;
        }

        protected void maxHeapify(int index)
        {
            if (index > heap.Count - 1) return;

            // Get both children indices
            int leftIndex = getLeftChildIndex(index);
            int rightIndex = getRightChildIndex(index);

            // Find the index between the 3 that has the largest key (If the indices exist)
            int largestKeyIndex = index;
            if (leftIndex < heap.Count && heap[leftIndex].Key > heap[largestKeyIndex].Key) largestKeyIndex = leftIndex;
            if (rightIndex < heap.Count && heap[rightIndex].Key > heap[largestKeyIndex].Key) largestKeyIndex = rightIndex;

            // Swap with smallest and recurse
            if (largestKeyIndex != index)
            {
                swap(index, largestKeyIndex);
                maxHeapify(largestKeyIndex);
            }
        }

    }
}
