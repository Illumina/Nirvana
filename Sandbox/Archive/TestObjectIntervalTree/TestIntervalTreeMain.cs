using System;
using System.Collections.Generic;
using Illumina.VariantAnnotation.DataStructures;

namespace TestObjectIntervalTree
{
    static class IntervalTreeTestMain
    {
        #region members
        private static readonly Random Rnd = new Random();
        #endregion

        static void Main()
        {
            const int numTrials = 10;

            for (int trialIndex = 0; trialIndex < numTrials; trialIndex++)
            {
                Console.WriteLine("=================");
                Console.WriteLine("Current trial: {0}", trialIndex + 1);
                Console.WriteLine("=================");
                Console.WriteLine();

                // ==========================
                // check for partial overlaps
                // ==========================

                // test for any overlap
                Test1(false);
                Test2(false);
                Test3(false);
                RandomTests(false, false);

                // test for all overlaps
                Test1(true);
                Test2(true);
                Test3(true);
                RandomTests(true, false);

                // ========================
                // check for exact overlaps
                // ========================

                // test for any overlap
                RandomTests(false, true);

                // test for all overlaps
                RandomTests(true, true);
            }

            // test memory usage
            TestMemoryUsage();
        }

        /// <summary>
        /// tests the memory usage of an interval tree
        /// </summary>
        private static void TestMemoryUsage()
        {
            // initialize
            const int numElements = 10000;

            // initialize our interval parameters
            var intervalParameters = new IntervalParameters
            {
                MinCoordinate = 0,
                MaxCoordinate = 15000,
                MinLength = 1,
                MaxLength = 100
            };

            // capture the baseline memory usage
            long baselineUsage = GC.GetTotalMemory(true);

            // randomly create some intervals
            List<IntervalTree<int>.Interval> testIntervals = CreateRandomIntervals(intervalParameters, numElements);
            long intervalCreationUsage = GC.GetTotalMemory(true);

            // build the interval tree
            var intervalTree = new IntervalTree<int>();
            foreach (var interval in testIntervals) intervalTree.Add(interval);
            long intervalTreeUsage = GC.GetTotalMemory(true);

            var objects = new List<int>();
            intervalTree.GetAllOverlappingValues(new IntervalTree<int>.Interval("chr1", 10, 100), objects);

            long intervalTreeDifference = intervalTreeUsage - intervalCreationUsage;

            Console.WriteLine("baseline:            {0}", baselineUsage);
            Console.WriteLine("interval creation:   {0}", intervalCreationUsage);
            Console.WriteLine("interval tree:       {0}", intervalTreeUsage);
            Console.WriteLine("interval tree usage: {0} bytes for {1} entries", intervalTreeDifference, numElements);

            double bytesPerElement = intervalTreeDifference / (double)numElements;
            Console.WriteLine("bytes/element:       {0:0.0}", bytesPerElement);
        }

        /// <summary>
        /// our attempt to break the interval tree using data set 1
        /// </summary>
        private static void Test1(bool findAllOverlaps)
        {
            // create some intervals and populate the interval tree
            var caseIntervals = new List<IntervalTree<int>.Interval>
            {
                new IntervalTree<int>.Interval("X", 14, 17), 
                new IntervalTree<int>.Interval("X", 17, 21, 1), 
                new IntervalTree<int>.Interval("X",  0,  5, 2),
                new IntervalTree<int>.Interval("X",  3,  3, 3), 
                new IntervalTree<int>.Interval("X",  7, 10, 4), 
                new IntervalTree<int>.Interval("X",  8, 11, 5),
                new IntervalTree<int>.Interval("X",  7, 13, 6), 
                new IntervalTree<int>.Interval("X", 11, 16, 7)
            };

            var intervalTreeTest = new ObjectIntervalTreeTest<int>();
            intervalTreeTest.RunTest(caseIntervals, "Test 1", findAllOverlaps);
        }

        /// <summary>
        /// our attempt to break the interval tree using data set 2
        /// </summary>
        private static void Test2(bool findAllOverlaps)
        {
            var intervalTreeTest = new ObjectIntervalTreeTest<int>();
            intervalTreeTest.RunTest(CreateTestIntervals2(), "Test 2", findAllOverlaps);
        }

        /// <summary>
        /// our attempt to break the interval tree using data set 3
        /// </summary>
        private static void Test3(bool findAllOverlaps)
        {
            var intervalTreeTest = new ObjectIntervalTreeTest<int>();
            intervalTreeTest.RunTest(CreateTestIntervals(), "Test 3", findAllOverlaps);
        }

