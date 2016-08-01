using System;
using System.Collections.Generic;
using Illumina.DataDumperImport.Utilities;
using DS = Illumina.DataDumperImport.DataStructures;

namespace Illumina.DataDumperImport.Import
{
    internal static class PairCodingDna
    {
        #region members

        private const string CodingDnaKey = "CDNA";

        private static readonly HashSet<string> KnownKeys;

        #endregion

        // constructor
        static PairCodingDna()
        {
            KnownKeys = new HashSet<string>
            {
                CodingDnaKey
            };
        }

        /// <summary>
        /// parses the relevant data from each pair cDNA object
        /// </summary>
        public static DS.VEP.PairCodingDna Parse(DS.ObjectValue objectValue, DS.ImportDataStore dataStore)
        {
            var pairCodingDna = new DS.VEP.PairCodingDna();

            // loop over all of the key/value pairs in the pair cDNA object
            foreach (DS.AbstractData ad in objectValue)
            {
                // sanity check: make sure we know about the keys are used for
                if (!KnownKeys.Contains(ad.Key))
                {
                    throw new ApplicationException($"Encountered an unknown key in the pair cDNA object: {ad.Key}");
                }

                // handle each key
                switch (ad.Key)
                {
                    case CodingDnaKey:
                        var codingDnaNode = ad as DS.ListObjectKeyValue;
                        if (codingDnaNode != null)
                        {
                            pairCodingDna.CodingDna = MapperPair.ParseList(codingDnaNode.Values, dataStore);
                        }
                        else if (DumperUtilities.IsUndefined(ad))
                        {
                            pairCodingDna.CodingDna = null;
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

            return pairCodingDna;
        }

        /// <summary>
        /// parses the relevant data from each pair cDNA object
        /// </summary>
        public static void ParseReference(DS.ObjectValue objectValue, DS.VEP.PairCodingDna pairCodingDna, DS.ImportDataStore dataStore)
        {
            // loop over all of the key/value pairs in the pair cDNA object
            foreach (DS.AbstractData ad in objectValue)
            {
                // handle each key
                switch (ad.Key)
                {
                    case CodingDnaKey:
                        var codingDnaNode = ad as DS.ListObjectKeyValue;
                        if (codingDnaNode != null)
                        {
                            MapperPair.ParseListReference(codingDnaNode.Values, pairCodingDna.CodingDna, dataStore);
                        }
                        break;
                    default:
                        throw new ApplicationException($"Unknown key found: {ad.Key}");
                }
            }
        }
    }
}
