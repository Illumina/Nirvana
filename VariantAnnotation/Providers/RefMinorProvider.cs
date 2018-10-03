using System.Collections.Generic;
using System.IO;
using Genome;
using IO;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.NSA;
using VariantAnnotation.SA;

namespace VariantAnnotation.Providers
{
    public sealed class RefMinorProvider : IRefMinorProvider
    {
        private readonly RefMinorDbReader _reader;

        public RefMinorProvider(List<string> supplementaryAnnotationDirectories)
        {
            foreach (string directory in supplementaryAnnotationDirectories)
            {
                foreach (string file in Directory.GetFiles(directory, "*"+SaCommon.RefMinorFileSuffix))
                {
                    var dbExtReader = new ExtendedBinaryReader(FileUtilities.GetReadStream(file));
                    var indexFileName = file + SaCommon.IndexSufix;
                    var indexExtReader = new ExtendedBinaryReader(FileUtilities.GetReadStream(indexFileName));
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