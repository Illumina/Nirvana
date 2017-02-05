using System.Collections.Generic;
using CacheUtils.DataDumperImport.DataStructures;
using CacheUtils.DataDumperImport.Utilities;
using ErrorHandling.Exceptions;

namespace CacheUtils.DataDumperImport.Import
{
    internal static class Mapper
    {
        #region members

        private const string FromCoordSystemKey = "from_cs";
        private const string FromNameKey        = "from";
        private const string IsSortedKey        = "_is_sorted";
        private const string PairCodingDnaKey   = "_pair_cdna";
        private const string PairCountKey       = "pair_count";
        private const string PairGenomicKey     = "_pair_genomic";
        private const string ToCoordSystemKey   = "to_cs";
        private const string ToNameKey          = "to";

        private static readonly HashSet<string> KnownKeys;

        #endregion

        // constructor
        static Mapper()
        {
            KnownKeys = new HashSet<string>
            {
                FromCoordSystemKey,
                FromNameKey,
                IsSortedKey,
                PairCodingDnaKey,
                PairCountKey,
                PairGenomicKey,
                ToCoordSystemKey,
                ToNameKey
            };
        }

        /// <summary>
        /// parses the relevant data from each exon coordinate mapper object
        /// </summary>
        public static DataStructures.VEP.Mapper Parse(ObjectValue objectValue, ImportDataStore dataStore)
        {
            var mapper = new DataStructures.VEP.Mapper();

            // loop over all of the key/value pairs in the exon coordinate mapper object
            foreach (AbstractData ad in objectValue)
            {
                // sanity check: make sure we know about the keys are used for
                if (!KnownKeys.Contains(ad.Key))
                {
                    throw new GeneralException($"Encountered an unknown key in the dumper mapper object: {ad.Key}");
                }

                // handle each key
                switch (ad.Key)
                {
                    case FromCoordSystemKey:
                        if (!DumperUtilities.IsUndefined(ad))
                        {
                            throw new GeneralException("Found an unexpected value in FromCoordSystemKey");
                        }
                        break;
                    case FromNameKey:
                        mapper.FromType = DumperUtilities.GetString(ad);
                        break;
                    case IsSortedKey:
                        mapper.IsSorted = DumperUtilities.GetBool(ad);
                        break;
                    case PairCodingDnaKey:
                        var pairCodingDnaNode = ad as ObjectKeyValue;
                        if (pairCodingDnaNode != null)
                        {
                            mapper.PairCodingDna = PairCodingDna.Parse(pairCodingDnaNode.Value, dataStore);
                        }
                        else if (DumperUtilities.IsUndefined(ad))
                        {
                            mapper.PairCodingDna = null;
                        }
                        else
                        {
                            throw new GeneralException(
                                $"Could not transform the AbstractData object into an ObjectKeyValue: [{ad.GetType()}]");
                        }
                        break;
                    case PairCountKey:
                        mapper.PairCount = DumperUtilities.GetInt32(ad);
                        break;
                    case PairGenomicKey:
                        var pairGenomicNode = ad as ObjectKeyValue;
                        if (pairGenomicNode != null)
                        {
                            mapper.PairGenomic = PairGenomic.Parse(pairGenomicNode.Value, dataStore);
                        }
                        else if (DumperUtilities.IsUndefined(ad))
                        {
                            mapper.PairGenomic = null;
                        }
                        else
                        {
                            throw new GeneralException(
                                $"Could not transform the AbstractData object into an ObjectKeyValue: [{ad.GetType()}]");
                        }
                        break;
                    case ToCoordSystemKey:
                        if (!DumperUtilities.IsUndefined(ad))
                        {
                            throw new GeneralException("Found an unexpected value in ToCoordSystemKey");
                        }
                        break;
                    case ToNameKey:
                        mapper.ToType = DumperUtilities.GetString(ad);
                        break;
                    default:
                        throw new GeneralException($"Unknown key found: {ad.Key}");
                }
            }

            return mapper;
        }

        /// <summary>
        /// parses the relevant data from each mapper
        /// </summary>
        public static void ParseReference(ObjectValue objectValue, DataStructures.VEP.Mapper mapper, ImportDataStore dataStore)
        {
            // loop over all of the key/value pairs in the mapper object
            foreach (AbstractData ad in objectValue)
            {
                switch (ad.Key)
                {
                    case PairCodingDnaKey:
                        var pairCodingDnaNode = ad as ObjectKeyValue;
                        if (pairCodingDnaNode != null) PairCodingDna.ParseReference(pairCodingDnaNode.Value, mapper.PairCodingDna, dataStore);
                        break;
                    case PairGenomicKey:
                        var pairGenomicNode = ad as ObjectKeyValue;
                        if (pairGenomicNode != null) PairGenomic.ParseReference(pairGenomicNode.Value, mapper.PairGenomic, dataStore);
                        break;
                }
            }
        }
    }
}
