using System;
using System.Collections.Generic;

namespace SAUtils.DataStructures
{
    public sealed class MinHeap<T>
    {
        private readonly List<T> _itemArray;
        private readonly Func<T, T, int> _comparerFunc;

        
        public MinHeap(Func<T,T, int> comparerFunc)
        {
            _itemArray = new List<T>();
            _comparerFunc = comparerFunc;
        }

        public void Add(T item)
        {
            _itemArray.Add(item);
            Heapify();
        }

        private void Heapify()
        {
            var i = _itemArray.Count - 1;
            while (i > 0)
            {
                var j = i % 2 == 0 ? i / 2 - 1 : i / 2;//the index of the parent
                //if (_itemArray[i].CompareTo(_itemArray[j]) < 0)
                if (_comparerFunc(_itemArray[i], _itemArray[j]) < 0)
                    SwapItems(_itemArray, i, j);

                i = j;
            }
        }

        public T ExtractMin()
        {
            var min = _itemArray[0];

            // the last item form the array is brought to the root and pushed down to the appropriate position
            _itemArray[0] = _itemArray[_itemArray.Count - 1];
            _itemArray.RemoveAt(_itemArray.Count - 1);


            for (var i = 0; i < _itemArray.Count / 2;)
            {
                var j = 2 * i + 1;

                if (j + 1 < _itemArray.Count && _comparerFunc(_itemArray[j], _itemArray[j + 1]) > 0)
                    // both children are present
                    j++; //A[2*i+2] is the smaller child

                if (_comparerFunc(_itemArray[i],_itemArray[j]) > 0)
                    SwapItems(_itemArray, i, j);

                i = j;
            }
            return min;
        }

        private static void SwapItems(IList<T> list, int i, int j)
        {
            var temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }

        public T GetMin()
        {
            return _itemArray.Count == 0 ? default : _itemArray[0];
        }

        public int Count()
        {
            return _itemArray.Count;
        }

        public override string ToString()
        {
            return string.Join(",", _itemArray);

        }
    }
}