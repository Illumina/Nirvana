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
        private NpdReader _phylopReader;

        public           string                          Name               { get; }
        public           GenomeAssembly                  Assembly           => _phylopReader.Assembly;
        public           IEnumerable<IDataSourceVersion> DataSourceVersions => _versions;
        private readonly List<IDataSourceVersion>        _versions = new();

        public ConservationScoreProvider()
        {
            Name = "Conservation score provider";
        }

        public ConservationScoreProvider AddPhylopReader(Stream dbStream, Stream indexStream)
        {
            if (dbStream == null || indexStream == null) return this;
            _phylopReader = new NpdReader(dbStream, indexStream);
            _versions.Add(_phylopReader.Version);
            return this;
        }

        public void Annotate(IAnnotatedPosition annotatedPosition)
        {
            foreach (var annotatedVariant in annotatedPosition.AnnotatedVariants)
            {
                if (annotatedVariant.Variant.Type != VariantType.SNV) continue;
                if (_phylopReader != null)
                    annotatedVariant.PhylopScore = _phylopReader.GetAnnotation(annotatedPosition.Position.Chromosome, annotatedVariant.Variant.Start);
            }
        }

        public void PreLoad(Chromosome chromosome, List<int> positions)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _phylopReader?.Dispose();
        }
    }
}