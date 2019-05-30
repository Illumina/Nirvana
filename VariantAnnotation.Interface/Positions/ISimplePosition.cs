using System.Collections.Generic;
using Genome;

namespace VariantAnnotation.Interface.Positions
{
    public interface ISimplePosition : IChromosomeInterval
    {
        string RefAllele { get; }
        string[] AltAlleles { get; }
        string[] VcfFields { get; }
        bool[] IsDecomposed { get; }
        bool IsRecomposed { get; }
        string[] Vids { get; }
        List<string>[] LinkedVids { get; }
    }
}