        /// <summary>
        /// our attempt to break the interval tree
        /// </summary>
        private static void RandomTests(bool findAllOverlaps, bool useExactOverlap)
        {
            // initialize
            const int numElements = 10000;
            const int maxNumIterations = 100;

            // initialize our interval parameters
            IntervalParameters intervalParameters;

            if (useExactOverlap)
            {
                intervalParameters = new IntervalParameters
                {
                    MinCoordinate = 0,
                    MaxCoordinate = 200,
                    MinLength     = 1,
                    MaxLength     = 5
                };
            }
            else
            {
                intervalParameters = new IntervalParameters
                {
                    MinCoordinate = 0,
                    MaxCoordinate = 15000,
                    MinLength     = 1,
                    MaxLength     = 100
                };
            }

            Console.WriteLine("- Trying to break the interval tree:");

            // keep looping until we find a bad set of annotations
            for (int currentIteration = 0; currentIteration < maxNumIterations; currentIteration++)
            {
                // randomly create some intervals and populate the interval tree
                List<IntervalTree<int>.Interval> randomIntervals = CreateRandomIntervals(intervalParameters, numElements);

                var intervalTreeTest = new ObjectIntervalTreeTest<int>();
                intervalTreeTest.RunTest(randomIntervals, $"random iteration {currentIteration + 1}/{maxNumIterations}", findAllOverlaps);
            }
        }

        /// <summary>
        /// creates a list of random intervals
        /// </summary>
        private static List<IntervalTree<int>.Interval> CreateRandomIntervals(IntervalParameters intervalParameters, int numElements)
        {
            var randomIntervals = new List<IntervalTree<int>.Interval>(numElements);
            var observedIntervals = new HashSet<IntervalTree<int>.Interval>();
            int currentIndex = 0;

            while (randomIntervals.Count < numElements)
            {
                var randomInterval = CreateRandomInterval(intervalParameters, currentIndex);
                if (observedIntervals.Contains(randomInterval)) continue;

                observedIntervals.Add(randomInterval);
                randomIntervals.Add(randomInterval);
                ++currentIndex;
            }

            return randomIntervals;
        }

        private static List<IntervalTree<int>.Interval> CreateTestIntervals()
        {
            var testIntervals = new List<IntervalTree<int>.Interval>
            {
                new IntervalTree<int>.Interval("X", 28, 34),
                new IntervalTree<int>.Interval("X", 104, 106, 1),
                new IntervalTree<int>.Interval("X", 124, 127, 2),
                new IntervalTree<int>.Interval("X", 118, 122, 3),
                new IntervalTree<int>.Interval("X", 104, 105, 4),
                new IntervalTree<int>.Interval("X", 35, 40, 5),
                new IntervalTree<int>.Interval("X", 45, 48, 6),
                new IntervalTree<int>.Interval("X", 133, 137, 7),
                new IntervalTree<int>.Interval("X", 82, 82, 8),
                new IntervalTree<int>.Interval("X", 97, 102, 9),
                new IntervalTree<int>.Interval("X", 35, 38, 10),
                new IntervalTree<int>.Interval("X", 49, 50, 11),
                new IntervalTree<int>.Interval("X", 98, 99, 12),
                new IntervalTree<int>.Interval("X", 137, 143, 13),
                new IntervalTree<int>.Interval("X", 50, 56, 14),
                new IntervalTree<int>.Interval("X", 120, 120, 15),
                new IntervalTree<int>.Interval("X", 140, 140, 16),
                new IntervalTree<int>.Interval("X", 50, 52, 17),
                new IntervalTree<int>.Interval("X", 14, 20, 18),
                new IntervalTree<int>.Interval("X", 100, 104, 19),
                new IntervalTree<int>.Interval("X", 97, 97, 20),
                new IntervalTree<int>.Interval("X", 16, 18, 21),
                new IntervalTree<int>.Interval("X", 14, 14, 22),
                new IntervalTree<int>.Interval("X", 149, 154, 23),
                new IntervalTree<int>.Interval("X", 51, 51, 24),
                new IntervalTree<int>.Interval("X", 63, 64, 25),
                new IntervalTree<int>.Interval("X", 138, 144, 26),
                new IntervalTree<int>.Interval("X", 120, 124, 27),
                new IntervalTree<int>.Interval("X", 86, 89, 28),
                new IntervalTree<int>.Interval("X", 115, 119, 29),
                new IntervalTree<int>.Interval("X", 82, 87, 30),
                new IntervalTree<int>.Interval("X", 54, 59, 31),
                new IntervalTree<int>.Interval("X", 70, 71, 32),
                new IntervalTree<int>.Interval("X", 136, 137, 33),
                new IntervalTree<int>.Interval("X", 23, 26, 34),
                new IntervalTree<int>.Interval("X", 92, 96, 35),
                new IntervalTree<int>.Interval("X", 67, 73, 36),
                new IntervalTree<int>.Interval("X", 29, 30, 37),
                new IntervalTree<int>.Interval("X", 58, 58, 38),
                new IntervalTree<int>.Interval("X", 11, 15, 39),
                new IntervalTree<int>.Interval("X", 35, 36, 40),
                new IntervalTree<int>.Interval("X", 92, 93, 41),
                new IntervalTree<int>.Interval("X", 79, 83, 42),
                new IntervalTree<int>.Interval("X", 21, 27, 43),
                new IntervalTree<int>.Interval("X", 46, 50, 44),
                new IntervalTree<int>.Interval("X", 117, 120, 45),
                new IntervalTree<int>.Interval("X", 17, 17, 46),
                new IntervalTree<int>.Interval("X", 18, 23, 47),
                new IntervalTree<int>.Interval("X", 108, 110, 48),
                new IntervalTree<int>.Interval("X", 126, 132, 49)
            };

            return testIntervals;
        }

