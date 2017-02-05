using System;
using System.Collections.Generic;
using System.Linq;
using VariantAnnotation.Interface;
using VariantAnnotation.Utilities;

namespace TestIntervalSearch
{
    public static class TestUtilities
    {
        private static readonly Random Rnd = new Random();

        /// <summary>
        /// runs the interval array tests
        /// </summary>
        public static void GenericTester(IIntervalForest<int> intervalForest,
            List<Tuple<ushort, int, int, int>> intervals, string description)
        {
            Console.WriteLine($"Running {description}:");
            Console.Write("  - checking intervals... ");

            ulong numIntervalsChecked = 0;
            foreach (var interval in intervals)
            {
                FindAllOverlaps(intervalForest, intervals, interval);
                numIntervalsChecked++;
            }

            Console.WriteLine($"{numIntervalsChecked} intervals checked.");
            Console.WriteLine();
        }

        public static void SpeedTester(IIntervalForest<int> intervalForest, List<Tuple<ushort, int, int, int>> intervals,
            string description)
        {
            Console.WriteLine($"Running {description}:");
            Console.Write("  - checking speed... ");

            var bench = new Benchmark();

            int numOverlaps = 0;
            foreach (var interval in intervals)
            {
                var overlappingObjects = new List<int>();
                intervalForest.GetAllOverlappingValues(interval.Item1, interval.Item2, interval.Item3, overlappingObjects);

                numOverlaps++;
            }

            double ups;
            Console.WriteLine(bench.GetElapsedIterationTime(numOverlaps, "interval", out ups));
            Console.WriteLine();
        }

        /// <summary>
        /// Compares the brute force overlaps (all overlaps) with the interval
        /// array overlaps for the specified interval.
        /// </summary>
        private static void FindAllOverlaps(IIntervalForest<int> intervalForest,
            List<Tuple<ushort, int, int, int>> testIntervals, Tuple<ushort, int, int, int> interval)
        {
            // get the overlapping indices for this interval
            var overlappingObjects = new List<int>();
            intervalForest.GetAllOverlappingValues(interval.Item1, interval.Item2, interval.Item3, overlappingObjects);

            var bruteOverlappingObjects = new List<int>();
            GetBruteForceInts(testIntervals, interval, bruteOverlappingObjects);

            // compare the results
            if (!ObjectsAreDifferent(overlappingObjects, bruteOverlappingObjects)) return;

            Console.WriteLine();
            Console.WriteLine("Error found:");
            Console.WriteLine();

            // dump the random interval
            Console.WriteLine("current interval: {0}", interval);
            Console.WriteLine();

            // dump the master list
            Console.WriteLine("Dumping the master list:");
            foreach (var tempInterval in testIntervals) Console.WriteLine(tempInterval);
            Console.WriteLine();

            // dump the interval array
            Console.WriteLine("Dumping interval array:");
            Console.WriteLine(intervalForest);
            Console.WriteLine();

            // sort the index lists
            overlappingObjects.Sort();
            bruteOverlappingObjects.Sort();

            // display the faulty annotations
            Console.WriteLine("Overlapping indices found: ");
            foreach (var index in overlappingObjects)
            {
                Console.Write("{0} ", index);
            }
            Console.WriteLine("\n");

            Console.WriteLine("Overlapping indices found via brute force: ");
            foreach (var index in bruteOverlappingObjects)
            {
                Console.Write("{0} ", index);
            }
            Console.WriteLine("\n");
            Environment.Exit(1);
        }

        /// <summary>
        /// adds the IDs of all elements that overlap with random interval to a list
        /// </summary>
        private static void GetBruteForceInts(List<Tuple<ushort, int, int, int>> testIntervals,
            Tuple<ushort, int, int, int> interval, List<int> bruteForceInts)
        {
            bruteForceInts.Clear();
            bruteForceInts.AddRange(from testInterval in testIntervals
                where Overlaps(testInterval, interval)
                select testInterval.Item4);
        }

        private static bool Overlaps(Tuple<ushort, int, int, int> a, Tuple<ushort, int, int, int> b)
        {
            if (a.Item1 != b.Item1) return false;
            return (a.Item3 >= b.Item2) && (a.Item2 <= b.Item3);
        }

        /// <summary>
        /// returns true if the objects in the two lists are different
        /// </summary>
        private static bool ObjectsAreDifferent(List<int> intervalSearchInts, List<int> bruteForceInts)
        {
            // some sanity checks
            if (intervalSearchInts == null || bruteForceInts == null) return true;
            if (intervalSearchInts.Count != bruteForceInts.Count) return true;

            // sort the lists
            intervalSearchInts.Sort();
            bruteForceInts.Sort();

            return intervalSearchInts.Where((t, i) => t != bruteForceInts[i]).Any();
        }

        public static List<Tuple<ushort, int, int, int>> GetRandomIntervals(int numElements,
            IntervalParameters ip)
        {
            var randomIntervals = new List<Tuple<ushort, int, int, int>>(numElements);
            var observedIntervals = new HashSet<Tuple<ushort, int, int, int>>();
            int currentIndex = 0;

            while (randomIntervals.Count < numElements)
            {
                var randomInterval = CreateRandomInterval(currentIndex, ip);
                if (observedIntervals.Contains(randomInterval)) continue;

                observedIntervals.Add(randomInterval);
                randomIntervals.Add(randomInterval);
                ++currentIndex;
            }

            return randomIntervals;
        }

        /// <summary>
        /// creates a random interval given the specified interval parameters
        /// </summary>
        private static Tuple<ushort, int, int, int> CreateRandomInterval(int index, IntervalParameters ip)
        {
            int begin       = Rnd.Next(ip.MinCoordinate, ip.MaxCoordinate);
            int length      = Rnd.Next(ip.MinLength, ip.MaxLength);
            int end         = begin + length - 1;
            ushort refIndex = (ushort)Rnd.Next(0, Factories.NumRefSeqs);

            return new Tuple<ushort, int, int, int>(refIndex, begin, end, index);
        }
    }
}
