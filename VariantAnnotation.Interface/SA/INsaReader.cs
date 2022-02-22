using System.Collections.Generic;
using Genome;
using Versioning;

namespace VariantAnnotation.Interface.SA
{
    public interface INsaReader
    {
        GenomeAssembly Assembly { get; }
        IDataSourceVersion Version { get; }
        string JsonKey { get; }
        bool MatchByAllele { get; }
        bool IsArray { get; }
        bool IsPositional { get; }

        IEnumerable<(string refAllele, string altAllele, string annotation)> GetAnnotation(Chromosome chrom, int position);
        void PreLoad(Chromosome chrom, List<int> positions);
    }
}