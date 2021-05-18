using System;
using System.Collections.Generic;
using Genome;

namespace VariantAnnotation.Interface.SA
{
    public interface INsaReader : ISaMetadata, IDisposable
    {
        bool               MatchByAllele { get; }
        bool               IsArray       { get; }
        bool               IsPositional  { get; }

        void GetAnnotation(int position, List<(string refAllele, string altAllele, string annotation)> annotations);
        void PreLoad(IChromosome chrom, List<int> positions);
    }
}