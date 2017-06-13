using System;
using System.Collections.Generic;

namespace TestIntervalSearch.Tests
{
    public sealed class Test3 : ITest
    {
        #region members

        private readonly List<Tuple<ushort, int, int, int>> _data;

        #endregion

        public Test3()
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
                new Tuple<ushort, int, int, int>(22, 28, 34,0),
                new Tuple<ushort, int, int, int>(22, 104, 106, 1),
                new Tuple<ushort, int, int, int>(22, 124, 127, 2),
                new Tuple<ushort, int, int, int>(22, 118, 122, 3),
                new Tuple<ushort, int, int, int>(22, 104, 105, 4),
                new Tuple<ushort, int, int, int>(22, 35, 40, 5),
                new Tuple<ushort, int, int, int>(22, 45, 48, 6),
                new Tuple<ushort, int, int, int>(22, 133, 137, 7),
                new Tuple<ushort, int, int, int>(22, 82, 82, 8),
                new Tuple<ushort, int, int, int>(22, 97, 102, 9),
                new Tuple<ushort, int, int, int>(22, 35, 38, 10),
                new Tuple<ushort, int, int, int>(22, 49, 50, 11),
                new Tuple<ushort, int, int, int>(22, 98, 99, 12),
                new Tuple<ushort, int, int, int>(22, 137, 143, 13),
                new Tuple<ushort, int, int, int>(22, 50, 56, 14),
                new Tuple<ushort, int, int, int>(22, 120, 120, 15),
                new Tuple<ushort, int, int, int>(22, 140, 140, 16),
                new Tuple<ushort, int, int, int>(22, 50, 52, 17),
                new Tuple<ushort, int, int, int>(22, 14, 20, 18),
                new Tuple<ushort, int, int, int>(22, 100, 104, 19),
                new Tuple<ushort, int, int, int>(22, 97, 97, 20),
                new Tuple<ushort, int, int, int>(22, 16, 18, 21),
                new Tuple<ushort, int, int, int>(22, 14, 14, 22),
                new Tuple<ushort, int, int, int>(22, 149, 154, 23),
                new Tuple<ushort, int, int, int>(22, 51, 51, 24),
                new Tuple<ushort, int, int, int>(22, 63, 64, 25),
                new Tuple<ushort, int, int, int>(22, 138, 144, 26),
                new Tuple<ushort, int, int, int>(22, 120, 124, 27),
                new Tuple<ushort, int, int, int>(22, 86, 89, 28),
                new Tuple<ushort, int, int, int>(22, 115, 119, 29),
                new Tuple<ushort, int, int, int>(22, 82, 87, 30),
                new Tuple<ushort, int, int, int>(22, 54, 59, 31),
                new Tuple<ushort, int, int, int>(22, 70, 71, 32),
                new Tuple<ushort, int, int, int>(22, 136, 137, 33),
                new Tuple<ushort, int, int, int>(22, 23, 26, 34),
                new Tuple<ushort, int, int, int>(22, 92, 96, 35),
                new Tuple<ushort, int, int, int>(22, 67, 73, 36),
                new Tuple<ushort, int, int, int>(22, 29, 30, 37),
                new Tuple<ushort, int, int, int>(22, 58, 58, 38),
                new Tuple<ushort, int, int, int>(22, 11, 15, 39),
                new Tuple<ushort, int, int, int>(22, 35, 36, 40),
                new Tuple<ushort, int, int, int>(22, 92, 93, 41),
                new Tuple<ushort, int, int, int>(22, 79, 83, 42),
                new Tuple<ushort, int, int, int>(22, 21, 27, 43),
                new Tuple<ushort, int, int, int>(22, 46, 50, 44),
                new Tuple<ushort, int, int, int>(22, 117, 120, 45),
                new Tuple<ushort, int, int, int>(22, 17, 17, 46),
                new Tuple<ushort, int, int, int>(22, 18, 23, 47),
                new Tuple<ushort, int, int, int>(22, 108, 110, 48),
                new Tuple<ushort, int, int, int>(22, 126, 132, 49)
            };
        }
    }
}
