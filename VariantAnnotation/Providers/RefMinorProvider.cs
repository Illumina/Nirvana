using System.Collections.Generic;
using System.IO;
using System.Linq;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.SA;
using VariantAnnotation.Utilities;

namespace VariantAnnotation.Providers
{
    public sealed class RefMinorProvider : IRefMinorProvider
    {
        private readonly Dictionary<string, Dictionary<int, string>> _positionDict = new Dictionary<string, Dictionary<int, string>>();

        public RefMinorProvider(List<string> supplementaryAnnotationDirectories)
        {
            foreach (var directory in supplementaryAnnotationDirectories)
            {
                foreach (var file in Directory.GetFiles(directory, "*.idx"))
                {
                    var chromeName = Path.GetFileNameWithoutExtension(file).Split('.')[0];
                    var refMinorPostions = SaIndex.Read(FileUtilities.GetReadStream(file)).GlobalMajorAlleleForRefMinor;
                    if (refMinorPostions.Length > 0) _positionDict[chromeName] = refMinorPostions.ToDictionary(x => x.Position, x => x.GlobalMajorAllele);

                }
            }
        }

        public bool IsReferenceMinor(IChromosome chromosome, int pos)
        {
            return _positionDict.ContainsKey(chromosome.UcscName) && _positionDict[chromosome.UcscName].ContainsKey(pos);
        }

        public string GetGlobalMajorAlleleForRefMinor(IChromosome chromosome, int pos)
        {
            return !IsReferenceMinor(chromosome, pos) ? null : _positionDict[chromosome.UcscName][pos];
        }
    }
}