        private static List<IntervalTree<int>.Interval> CreateTestIntervals2()
        {
            var testIntervals = new List<IntervalTree<int>.Interval>
            {
                new IntervalTree<int>.Interval("X", 37, 38),
                new IntervalTree<int>.Interval("X", 85, 88, 1),
                new IntervalTree<int>.Interval("X", 95, 95, 2),
                new IntervalTree<int>.Interval("X", 56, 61, 3),
                new IntervalTree<int>.Interval("X", 58, 58, 4),
                new IntervalTree<int>.Interval("X", 46, 46, 5),
                new IntervalTree<int>.Interval("X", 76, 76, 6),
                new IntervalTree<int>.Interval("X", 104, 105, 7),
                new IntervalTree<int>.Interval("X", 145, 146, 8),
                new IntervalTree<int>.Interval("X", 24, 26, 9),
                new IntervalTree<int>.Interval("X", 92, 95, 10),
                new IntervalTree<int>.Interval("X", 126, 126, 11),
                new IntervalTree<int>.Interval("X", 8, 14, 12),
                new IntervalTree<int>.Interval("X", 87, 92, 13),
                new IntervalTree<int>.Interval("X", 88, 88, 14),
                new IntervalTree<int>.Interval("X", 141, 141, 15),
                new IntervalTree<int>.Interval("X", 121, 126, 16),
                new IntervalTree<int>.Interval("X", 84, 87, 17),
                new IntervalTree<int>.Interval("X", 73, 78, 18),
                new IntervalTree<int>.Interval("X", 77, 83, 19),
                new IntervalTree<int>.Interval("X", 32, 36, 20),
                new IntervalTree<int>.Interval("X", 17, 21, 21),
                new IntervalTree<int>.Interval("X", 106, 106, 22),
                new IntervalTree<int>.Interval("X", 100, 104, 23),
                new IntervalTree<int>.Interval("X", 82, 83, 24),
                new IntervalTree<int>.Interval("X", 101, 103, 25),
                new IntervalTree<int>.Interval("X", 135, 140, 26),
                new IntervalTree<int>.Interval("X", 76, 81, 27),
                new IntervalTree<int>.Interval("X", 60, 63, 28),
                new IntervalTree<int>.Interval("X", 59, 63, 29),
                new IntervalTree<int>.Interval("X", 80, 83, 30),
                new IntervalTree<int>.Interval("X", 22, 23, 31),
                new IntervalTree<int>.Interval("X", 33, 38, 32),
                new IntervalTree<int>.Interval("X", 77, 78, 33),
                new IntervalTree<int>.Interval("X", 103, 104, 34),
                new IntervalTree<int>.Interval("X", 113, 115, 35),
                new IntervalTree<int>.Interval("X", 19, 19, 36),
                new IntervalTree<int>.Interval("X", 62, 63, 37),
                new IntervalTree<int>.Interval("X", 53, 59, 38),
                new IntervalTree<int>.Interval("X", 40, 46, 39),
                new IntervalTree<int>.Interval("X", 42, 44, 40),
                new IntervalTree<int>.Interval("X", 57, 61, 41),
                new IntervalTree<int>.Interval("X", 19, 25, 42),
                new IntervalTree<int>.Interval("X", 37, 42, 43),
                new IntervalTree<int>.Interval("X", 62, 67, 44),
                new IntervalTree<int>.Interval("X", 49, 53, 45),
                new IntervalTree<int>.Interval("X", 66, 71, 46),
                new IntervalTree<int>.Interval("X", 125, 131, 47),
                new IntervalTree<int>.Interval("X", 26, 28, 48),
                new IntervalTree<int>.Interval("X", 79, 82, 49)
            };

            return testIntervals;
        }

        /// <summary>
        /// creates a random interval given the specified interval parameters
        /// </summary>
        private static IntervalTree<int>.Interval CreateRandomInterval(IntervalParameters intervalParameters, int index)
        {
            string[] references =
            {
                "chr1",
                "chr2",
                "chr3",
                "chr4",
                "chr5",
                "chr6",
                "chr7",
                "chr8",
                "chr9",
                "chr10",
                "chr11",
                "chr12",
                "chr13",
                "chr14",
                "chr15",
                "chr16",
                "chr17",
                "chr18",
                "chr19",
                "chr20",
                "chr21",
                "chr22",
                "chrX",
                "chrY",
                "chrM"
            };

            int begin = Rnd.Next(intervalParameters.MinCoordinate, intervalParameters.MaxCoordinate);
            int length = Rnd.Next(intervalParameters.MinLength, intervalParameters.MaxLength);
            int end = begin + length - 1;
            int refIndex = Rnd.Next(0, references.Length);

            return new IntervalTree<int>.Interval(references[refIndex], begin, end, index);
        }
    }

    sealed class IntervalParameters
    {
        public int MinCoordinate;
        public int MaxCoordinate;
        public int MinLength;
        public int MaxLength;
    }
}
