using System;
using System.Collections.Generic;
using Genome;
using VariantAnnotation.Interface.Providers;

namespace VariantAnnotation.Interface.SA
{
    public interface INsaReader:IDisposable
    {
        GenomeAssembly Assembly { get; }
        IDataSourceVersion Version { get; }
        string JsonKey { get; }
        bool MatchByAllele { get; }
        bool IsArray { get; }
        bool IsPositional { get; }

        IEnumerable<(string refAllele, string altAllele, string annotation)> GetAnnotation(int position);
        void PreLoad(IChromosome chrom, List<int> positions);
    }
}