using System.IO;
using Genome;
using IO;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.NSA;

namespace VariantAnnotation.Providers
{
    public sealed class RefMinorProvider : IRefMinorProvider
    {
        private readonly RefMinorDbReader _reader;

        public RefMinorProvider(Stream dbStream, Stream indexStream)
        {
            _reader = new RefMinorDbReader(new ExtendedBinaryReader(dbStream), new ExtendedBinaryReader(indexStream));
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