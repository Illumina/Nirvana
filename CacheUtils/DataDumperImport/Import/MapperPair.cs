using System.Collections.Generic;
using System.Text.RegularExpressions;
using CacheUtils.DataDumperImport.DataStructures;
using CacheUtils.DataDumperImport.Utilities;
using ErrorHandling.Exceptions;

namespace CacheUtils.DataDumperImport.Import
{
    internal static class MapperPair
    {
        #region members

        private const string DataType = "Bio::EnsEMBL::Mapper::Pair";

        private const string FromKey = "from";
        private const string OriKey  = "ori";
        private const string ToKey   = "to";

        private static readonly HashSet<string> KnownKeys;

        private static readonly Regex ReferenceRegex;
        private static readonly Regex ReferenceCodingDnaRegex;

        #endregion

        // constructor
        static MapperPair()
        {
            KnownKeys = new HashSet<string>
            {
                FromKey,
                OriKey,
                ToKey
            };

            ReferenceRegex          = new Regex("\\$VAR1->{'[^']+?'}\\[(\\d+)\\]{'_variation_effect_feature_cache'}{'mapper'}{'exon_coord_mapper'}{'_pair_genomic'}{'GENOME'}\\[(\\d+)\\]", RegexOptions.Compiled);
            ReferenceCodingDnaRegex = new Regex("\\$VAR1->{'[^']+?'}\\[(\\d+)\\]{'_variation_effect_feature_cache'}{'mapper'}{'exon_coord_mapper'}{'_pair_cdna'}{'CDNA'}\\[(\\d+)\\]", RegexOptions.Compiled);
        }

        /// <summary>
        /// parses the relevant data from each mapper pairs object
        /// </summary>
        private static DataStructures.VEP.MapperPair Parse(ObjectValue objectValue, ushort currentReferenceIndex)
        {
            DataStructures.VEP.MapperUnit from    = null;
            DataStructures.VEP.MapperUnit to      = null;

            // loop over all of the key/value pairs in the mapper pair object
            foreach (AbstractData ad in objectValue)
            {
                // sanity check: make sure we know about the keys are used for
                if (!KnownKeys.Contains(ad.Key))
                {
                    throw new GeneralException($"Encountered an unknown key in the mapper pair object: {ad.Key}");
                }

                // handle each key
                switch (ad.Key)
                {
                    case FromKey:
                        var fromKeyNode = ad as ObjectKeyValue;
                        if (fromKeyNode != null)
                        {
                            from = MapperUnit.Parse(fromKeyNode.Value, currentReferenceIndex);
                        }
                        else
                        {
                            throw new GeneralException(
                                $"Could not transform the AbstractData object into an ObjectKeyValue: [{ad.GetType()}]");
                        }
                        break;
                    case OriKey:
                        // skip
                        break;
                    case ToKey:
                        var toKeyNode = ad as ObjectKeyValue;
                        if (toKeyNode != null)
                        {
                            to = MapperUnit.Parse(toKeyNode.Value, currentReferenceIndex);
                        }
                        else
                        {
                            throw new GeneralException(
                                $"Could not transform the AbstractData object into an ObjectKeyValue: [{ad.GetType()}]");
                        }
                        break;
                    default:
                        throw new GeneralException($"Unknown key found: {ad.Key}");
                }
            }

            return new DataStructures.VEP.MapperPair(from, to);
        }

        /// <summary>
        /// points to a mapper pair that has already been created
        /// </summary>
        private static DataStructures.VEP.MapperPair ParseReference(string reference, ImportDataStore dataStore)
        {
            var mapperPairReferenceMatch = ReferenceRegex.Match(reference);
            if (!mapperPairReferenceMatch.Success) return ParseCodingDnaReference(reference, dataStore);

            int transcriptIndex;
            if (!int.TryParse(mapperPairReferenceMatch.Groups[1].Value, out transcriptIndex))
            {
                throw new GeneralException(
                    $"Unable to convert the transcript index from a string to an integer: [{mapperPairReferenceMatch.Groups[1].Value}]");
            }

            // sanity check: make sure we have at least that many transcripts in our list
            if (transcriptIndex < 0 || transcriptIndex >= dataStore.Transcripts.Count)
            {
                throw new GeneralException(
                    $"Unable to link the mapper pair reference: transcript index: [{transcriptIndex}], current # of transcripts: [{dataStore.Transcripts.Count}]");
            }

            int genomicMapperPairIndex;
            if (!int.TryParse(mapperPairReferenceMatch.Groups[2].Value, out genomicMapperPairIndex))
            {
                throw new GeneralException(
                    $"Unable to convert the mapper pair index from a string to an integer: [{mapperPairReferenceMatch.Groups[2].Value}]");
            }

            // sanity check: make sure we have at least that many mapper pairs in our list
            int numGenomicMapperPairs = dataStore.Transcripts[transcriptIndex].VariantEffectCache.Mapper.ExonCoordinateMapper.PairGenomic.Genomic.Count;
            if (genomicMapperPairIndex < 0 || genomicMapperPairIndex >= numGenomicMapperPairs)
            {
                throw new GeneralException(
                    $"Unable to link the mapper pair reference: mapper pair index: [{genomicMapperPairIndex}], current # of mapper pairs: [{numGenomicMapperPairs}]");
            }

            // Console.WriteLine("reference:         {0}", reference);
            // Console.WriteLine("transcript index:  {0}", transcriptIndex);
            // Console.WriteLine("mapper pair index: {0}", genomicMapperPairIndex);

            return dataStore.Transcripts[transcriptIndex].VariantEffectCache.Mapper.ExonCoordinateMapper.PairGenomic.Genomic[genomicMapperPairIndex];
        }

