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

        void  GetAnnotation(int position, List<(string refAllele, string altAllele, string annotation)> annotations);
        void PreLoad(IChromosome chrom, List<int> positions);
    }
}