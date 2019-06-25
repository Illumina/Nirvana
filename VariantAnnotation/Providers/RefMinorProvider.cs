using System.IO;
using Genome;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.NSA;

namespace VariantAnnotation.Providers
{
    public sealed class RefMinorProvider : IRefMinorProvider
    {
        private readonly RefMinorDbReader _reader;

        public RefMinorProvider(Stream dbStream, Stream indexStream)
        {
            _reader = new RefMinorDbReader(dbStream, indexStream);
        }

        public string GetGlobalMajorAllele(IChromosome chromosome, int pos) => _reader.GetGlobalMajorAllele(chromosome, pos);

        public void Dispose()
        {
            _reader?.Dispose();
        }
    }
}