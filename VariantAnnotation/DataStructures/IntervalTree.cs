using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace VariantAnnotation.DataStructures
{
    /// <summary>
    /// Annotation interval tree based on the augmented interval tree described in
    /// Cormen et al. (2001, Section 14.3: Interval trees, pp. 311–317). In this case 
    /// using a high performance AVL tree with auxiliary parent pointers.
    /// </summary>
    /// <remarks>
    /// Both the Overlaps and GetAllOverlappingIntervals methods have been brute force 
    /// tested by the IntervalTreeTest project:
    /// $/Illumina.Bioinformatics/Dev/Trunk/mstromberg/MethodsDevelopment/IntervalTreeTest
    /// 
    /// 10,000 genomic annotations with an int32 payload use 1.6 MB RAM (160 bytes/annotation)
    /// 
    /// Michael Strömberg (2014-10-28)
    /// </remarks>
    public class IntervalTree<T> : IEnumerable<IntervalTree<T>.Node>
    {
        #region members

        private Node _root;
        private int _version;

        public bool IsEmpty => _root == null;

        #endregion

        // constructor
        public IntervalTree()
        {
            _root  = null;
        }

        // the interval tree is modeled after a typical collection, but doesn't
        // implement ICollection directly because Remove and Contains don't
        // exactly make sense.
        #region ICollection<T> pseudo-implementation

        /// <summary>
        /// Adds an item to the interval tree.
        /// </summary>
        /// TODO: CC 17
        public void Add(Interval key)
        {
            // A1. Initialization
            Node n = _root;
            Node p = null;
            Node q = null;

            // A2. Find insertion point
            if (n != null)
            {
                // look for a null pointer
                while (n != null)
                {
                    // update the max
                    n.StoreMax(key);

                    p = n;
                    if (p.Balance != 0) q = p;

                    int compare = key.CompareTo(n.Key);
                    if (compare < 0) n = n.Left;
                    else if (compare > 0) n = n.Right;
                    else
                    {
                        n.Key.Values.AddRange(key.Values);
                        unchecked { _version++; }
                        return;
                    }
                }
            }

            // A3. Insert
            unchecked { _version++; }
            n = new Node(key, p);

            if (p != null)
            {
                if (key.CompareTo(p.Key) < 0) p.Left = n;
                else p.Right = n;

                // A4. Adjust balance factors
                while (p != q)
                {
                    if (p.Left == n) p.Balance = -1;
                    else p.Balance = 1;

                    n = p;
                    p = p.Parent;
                }

                // A5. Check for imbalance
                if (q != null)
                {
                    if (q.Left == n)
                    {
                        --q.Balance;

                        if (q.Balance == -2)
                        {
                            // A6. Left imbalance
                            if (q.Left.Balance > 0) LeftRotate(q.Left);
                            RightRotate(q);
                        }
                    }

                    if (q.Right == n)
                    {
                        ++q.Balance;

                        if (q.Balance == 2)
                        {
                            // A7. Right imbalance
                            if (q.Right.Balance < 0) RightRotate(q.Right);
                            LeftRotate(q);
                        }
                    }
                }
            }
            else _root = n;

            // A8 All done.
        }

        /// <summary>
        /// Removes all items from the interval tree.
        /// </summary>
        public void Clear()
        {
            _root    = null;
            _version = 0;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<Node> GetEnumerator()
        {
            return new Enumerator(this);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>A IEnumerator that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        private struct Enumerator : IEnumerator<Node>
        {
            #region members

            private readonly IntervalTree<T> _intervalTree;
            private readonly int _version;
            private Node _current;
            private bool _resetEnumerator;

            #endregion

            // constructor
            internal Enumerator(IntervalTree<T> intervalTree)
            {
                _intervalTree    = intervalTree;
                _version         = intervalTree._version;
                _current         = null;
                _resetEnumerator = true;
            }

            /// <summary>
            /// Gets the element at the current position of the enumerator.
            /// </summary>
            public Node Current => _current;

            /// <summary>
            /// Gets the element at the current position of the enumerator.
            /// </summary>
            object IEnumerator.Current => Current;

            /// <summary>
            /// Releases all resources used by the interval tree Enumerator object.
            /// </summary>
            public void Dispose() {}

            /// <summary>
            /// Advances the enumerator to the next element of the interval tree.
            /// </summary>
            /// <returns>true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.</returns>
            public bool MoveNext()
            {
                if (_version != _intervalTree._version)
                    throw new InvalidOperationException("InvalidOperation_EnumFailedVersion");

                if (_resetEnumerator)
                {
                    _current = _intervalTree.GetFirstNode();
                    _resetEnumerator = false;
                }
                else
                {
                    _current = GetNextNode(_current);
                }

                return _current != null;
            }

            /// <summary>
            /// Sets the enumerator to its initial position, which is before the first element in the collection.
            /// </summary>
            void IEnumerator.Reset()
            {
                if (_version != _intervalTree._version)
                    throw new InvalidOperationException("InvalidOperation_EnumFailedVersion");
                
                _resetEnumerator = true;
                _current         = null;
            }
        }

        #endregion

        /// <summary>
        /// returns the first value in our interval tree
        /// </summary>
        private Node GetFirstNode()
        {
            return MinValue(_root);
        }

        /// <summary>
        /// returns the next node in the sorted binary tree
        /// </summary>
        private static Node GetNextNode(Node n)
        {
            // sanity check: handle null nodes
            if (n == null) return null;

            // if right subtree of node is not null, then the successor lies in the right subtree
            if (n.Right != null) return MinValue(n.Right);

            // if right subtree of the node is null, the successor is one of the ancestors
            Node p = n.Parent;

            while ((p != null) && (n == p.Right))
            {
                n = p;
                p = p.Parent;
            }

            return p;
        }

        /// <summary>
        /// search for all intervals which contain "key", starting with the node "n"
        /// and adding matching intervals to the list "result"
        /// </summary>
        private static void GetAllOverlappingIntervals(Node n, Interval key, List<T> result)
        {
            // sanity check: nothing to traverse
            if (n == null) return;

            // if key is to the right of the rightmost point of any interval
            // in this node and all children, there won't be any matches
            if (n.KeyStartsAfterSubtree(key)) return;

            // search left children
            if (n.Left != null) GetAllOverlappingIntervals(n.Left, key, result);

            // check this node
            if (n.Key.Overlaps(key)) result.AddRange(n.Key.Values);

            // if key is to the left of the start of this interval,
            // then it can't be in any child to the right
            if (n.KeyEndsBeforeNode(key)) return;

            // otherwise, search right children
            if (n.Right != null) GetAllOverlappingIntervals(n.Right, key, result);
        }

        /// <summary>
        /// returns IDs for all intervals that overlap the specified interval
        /// </summary>
        public void GetAllOverlappingValues(Interval key, List<T> values)
        {
            values.Clear();
            GetAllOverlappingIntervals(_root, key, values);
        }

        /// <summary>
        /// returns the minimum data value found given a specified root node
        /// </summary>
        private static Node MinValue(Node n)
        {
            Node currentNode = n;
            while (currentNode.Left != null) currentNode = currentNode.Left;
            return currentNode;
        }

        // rotates a given node left
        private void LeftRotate(Node n)
        {
            // L1. Do the rotation
            Node r = n.Right;
            n.Right = r.Left;

            if (r.Left != null) r.Left.Parent = n;

            Node p = n.Parent;
            r.Parent = p;

            if (p != null)
            {
                if (p.Left == n) p.Left = r;
                else p.Right = r;
            }
            else _root = r;

            r.Left = n;
            n.Parent = r;

            // L2. Recompute the balance factors
            n.Balance = n.Balance - (1 + Math.Max(r.Balance, 0));
            r.Balance = r.Balance - (1 - Math.Min(n.Balance, 0));

            // update the maximum
            r.Max = n.Max;
            r.MaxReference = n.MaxReference;

            n.Max = n.Key.End;
            n.MaxReference = n.Key.Reference;
            n.StoreMax(n.Left);
            n.StoreMax(n.Right);
        }

        // rotates a given node right
        private void RightRotate(Node n)
        {
            // R1. Do the rotation
            Node l = n.Left;
            n.Left = l.Right;

            if (l.Right != null) l.Right.Parent = n;

            Node p = n.Parent;
            l.Parent = p;

            if (p != null)
            {
                if (p.Left == n) p.Left = l;
                else p.Right = l;
            }
            else _root = l;

            l.Right = n;
            n.Parent = l;

            // R2. Recompute the balance factors
            n.Balance = n.Balance + (1 - Math.Min(l.Balance, 0));
            l.Balance = l.Balance + 1 + Math.Max(n.Balance, 0);

            // update the maximum
            l.Max = n.Max;
            l.MaxReference = n.MaxReference;

            n.Max = n.Key.End;
            n.MaxReference = n.Key.Reference;
            n.StoreMax(n.Left);
            n.StoreMax(n.Right);
        }

        /// <summary>
        /// Returns a string that represents the interval tree.
        /// </summary>
        /// <returns>A string that represents the interval tree.</returns>
        public override string ToString()
        {
            // nothing to traverse
            if (_root == null) return "(empty)";

            var sb = new StringBuilder();

            foreach (Node node in this)
            {
                sb.AppendFormat("{0}, max: {1} {2}", node.Key, node.MaxReference, node.Max);

                if (node.Parent != null)
                {
                    sb.AppendFormat(", parent: {0}", node.Parent.Key);
                    sb.AppendFormat(node.Parent.Left == node ? " LEFT" : " RIGHT");
                }

                if (node == _root) sb.AppendFormat(" ROOT");

                sb.AppendLine();
            }

            return sb.ToString();
        }

        // define our tree node
        public class Node
        {
            #region members

            public Interval Key;

            public Node Left;
            public Node Parent;
            public Node Right;

            public int Balance;
            public int Max;
            public string MaxReference;

            #endregion

            // constructor
            public Node(Interval key, Node parent)
            {
                Key = new Interval(key);

                Left         = null;
                Parent       = parent;
                Right        = null;
                Balance      = 0;
                Max          = key.End;
                MaxReference = key.Reference;
            }

            /// <summary>
            /// sets the maximum with respect to an interval
            /// </summary>
            internal void StoreMax(Interval interval)
            {
                if (((interval.Reference == MaxReference) && (interval.End > Max)) ||
                    (string.Compare(interval.Reference, MaxReference, StringComparison.Ordinal) > 0))
                {
                    MaxReference = interval.Reference;
                    Max = interval.End;
                }
            }

            /// <summary>
            /// sets the maximum with respect to a node
            /// </summary>
            internal void StoreMax(Node node)
            {
                if (node == null) return;
                if (((node.MaxReference == MaxReference) && (node.Max > Max)) ||
                    (string.Compare(node.MaxReference, MaxReference, StringComparison.Ordinal) > 0))
                {
                    MaxReference = node.MaxReference;
                    Max = node.Max;
                }
            }

            /// <summary>
            /// checks if the key interval ends before the current node
            /// </summary>
            internal bool KeyEndsBeforeNode(Interval keyInterval)
            {
                if ((keyInterval.Reference == Key.Reference) && (keyInterval.End < Key.Begin)) return true;
                return string.CompareOrdinal(keyInterval.Reference, Key.Reference) < 0;
            }

            /// <summary>
            /// checks if the key interval starts after the current subtree
            /// </summary>
            internal bool KeyStartsAfterSubtree(Interval keyInterval)
            {
                if ((keyInterval.Reference == MaxReference) && (keyInterval.Begin > Max)) return true;
                return string.CompareOrdinal(keyInterval.Reference, MaxReference) > 0;
            }
        }

        /// <summary>
        /// Our interval object. Uses a normal closed coordinates, none of this crap with
        /// half closed, half open that UCSC tries to impose.
        /// </summary>
        public struct Interval : IEquatable<Interval>
        {
            #region members

            public readonly int Begin;
            public readonly int End;
            public readonly List<T> Values;
            public readonly string Reference;

            private readonly int _hashCode;

            #endregion

            // constructor
            public Interval(string reference, int begin, int end, T value = default(T))
            {
                Reference = reference;
                Begin     = begin;
                End       = end;

                _hashCode = CalculateHashCode(Reference, Begin, End);
	            
                // add the value to the interval
                Values = new List<T> { value };
            }

            // copy constructor
            public Interval(Interval interval)
            {
                Reference = interval.Reference;
                Begin     = interval.Begin;
                End       = interval.End;
                Values    = new List<T>(interval.Values);

                _hashCode = CalculateHashCode(Reference, Begin, End);
            }

            /// <summary>
            /// implement our comparison function
            /// </summary>
            public int CompareTo(Interval interval)
            {
                if (Reference == interval.Reference)
                {
                    return Begin == interval.Begin ? End.CompareTo(interval.End) : Begin.CompareTo(interval.Begin);
                }

                return string.Compare(Reference, interval.Reference, StringComparison.Ordinal);
            }

            /// <summary>
            /// implement to string function
            /// </summary>
            public override string ToString()
            {
                var sb = new StringBuilder();
                sb.AppendFormat("{0}: {1} - {2}", Reference, Begin, End);
                sb.AppendFormat(" (values: {0})", string.Join(", ", Values[0]));
                return sb.ToString();
            }

            /// <summary>
            /// returns true if this interval overlaps with the specified interval
            /// </summary>
            public bool Overlaps(Interval readInterval)
            {
                if (Reference != readInterval.Reference) return false;
                return (End >= readInterval.Begin) && (Begin <= readInterval.End);
            }

            #region IEquatable methods

            public override bool Equals(object obj)
            {
                var other = (Interval)obj;
                return this == other;
            }

            /// <summary>
            /// Indicates whether the current object is equal to another object of the same type.
            /// </summary>
            /// <param name="other">An object to compare with this object.</param>
            /// <returns>true if the current object is equal to the other parameter; otherwise, false.</returns>
            public bool Equals(Interval other)
            {
                return this == other;
            }

            public static bool operator ==(Interval a, Interval b)
            {
                if (a._hashCode != b._hashCode) return false;

                if (a.Begin != b.Begin) return false;
                if (a.End != b.End) return false;
                if (a.Reference != b.Reference) return false;
                return true;

                // return (a.Begin     == b.Begin) &&
                //       (a.End       == b.End)   &&
                //       (a.Reference == b.Reference);
            }

            public static bool operator !=(Interval a, Interval b)
            {
                return !(a == b);
            }

            /// <summary>
            /// calculates the hash code based on the reference name and the begin/end coordinates
            /// </summary>
            private static int CalculateHashCode(string reference, int begin, int end)
            {
                int hashCode = begin;

                unchecked
                {
                    hashCode = (hashCode * 397) ^ end;
                    hashCode = (hashCode * 397) ^ reference.GetHashCode();
                }

                return hashCode;
            }

            /// <summary>
            /// Serves as the default hash function.
            /// </summary>
            /// <returns>A hash code for the current object.</returns>
            public override int GetHashCode()
            {
                return _hashCode;
            }

            #endregion
        }
    }
}