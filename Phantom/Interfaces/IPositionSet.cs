using System.Collections.Generic;
using Phantom.DataStructures;

namespace Phantom.Interfaces
{
    public interface IPositionSet
    {
        AlleleSet AlleleSet { get; }
        Dictionary<AlleleIndexBlock, List<SampleAllele>> AlleleIndexBlockToSampleIndex { get; }
    }
}