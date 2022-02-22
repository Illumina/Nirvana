using System.Collections.Generic;
using System.IO;
using Genome;
using VariantAnnotation.AnnotatedPositions;
using VariantAnnotation.PhyloP;
using Variants;
using Versioning;

namespace VariantAnnotation.Providers
{
    public sealed class ConservationScoreProvider : IAnnotationProvider
    {
        private readonly NpdReader _phylopReader;

        public string                          Name               => "Conservation score provider";
        public GenomeAssembly                  Assembly           => _phylopReader.Assembly;
        public IEnumerable<IDataSourceVersion> DataSourceVersions { get; }

        public ConservationScoreProvider(Stream dbStream, Stream indexStream)
        {
            _phylopReader      = new NpdReader(dbStream, indexStream);
            DataSourceVersions = new[] {_phylopReader.Version};
        }

        public void Annotate(AnnotatedPosition annotatedPosition)
        {
            foreach (var annotatedVariant in annotatedPosition.AnnotatedVariants)
            {
                if (annotatedVariant.Variant.Type != VariantType.SNV) continue;
                annotatedVariant.PhylopScore = _phylopReader.GetAnnotation(annotatedPosition.Position.Chromosome,
                    annotatedVariant.Variant.Start);
            }
        }
    }
}