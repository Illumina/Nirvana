using System;
using System.Collections.Generic;
using System.IO;
using Genome;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.PhyloP;
using Variants;

namespace VariantAnnotation.Providers
{
    public sealed class ConservationScoreProvider : IAnnotationProvider
    {
        private readonly NpdReader _phylopReader;

        public string Name { get; }
        public GenomeAssembly Assembly => _phylopReader.Assembly;
        public IEnumerable<IDataSourceVersion> DataSourceVersions { get; }

        public ConservationScoreProvider(Stream dbStream, Stream indexStream)
        {
            _phylopReader = new NpdReader(dbStream, indexStream);
            Name = "Conservation score provider";
            DataSourceVersions = new[] { _phylopReader.Version };
        }

        public void Annotate(IAnnotatedPosition annotatedPosition)
        {
            foreach (var annotatedVariant in annotatedPosition.AnnotatedVariants)
            {
                if (annotatedVariant.Variant.Type != VariantType.SNV) continue;
                annotatedVariant.PhylopScore = _phylopReader.GetAnnotation(annotatedPosition.Position.Chromosome, annotatedVariant.Variant.Start);
            }
        }

        public void PreLoad(IChromosome chromosome, List<int> positions)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _phylopReader?.Dispose();
        }
    }
}