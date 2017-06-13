using System;
using System.Collections.Generic;
using TestIntervalSearch.Tests;

namespace TestIntervalSearch
{
    public sealed class TestRunner
    {
        private readonly Dictionary<string, int> _referenceIndex;

        /// <summary>
        /// constructor
        /// </summary>
        private TestRunner()
        {
            _referenceIndex = new Dictionary<string, int>
            {
                ["1"]  = 0,
                ["2"]  = 1,
                ["3"]  = 2,
                ["4"]  = 3,
                ["5"]  = 4,
                ["6"]  = 5,
                ["7"]  = 6,
                ["8"]  = 7,
                ["9"]  = 8,
                ["10"] = 9,
                ["11"] = 10,
                ["12"] = 11,
                ["13"] = 12,
                ["14"] = 13,
                ["15"] = 14,
                ["16"] = 15,
                ["17"] = 16,
                ["18"] = 17,
                ["19"] = 18,
                ["20"] = 19,
                ["21"] = 20,
                ["22"] = 21,
                ["X"]  = 22,
                ["Y"]  = 23,
                ["MT"] = 24
            };
        }

        private void RunTests()
        {
            // define our test collection
            var tests = new List<ITest>
            {
                //new TestMemory(_referenceIndex),
                new Test1(),
                new Test2(),
                new Test3(),
                new TestSpeed(),
                new TestRandom()
            };

            for (int trialIndex = 0; trialIndex < 10; trialIndex++)
            {
                Console.WriteLine("=================");
                Console.WriteLine("Current trial: {0}", trialIndex + 1);
                Console.WriteLine("=================");
                Console.WriteLine();

                // run the test collection
                foreach (var test in tests) test.RunTest();
            }
        }

        static void Main()
        {
            var runner = new TestRunner();
            runner.RunTests();
        }
    }
}