        /// <summary>
        /// points to a mapper pair that has already been created
        /// </summary>
        private static DataStructures.VEP.MapperPair ParseCodingDnaReference(string reference, ImportDataStore dataStore)
        {
            var mapperPairReferenceMatch = ReferenceCodingDnaRegex.Match(reference);

            if (!mapperPairReferenceMatch.Success)
            {
                throw new GeneralException(
                    $"Unable to use the regular expression on the mapper pair reference string: [{reference}]");
            }

            int transcriptIndex;
            if (!int.TryParse(mapperPairReferenceMatch.Groups[1].Value, out transcriptIndex))
            {
                throw new GeneralException(
                    $"Unable to convert the transcript index from a string to an integer: [{mapperPairReferenceMatch.Groups[1].Value}]");
            }

            // sanity check: make sure we have at least that many transcripts in our list
            if (transcriptIndex < 0 || transcriptIndex >= dataStore.Transcripts.Count)
            {
                throw new GeneralException(
                    $"Unable to link the mapper pair reference: transcript index: [{transcriptIndex}], current # of transcripts: [{dataStore.Transcripts.Count}]");
            }

            int codingDnaMapperPairIndex;
            if (!int.TryParse(mapperPairReferenceMatch.Groups[2].Value, out codingDnaMapperPairIndex))
            {
                throw new GeneralException(
                    $"Unable to convert the mapper pair index from a string to an integer: [{mapperPairReferenceMatch.Groups[2].Value}]");
            }

            // sanity check: make sure we have at least that many mapper pairs in our list
            int numGenomicMapperPairs = dataStore.Transcripts[transcriptIndex].VariantEffectCache.Mapper.ExonCoordinateMapper.PairGenomic.Genomic.Count;
            if (codingDnaMapperPairIndex < 0 || codingDnaMapperPairIndex >= numGenomicMapperPairs)
            {
                throw new GeneralException(
                    $"Unable to link the mapper pair reference: mapper pair index: [{codingDnaMapperPairIndex}], current # of mapper pairs: [{numGenomicMapperPairs}]");
            }

            // Console.WriteLine("reference:         {0}", reference);
            // Console.WriteLine("transcript index:  {0}", transcriptIndex);
            // Console.WriteLine("mapper pair index: {0}", genomicMapperPairIndex);

            return dataStore.Transcripts[transcriptIndex].VariantEffectCache.Mapper.ExonCoordinateMapper.PairCodingDna.CodingDna[codingDnaMapperPairIndex];
        }

        /// <summary>
        /// parses the relevant data from each mapper pairs object
        /// </summary>
        public static List<DataStructures.VEP.MapperPair> ParseList(List<AbstractData> abstractDataList, ImportDataStore dataStore)
        {
            var mapperPairs = DumperUtilities.GetPopulatedList<DataStructures.VEP.MapperPair>(abstractDataList.Count);

            // loop over all of the key/value pairs in the mapper pairs object
            for (int mapperPairIndex = 0; mapperPairIndex < abstractDataList.Count; mapperPairIndex++)
            {
                var ad = abstractDataList[mapperPairIndex];

                // skip references
                if (DumperUtilities.IsReference(ad)) continue;

                if (ad.DataType != DataType)
                {
                    throw new GeneralException(
                        $"Expected a mapper pair data type, but found the following data type: [{ad.DataType}]");
                }

                var mapperPairNode = ad as ObjectValue;
                if (mapperPairNode == null)
                {
                    throw new GeneralException(
                        $"Could not transform the AbstractData object into an ObjectValue: [{ad.GetType()}]");
                }

                var newMapperPair = Parse(mapperPairNode, dataStore.CurrentReferenceIndex);
                // DS.VEP.MapperPair oldMapperPair;
                // if (dataStore.MapperPairs.TryGetValue(newMapperPair, out oldMapperPair))
                //{
                //    mapperPairs[mapperPairIndex] = oldMapperPair;
                //}
                // else
                //{
                mapperPairs[mapperPairIndex] = newMapperPair;
                //    dataStore.MapperPairs[newMapperPair] = newMapperPair;
                //}
            }

            return mapperPairs;
        }

        /// <summary>
        /// parses the relevant data from each mapper pairs object
        /// </summary>
        public static void ParseListReference(List<AbstractData> abstractDataList, List<DataStructures.VEP.MapperPair> mapperPairs, ImportDataStore dataStore)
        {
            // loop over all of the key/value pairs in the mapper pairs object
            for (int mapperPairIndex = 0; mapperPairIndex < abstractDataList.Count; mapperPairIndex++)
            {
                var mapperNode = abstractDataList[mapperPairIndex];

                // skip normal mapper pairs
                if (!DumperUtilities.IsReference(mapperNode)) continue;

                var referenceStringValue = mapperNode as ReferenceStringValue;
                if (referenceStringValue != null)
                {
                    var mapperPair = ParseReference(referenceStringValue.Value,dataStore);
                    mapperPairs[mapperPairIndex] = mapperPair;
                }
            }
        }
    }
}
