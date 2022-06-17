using System.Collections.Generic;
using OptimizedCore;
using SAUtils.DataStructures;
using Xunit;

namespace UnitTests.SAUtils.DataStructures;

public sealed class CounterDictionaryTests
{
    [Fact]
    public void TestCounterDictionary()
    {
        var inputData = new[]
        {
            "A", "B", "A", "A", "C", "B"
        };

        var counterDict = new CounterDictionary<string>();
        foreach (string keys in inputData)
        {
            counterDict.Add(keys);
        }
        
        Assert.Equal<uint>(6, counterDict.Total);
        
        Assert.Equal<uint>(3, counterDict["A"]);
        Assert.Equal<uint>(2, counterDict["B"]);
        Assert.Equal<uint>(1, counterDict["C"]);
        Assert.Equal<uint>(0, counterDict.GetValueOrDefault<string, uint>("NOT THERE", 0));

        var sb = StringBuilderPool.Get();
        counterDict.SerializeJson(sb);
        
        Assert.Equal("{\"count\":6,\"A\":3,\"B\":2,\"C\":1}", sb.ToString());
    }
}