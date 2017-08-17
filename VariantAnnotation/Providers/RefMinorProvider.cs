using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.SA;
using VariantAnnotation.Utilities;

namespace VariantAnnotation.Providers
{
    public sealed class RefMinorProvider:IRefMinorProvider
    {
        private readonly Dictionary<string,ImmutableHashSet<int>> _positionDict = new Dictionary<string,ImmutableHashSet<int>>();

        public RefMinorProvider(List<string> supplementaryAnnotationDirectories)
        {
            foreach (var directory in supplementaryAnnotationDirectories)
            {
                foreach (var file in Directory.GetFiles(directory,"*.idx"))
                {
                    var chromeName = Path.GetFileNameWithoutExtension(file).Split('.')[0];
                    var refMinorPostions = SaIndex.Read(FileUtilities.GetReadStream(file)).RefMinorPositions;
                    if(refMinorPostions.Length>0) _positionDict[chromeName] = refMinorPostions.ToImmutableHashSet();

                }
            }
        }

        public bool IsReferenceMinor(IChromosome chromosome, int pos)
        {
            return _positionDict.ContainsKey(chromosome.UcscName) && _positionDict[chromosome.UcscName].Contains(pos);
        }
    }
}