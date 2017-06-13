using System;
using System.Collections.Generic;

namespace TestIntervalSearch.Tests
{
    public sealed class TestSpeed : ITest
    {
        #region members

        private readonly IntervalParameters _intervalParameters;
        private const int NumElements = 100000;

        #endregion

        public TestSpeed()
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
            Console.WriteLine("- Timing overlap performance:");

            var data = GetData();

            var intervalArray = Factories.CreateIntervalArray(data);
            TestUtilities.SpeedTester(intervalArray, data, "IntervalArray");
        }
    }
}
