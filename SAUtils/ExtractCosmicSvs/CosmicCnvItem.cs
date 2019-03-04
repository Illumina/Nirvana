using System.Collections.Generic;
using System.IO;
using Genome;
using OptimizedCore;
using VariantAnnotation.IO;
using Variants;

namespace SAUtils.ExtractCosmicSvs
{
   
    public sealed class CosmicCnvItem
    {
        public readonly int CNVId;
        private readonly IChromosome Chromosome;
        private readonly VariantType _cnvType;
        private readonly int _copyNumber;
        private readonly int _studyId;
        private readonly Dictionary<string, int> _cancerTypes;
        public int CancerTypeCount => _cancerTypes.Count;
        private readonly Dictionary<string, int> _tissueTypes;
        public int TissueTypeCount => _tissueTypes.Count;

        public CosmicCnvItem(int cnvId, IChromosome chromosome, int start, int end, VariantType cnvType, int copyNumber, Dictionary<string, int> cancerTypes, Dictionary<string, int> tissueTypes, int studyId)
        {
            CNVId        = cnvId;
            Chromosome   = chromosome;
            Start        = start;
            End          = end;
            _cnvType     = cnvType;
            _studyId     = studyId;
            _copyNumber  = copyNumber;
            _cancerTypes = cancerTypes;
            _tissueTypes = tissueTypes;
        }

        private int Start { get; }
        private int End { get; }

        public string GetJsonString()
        {
            var sb = StringBuilderCache.Acquire();

            var jsonObject = new JsonObject(sb);

            jsonObject.AddIntValue("id", CNVId);
            jsonObject.AddStringValue("variantType", _cnvType.ToString());
            if (_copyNumber!=-1)
                jsonObject.AddIntValue("copyNumber", _copyNumber);

            jsonObject.AddStringValues("cancerTypes", GetJsonStrings(_cancerTypes), false);
            jsonObject.AddStringValues("tissueTypes", GetJsonStrings(_tissueTypes), false);

            return sb.ToString();
        }

        private static IEnumerable<string> GetJsonStrings(IDictionary<string, int> dictionary)
        {
            foreach (var kvp in dictionary)
            {
                yield return $"{{\"{kvp.Key.Replace('_', ' ')}\":{kvp.Value}}}";
            }
        }

        public void Merge(CosmicCnvItem other)
        {
            if (CNVId != other.CNVId 
                || _cnvType != other._cnvType 
                || _copyNumber!= other._copyNumber)
                throw new InvalidDataException("Attempting to merge different cosmic CNVs");

            //avoid double counting 
            if (_studyId != other._studyId)
            {
                MergeCounts(_cancerTypes, other._cancerTypes);
                MergeCounts(_tissueTypes, other._tissueTypes);
            }

        }

        private static void MergeCounts(IDictionary<string, int> countDict1, IDictionary<string, int> countDict2)
        {
            foreach (var kvp in countDict2)
            {
                if (!countDict1.TryAdd(kvp.Key, kvp.Value)) // this key already exist
                    countDict1[kvp.Key] += kvp.Value;
            }
        }
    }
}