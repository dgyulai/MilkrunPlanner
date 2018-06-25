using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HeapApp
{
    /// <summary>
    /// The class stores data in heap strucutre.
    /// </summary>
    /// <typeparam name="T">type of the stored data</typeparam>
    class Heap<T>  where T: IComparable
    {
        private readonly List<T> _listOfValues = new List<T>();

        /// <summary>
        /// Insert an item into the heap
        /// </summary>
        /// <param name="item">the item to insert</param>
        public void Add(T item)
        {
            int child = _listOfValues.Count + 1;
            int parent = child >> 1;
            _listOfValues.Add(item);
            while (parent != 0 && ((IComparable)_listOfValues[parent-1]).CompareTo(item) > 0)
            {
                    _listOfValues[child-1] = _listOfValues[parent-1];
                child = parent;
                parent >>= 1;
            }
            _listOfValues[child-1] = item;
        }

        /// <summary>
        /// The number of items stored in the heap
        /// </summary>
        public int Count
        {
            get { return _listOfValues.Count; }
        }

        /// <summary>
        /// Return the first element of the heap 
        /// </summary>
        /// <returns>the first element of the heap if it is not empty, otherwise the default value of the type</returns>
        public T GetFirst()
        {
            if(_listOfValues.Count>0)
            {
                return _listOfValues[0];
            }
            else
            {
                return default(T);
            }
        }

        /// <summary>
        /// Remove the first element of the heap
        /// </summary>
        /// <returns>the first element of the heap if it is not empty, otherwise the default value of the type</returns>
        public T GetAndRemoveFirst()
        {
            if (_listOfValues.Count > 0)
            {
                T first = _listOfValues[0];
                T last = _listOfValues[_listOfValues.Count - 1];

                int x = 1;
                int c = MinChild(1);
                while (((IComparable)_listOfValues[c - 1]).CompareTo(last) < 0)
                {
                    _listOfValues[x - 1] = _listOfValues[c - 1];
                    x = c;
                    c = MinChild(c);
                }
                _listOfValues[x - 1] = last;
                _listOfValues.RemoveAt(_listOfValues.Count - 1);
                return first;
            }
            else
            {
                return default(T);
            }
        }

        /// <summary>
        /// Find the smallest child of a node
        /// </summary>
        /// <param name="parent">node</param>
        /// <returns>the smallest child</returns>
        private int MinChild(int parent)
        {
            int left = parent << 1;
            int right = left | 1;
            int result;
            if(left>_listOfValues.Count-1)
            {
                return _listOfValues.Count;
            } else
            {
                if(right > (_listOfValues.Count) || ((IComparable)_listOfValues[left-1]).CompareTo(_listOfValues[right-1])<0)
                {
                    result = left;
                } else
                {
                    result = right;
                }
            }

            return result;
        }
    }
}
