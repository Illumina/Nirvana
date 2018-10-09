using System.Collections.Generic;
using System.IO;
using System.Linq;
using Genome;
using IO;
using IO.StreamSourceCollection;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.NSA;
using VariantAnnotation.SA;

namespace VariantAnnotation.Providers
{
    public sealed class RefMinorProvider : IRefMinorProvider
    {
        private readonly RefMinorDbReader _reader;

        public RefMinorProvider(IEnumerable<IStreamSourceCollection> annotationStreamSourceCollections)
        {
            foreach (var collection in annotationStreamSourceCollections)
            {
                foreach (var streamSource in collection.GetStreamSources(SaCommon.RefMinorFileSuffix))
                {
                    var dbExtReader = new ExtendedBinaryReader(streamSource.GetStream());
                    var indexExtReader = new ExtendedBinaryReader(streamSource.GetAssociatedStreamSource(SaCommon.IndexSufix).GetStream());
                    _reader= new RefMinorDbReader(dbExtReader, indexExtReader);
                }
            }
        }

        public void PreLoad(IChromosome chromosome)
        {
            _reader.PreLoad(chromosome);
        }

        public string GetGlobalMajorAllele(IChromosome chromosome, int pos)
        {
            return _reader.GetGlobalMajorAllele(chromosome, pos);
        }
    }
}