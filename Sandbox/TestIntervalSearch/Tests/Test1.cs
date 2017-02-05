using System;
using System.Collections.Generic;

namespace TestIntervalSearch.Tests
{
    public sealed class Test1 : ITest
    {
        #region members

        private readonly List<Tuple<ushort, int, int, int>> _data;

        #endregion

        public Test1()
        {
            _data = GetData();
        }

        public void RunTest()
        {
            var intervalArray = Factories.CreateIntervalArray(_data);
            TestUtilities.GenericTester(intervalArray, _data, "IntervalArray");
        }

        public List<Tuple<ushort, int, int, int>> GetData()
        {
            return new List<Tuple<ushort, int, int, int>>
            {
                new Tuple<ushort, int, int, int>(22, 14, 17, 0),
                new Tuple<ushort, int, int, int>(22, 17, 21, 1),
                new Tuple<ushort, int, int, int>(22, 0, 5, 2),
                new Tuple<ushort, int, int, int>(22, 3, 3, 3),
                new Tuple<ushort, int, int, int>(22, 7, 10, 4),
                new Tuple<ushort, int, int, int>(22, 8, 11, 5),
                new Tuple<ushort, int, int, int>(22, 7, 13, 6),
                new Tuple<ushort, int, int, int>(22, 11, 16, 7)
            };
        }
    }
}
