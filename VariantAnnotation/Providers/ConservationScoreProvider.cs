using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Genome;
using IO;
using IO.StreamSourceCollection;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.PhyloP;
using VariantAnnotation.SA;
using Variants;

namespace VariantAnnotation.Providers
{
    public sealed class ConservationScoreProvider : IAnnotationProvider
    {
        private readonly NpdReader _phylopReader;

        public string Name { get; }
        public GenomeAssembly Assembly { get; }
        public IEnumerable<IDataSourceVersion> DataSourceVersions { get; }

        public ConservationScoreProvider(NpdReader phylopReader)
        {
            _phylopReader = phylopReader;
            Name = "Conservation score provider";
            Assembly = phylopReader.Assembly;
            DataSourceVersions = new[] { phylopReader.Version };
        }

        public static ConservationScoreProvider GetConservationScoreProvider(IEnumerable<IStreamSourceCollection> annotationStreamSourceCollections)
        {

            foreach (var collection in annotationStreamSourceCollections)
            {
                var phylopStreamSources = collection.GetStreamSources(SaCommon.PhylopFileSuffix).ToArray();
                if (phylopStreamSources.Length <= 0) continue;
                var npdSource = phylopStreamSources[0];
                var npdIndexSource = npdSource.GetAssociatedStreamSource(SaCommon.IndexSufix);
                //we can have only one phylop database
                return new ConservationScoreProvider(new NpdReader(npdSource.GetStream(), npdIndexSource.GetStream()));
            }

            return null;
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

    }
}