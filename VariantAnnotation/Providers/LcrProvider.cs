using System.Collections.Generic;
using System.IO;
using Genome;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.NSA;

namespace VariantAnnotation.Providers
{
    public class LcrProvider: IAnnotationProvider
    {
        public string Name => "Lcr provider";
        public GenomeAssembly Assembly { get; }
        public IEnumerable<IDataSourceVersion> DataSourceVersions { get; }

        private readonly NsiReader _nsiReader;

        public LcrProvider(Stream stream)
        {
            _nsiReader = NsiReader.Read(stream);
            Assembly = _nsiReader.Assembly;
            DataSourceVersions = new[] { _nsiReader.Version };
        }

        public void Dispose()
        {
            // nsiReaders are not disposable. They read from the input stream and disposes it in the Read method.
        }

        public void Annotate(IAnnotatedPosition annotatedPosition)
        {
            foreach (var annotatedVariant in annotatedPosition.AnnotatedVariants)
            {
                annotatedVariant.InLowComplexityRegion = _nsiReader.OverlapsAny(annotatedVariant.Variant);
            }
        }

        public void PreLoad(IChromosome chromosome, List<int> positions)
        {
            throw new System.NotImplementedException();
        }
    }
}