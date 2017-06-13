using System;
using System.Collections.Generic;

namespace TestIntervalSearch.Tests
{
    public interface ITest
    {
        List<Tuple<ushort, int, int, int>> GetData();
        void RunTest();
    }
}
