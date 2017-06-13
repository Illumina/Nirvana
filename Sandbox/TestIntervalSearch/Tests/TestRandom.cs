using System;
using System.Collections.Generic;

namespace TestIntervalSearch.Tests
{
    public sealed class TestRandom : ITest
    {
        #region members

        private readonly IntervalParameters _intervalParameters;

        private const int NumElements      = 10000;
        private const int MaxNumIterations = 100;

        #endregion

        public TestRandom()
        {
            _intervalParameters = new IntervalParameters
            {
                MinCoordinate = 0,
                MaxCoordinate = 15000,
                MinLength     = 1,
                MaxLength     = 100
            };
        }

        public List<Tuple<ushort, int, int, int>> GetData()
        {
            return TestUtilities.GetRandomIntervals(NumElements, _intervalParameters);
        }

        public void RunTest()
        {
            Console.WriteLine("- Trying to break the interval data structures:");

            for (int iteration = 0; iteration < MaxNumIterations; iteration++)
            {
                var data = GetData();

                var intervalArray = Factories.CreateIntervalArray(data);
                TestUtilities.GenericTester(intervalArray, data, $"IntervalArray ({iteration + 1}/{MaxNumIterations})");
            }
        }
    }
}
