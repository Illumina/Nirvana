using System;
using System.Collections.Generic;
using System.Linq;
using Illumina.VariantAnnotation.DataStructures;

namespace TestObjectIntervalTree
{
    public sealed class ObjectIntervalTreeTest<T>
    {
        #region members

        private readonly List<T> _overlappingObjects;
        private readonly List<T> _bruteOverlappingObjects;

        #endregion

        // constructor
        public ObjectIntervalTreeTest()
        {
            _overlappingObjects      = new List<T>();
            _bruteOverlappingObjects = new List<T>();
        }

        /// <summary>
        /// Compares the brute force overlaps (all overlaps) with the interval
        /// tree overlaps for the specified interval.
        /// </summary>
        private void FindAllOverlaps(IntervalTree<T> intervalTree, List<IntervalTree<T>.Interval> testIntervals, IntervalTree<T>.Interval interval)
        {
            // get the overlapping indices for this interval
            intervalTree.GetAllOverlappingValues(interval, _overlappingObjects);
            GetBruteForceOverlappingObjects(interval, testIntervals, _bruteOverlappingObjects);

            // compare the results
            if (ObjectsAreDifferent(_overlappingObjects, _bruteOverlappingObjects))
            {
                Console.WriteLine();
                Console.WriteLine("Error found:");
                Console.WriteLine();

                // dump the random interval
                Console.WriteLine("current interval: {0}", interval);
                Console.WriteLine();

                // intervalTree.GetAllOverlappingIndices(interval, ref overlappingIndices, OverlapBehavior.Any);

                // dump the master list
                Console.WriteLine("Dumping the master list:");
                foreach (var tempInterval in testIntervals) Console.WriteLine(tempInterval);
                Console.WriteLine();

                // dump the interval tree
                Console.WriteLine("Dumping interval tree:");
                Console.WriteLine(intervalTree);
                Console.WriteLine();

                // sort the index lists
                _overlappingObjects.Sort();
                _bruteOverlappingObjects.Sort();

                // display the faulty annotations
                Console.WriteLine("Overlapping indices found: ");
                foreach (T index in _overlappingObjects)
                {
                    Console.Write("{0} ", index);
                }
                Console.WriteLine("\n");

                Console.WriteLine("Overlapping indices found via brute force: ");
                foreach (T index in _bruteOverlappingObjects)
                {
                    Console.Write("{0} ", index);
                }
                Console.WriteLine("\n");
                Environment.Exit(1);
            }
        }

        ///// <summary>
        ///// Compares the interval tree overlap result to the brute force
        ///// overlap result given the specified interval
        ///// </summary>
        //private static void FindAnyOverlap(IntervalTree<T> intervalTree, List<IntervalTree<T>.Interval> testIntervals, IntervalTree<T>.Interval interval)
        //{
        //    // get the overlapping indices for this interval
        //    var overlaps = intervalTree.GetAnyOverlappingValue(interval) != null;
        //    bool bruteForceOverlaps = GetBruteForceOverlaps(interval, testIntervals);

        //    // compare the results
        //    if (overlaps != bruteForceOverlaps)
        //    {
        //        Console.WriteLine();
        //        Console.WriteLine("Error found:");
        //        Console.WriteLine();

        //        // dump the random interval
        //        Console.WriteLine("current interval: {0}", interval);
        //        Console.WriteLine();

        //        // dump the master list
        //        Console.WriteLine("Dumping the master list:");
        //        foreach (var tempInterval in testIntervals) Console.WriteLine(tempInterval);
        //        Console.WriteLine();

        //        // dump the interval tree
        //        Console.WriteLine("Dumping interval tree:");
        //        Console.WriteLine(intervalTree);
        //        Console.WriteLine();

        //        Environment.Exit(1);
        //    }
        //}

        /// <summary>
        /// adds the objects of all elements that overlap with random interval to a list
        /// </summary>
        private static void GetBruteForceOverlappingObjects(IntervalTree<T>.Interval randomInterval, IEnumerable<IntervalTree<T>.Interval> randomIntervals, List<T> bruteOverlappingObjects)
        {
            bruteOverlappingObjects.Clear();

            foreach (var interval in randomIntervals)
            {
                if (randomInterval.Overlaps(interval))
                {
                    bruteOverlappingObjects.AddRange(interval.Values);
                }
            }
        }

        /// <summary>
        /// adds the objects of all elements that overlap with random interval to a list
        /// </summary>
        private static bool GetBruteForceOverlaps(IntervalTree<T>.Interval randomInterval, IEnumerable<IntervalTree<T>.Interval> randomIntervals)
        {
            return randomIntervals.Any(interval => interval.Overlaps(randomInterval));
        }

        /// <summary>
        /// returns true if the objects in the two lists are different
        /// </summary>
        private static bool ObjectsAreDifferent(List<T> treeObjects, List<T> bruteOverlappingObjects)
        {
            // some sanity checks
            if (treeObjects == null || bruteOverlappingObjects == null) return true;
            if (treeObjects.Count != bruteOverlappingObjects.Count) return true;

            // sort the lists
            treeObjects.Sort();
            bruteOverlappingObjects.Sort();

            return treeObjects.Where((t, index) => !EqualityComparer<T>.Default.Equals(t, bruteOverlappingObjects[index])).Any();
        }

        /// <summary>
        /// runs the interval tree tests
        /// </summary>
        public void RunTest(List<IntervalTree<T>.Interval> testIntervals, string description, bool findAllOverlaps)
        {
            Console.WriteLine("Running {0} ({1}, {2}):", description, findAllOverlaps ? "all overlaps" : "any overlap",
                "partial overlap");

            // populate the interval tree
            Console.Write("  - adding intervals... ");
            var intervalTree = new IntervalTree<T>();
            foreach (var interval in testIntervals) intervalTree.Add(interval);
            Console.WriteLine("{0} intervals added.", testIntervals.Count);

            Console.Write("  - checking intervals... ");

            // check that all of our intervals are in the tree
            ulong numIntervalsChecked = 0;
            foreach (var interval in testIntervals)
            {
                // Console.WriteLine("interval {0}", numIntervalsChecked + 1);
                if (findAllOverlaps) FindAllOverlaps(intervalTree, testIntervals, interval);
                //else FindAnyOverlap(intervalTree, testIntervals, interval);

                numIntervalsChecked++;
            }

            Console.WriteLine("{0} intervals checked.", numIntervalsChecked);
            Console.WriteLine();
        }
    }
}
