using System.Collections.Generic;

namespace Phantom.PositionCollections
{
    public interface IPositionSet
    {
        AlleleSet AlleleSet { get; }
        Dictionary<AlleleBlock, List<SampleHaplotype>> AlleleBlockToSampleHaplotype { get; }
    }
}