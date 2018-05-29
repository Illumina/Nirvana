using System.Collections.Generic;
using System.IO;
using Genome;
using IO;
using OptimizedCore;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.SA;

namespace VariantAnnotation.Providers
{
    public sealed class RefMinorProvider : IRefMinorProvider
    {
        private readonly Dictionary<int, string>[] _refMinorDictByChromosome;

        public RefMinorProvider(IDictionary<string, IChromosome> refNameToChromosome, List<string> supplementaryAnnotationDirectories)
        {
            int maxRefIndex = GetMaxRefIndex(refNameToChromosome);

            _refMinorDictByChromosome = new Dictionary<int, string>[maxRefIndex + 1];
            for (var i = 0; i <= maxRefIndex; i++) _refMinorDictByChromosome[i] = new Dictionary<int, string>();

            foreach (string directory in supplementaryAnnotationDirectories)
            {
                foreach (string file in Directory.GetFiles(directory, "*.idx"))
                {
                    string chromosomeName = Path.GetFileNameWithoutExtension(file).OptimizedSplit('.')[0];
                    var chromosome = ReferenceNameUtilities.GetChromosome(refNameToChromosome, chromosomeName);
                    if (chromosome.Index == ushort.MaxValue) continue;

                    var refMinorDict = _refMinorDictByChromosome[chromosome.Index];
                    var refMinorPostions  = SaIndex.Read(FileUtilities.GetReadStream(file)).GlobalMajorAlleleForRefMinor;
                    foreach (var kvp in refMinorPostions) refMinorDict[kvp.Position] = kvp.GlobalMajorAllele;
                }
            }
        }

        private static int GetMaxRefIndex(IDictionary<string, IChromosome> refNameToChromosome)
        {
            ushort maxIndex = 0;
            foreach(var chromosome in refNameToChromosome.Values)
                if (chromosome.Index > maxIndex)
                    maxIndex = chromosome.Index;
            return maxIndex;
        }

        public string GetGlobalMajorAllele(IChromosome chromosome, int pos)
        {
            if (chromosome.Index == ushort.MaxValue) return null;
            var refMinorDict = _refMinorDictByChromosome[chromosome.Index];
            return refMinorDict.TryGetValue(pos, out string globalMajorAllele) ? globalMajorAllele : null;
        }
    }
}