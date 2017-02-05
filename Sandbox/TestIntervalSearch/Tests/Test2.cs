using System;
using System.Collections.Generic;

namespace TestIntervalSearch.Tests
{
    public sealed class Test2 : ITest
    {
        #region members

        private readonly List<Tuple<ushort, int, int, int>> _data;

        #endregion

        public Test2()
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
                new Tuple<ushort, int, int, int>(22, 37, 38, 0),
                new Tuple<ushort, int, int, int>(22, 85, 88, 1),
                new Tuple<ushort, int, int, int>(22, 95, 95, 2),
                new Tuple<ushort, int, int, int>(22, 56, 61, 3),
                new Tuple<ushort, int, int, int>(22, 58, 58, 4),
                new Tuple<ushort, int, int, int>(22, 46, 46, 5),
                new Tuple<ushort, int, int, int>(22, 76, 76, 6),
                new Tuple<ushort, int, int, int>(22, 104, 105, 7),
                new Tuple<ushort, int, int, int>(22, 145, 146, 8),
                new Tuple<ushort, int, int, int>(22, 24, 26, 9),
                new Tuple<ushort, int, int, int>(22, 92, 95, 10),
                new Tuple<ushort, int, int, int>(22, 126, 126, 11),
                new Tuple<ushort, int, int, int>(22, 8, 14, 12),
                new Tuple<ushort, int, int, int>(22, 87, 92, 13),
                new Tuple<ushort, int, int, int>(22, 88, 88, 14),
                new Tuple<ushort, int, int, int>(22, 141, 141, 15),
                new Tuple<ushort, int, int, int>(22, 121, 126, 16),
                new Tuple<ushort, int, int, int>(22, 84, 87, 17),
                new Tuple<ushort, int, int, int>(22, 73, 78, 18),
                new Tuple<ushort, int, int, int>(22, 77, 83, 19),
                new Tuple<ushort, int, int, int>(22, 32, 36, 20),
                new Tuple<ushort, int, int, int>(22, 17, 21, 21),
                new Tuple<ushort, int, int, int>(22, 106, 106, 22),
                new Tuple<ushort, int, int, int>(22, 100, 104, 23),
                new Tuple<ushort, int, int, int>(22, 82, 83, 24),
                new Tuple<ushort, int, int, int>(22, 101, 103, 25),
                new Tuple<ushort, int, int, int>(22, 135, 140, 26),
                new Tuple<ushort, int, int, int>(22, 76, 81, 27),
                new Tuple<ushort, int, int, int>(22, 60, 63, 28),
                new Tuple<ushort, int, int, int>(22, 59, 63, 29),
                new Tuple<ushort, int, int, int>(22, 80, 83, 30),
                new Tuple<ushort, int, int, int>(22, 22, 23, 31),
                new Tuple<ushort, int, int, int>(22, 33, 38, 32),
                new Tuple<ushort, int, int, int>(22, 77, 78, 33),
                new Tuple<ushort, int, int, int>(22, 103, 104, 34),
                new Tuple<ushort, int, int, int>(22, 113, 115, 35),
                new Tuple<ushort, int, int, int>(22, 19, 19, 36),
                new Tuple<ushort, int, int, int>(22, 62, 63, 37),
                new Tuple<ushort, int, int, int>(22, 53, 59, 38),
                new Tuple<ushort, int, int, int>(22, 40, 46, 39),
                new Tuple<ushort, int, int, int>(22, 42, 44, 40),
                new Tuple<ushort, int, int, int>(22, 57, 61, 41),
                new Tuple<ushort, int, int, int>(22, 19, 25, 42),
                new Tuple<ushort, int, int, int>(22, 37, 42, 43),
                new Tuple<ushort, int, int, int>(22, 62, 67, 44),
                new Tuple<ushort, int, int, int>(22, 49, 53, 45),
                new Tuple<ushort, int, int, int>(22, 66, 71, 46),
                new Tuple<ushort, int, int, int>(22, 125, 131, 47),
                new Tuple<ushort, int, int, int>(22, 26, 28, 48),
                new Tuple<ushort, int, int, int>(22, 79, 82, 49)
            };
        }
    }
}
