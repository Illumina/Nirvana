using System;
using System.Collections.Generic;
using Illumina.DataDumperImport.Utilities;
using DS = Illumina.DataDumperImport.DataStructures;

namespace Illumina.DataDumperImport.Import
{
    internal static class PairGenomic
    {
        #region members

        private const string GenomicKey = "GENOME";

        private static readonly HashSet<string> KnownKeys;

        #endregion

        // constructor
        static PairGenomic()
        {
            KnownKeys = new HashSet<string>
            {
                GenomicKey
            };
        }

        /// <summary>
        /// parses the relevant data from each pair genomic object
        /// </summary>
        public static DS.VEP.PairGenomic Parse(DS.ObjectValue objectValue, DS.ImportDataStore dataStore)
        {
            var pairGenomic = new DS.VEP.PairGenomic();

            // loop over all of the key/value pairs in the pair genomic object
            foreach (DS.AbstractData ad in objectValue)
            {
                // sanity check: make sure we know about the keys are used for
                if (!KnownKeys.Contains(ad.Key))
                {
                    throw new ApplicationException($"Encountered an unknown key in the pair genomic object: {ad.Key}");
                }

                // handle each key
                switch (ad.Key)
                {
                    case GenomicKey:
                        var genomicNode = ad as DS.ListObjectKeyValue;
                        if (genomicNode != null)
                        {
                            pairGenomic.Genomic = MapperPair.ParseList(genomicNode.Values, dataStore);
                        }
                        else if (DumperUtilities.IsUndefined(ad))
                        {
                            pairGenomic.Genomic = null;
                        }
                        else
                        {
                            throw new ApplicationException(
                                $"Could not transform the AbstractData object into an ObjectKeyValue: [{ad.GetType()}]");
                        }
                        break;
                    default:
                        throw new ApplicationException($"Unknown key found: {ad.Key}");
                }
            }

            return pairGenomic;
        }

        /// <summary>
        /// parses the relevant data from each pair genomic object
        /// </summary>
        public static void ParseReference(DS.ObjectValue objectValue, DS.VEP.PairGenomic pairGenomic, DS.ImportDataStore dataStore)
        {
            // loop over all of the key/value pairs in the pair genomic object
            foreach (DS.AbstractData ad in objectValue)
            {
                // handle each key
                switch (ad.Key)
                {
                    case GenomicKey:
                        var genomicNode = ad as DS.ListObjectKeyValue;
                        if (genomicNode != null)
                        {
                            MapperPair.ParseListReference(genomicNode.Values, pairGenomic.Genomic, dataStore);
                        }
                        break;
                    default:
                        throw new ApplicationException($"Unknown key found: {ad.Key}");
                }
            }
        }
    }
}
