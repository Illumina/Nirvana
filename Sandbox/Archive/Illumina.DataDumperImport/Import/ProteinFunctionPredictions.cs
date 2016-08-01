using System;
using System.Collections.Generic;
using Illumina.DataDumperImport.Utilities;
using DS = Illumina.DataDumperImport.DataStructures;

namespace Illumina.DataDumperImport.Import
{
    internal static class ProteinFunctionPredictions
    {
        #region members

        private const string PolyPhenKey       = "polyphen";
        private const string PolyPhenHumVarKey = "polyphen_humvar";
        private const string PolyPhenHumDivKey = "polyphen_humdiv";
        private const string SiftKey           = "sift";

        private static readonly HashSet<string> KnownKeys;

        #endregion

        // constructor
        static ProteinFunctionPredictions()
        {
            KnownKeys = new HashSet<string>
            {
                PolyPhenHumVarKey,
                PolyPhenHumDivKey,
                PolyPhenKey,
                SiftKey
            };
        }

        /// <summary>
        /// parses the relevant data from each protein function predictions object
        /// </summary>
        public static DS.VEP.ProteinFunctionPredictions Parse(DS.ObjectValue objectValue)
        {
            var predictions          = new DS.VEP.ProteinFunctionPredictions();

            // loop over all of the key/value pairs in the protein function predictions object
            foreach (DS.AbstractData ad in objectValue)
            {
                // sanity check: make sure we know about the keys are used for
                if (!KnownKeys.Contains(ad.Key))
                {
                    throw new ApplicationException($"Encountered an unknown key in the dumper mapper object: {ad.Key}");
                }

                // handle each key
                switch (ad.Key)
                {
                    case PolyPhenHumDivKey:
                        // not used by default
                        break;
                    case PolyPhenKey:
                        if (DumperUtilities.IsUndefined(ad))
                        {
                            // do nothing
                        }
                        else
                        {
                            throw new ApplicationException($"Could not handle the PolyPhen key: [{ad.GetType()}]");
                        }
                        break;
                    case PolyPhenHumVarKey:
                        // used by default
                        var polyPhenHumVarNode = ad as DS.ObjectKeyValue;
                        if (polyPhenHumVarNode != null)
                        {
                            predictions.PolyPhen = PolyPhen.Parse(polyPhenHumVarNode.Value);
                        }
                        else if (DumperUtilities.IsUndefined(ad))
                        {
                            predictions.PolyPhen = null;
                        }
                        else if (DumperUtilities.IsReference(ad))
                        {
                            // skip references for now
                        }
                        else
                        {
                            throw new ApplicationException(
                                $"Could not transform the AbstractData object into an ObjectKeyValue: [{ad.GetType()}]");
                        }
                        break;
                    case SiftKey:
                        var siftNode = ad as DS.ObjectKeyValue;
                        if (siftNode != null)
                        {
                            predictions.Sift = Sift.Parse(siftNode.Value);
                        }
                        else if (DumperUtilities.IsUndefined(ad))
                        {
                            predictions.Sift = null;
                        }
                        else if (DumperUtilities.IsReference(ad))
                        {
                            // skip references for now
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

            return predictions;
        }

        /// <summary>
        /// parses the relevant data from each protein function prediction object
        /// </summary>
        public static void ParseReference(DS.ObjectValue objectValue, DS.VEP.ProteinFunctionPredictions cache, DS.ImportDataStore dataStore)
        {
            // loop over all of the key/value pairs in the cache object
            foreach (DS.AbstractData ad in objectValue)
            {
                if (!DumperUtilities.IsReference(ad)) continue;

                // handle each key
                var referenceKeyValue = ad as DS.ReferenceKeyValue;
                if (referenceKeyValue == null) continue;

                switch (referenceKeyValue.Key)
                {
                    case PolyPhenHumVarKey:
                        cache.PolyPhen = PolyPhen.ParseReference(referenceKeyValue.Value, dataStore);
                        break;
                    case SiftKey:
                        cache.Sift = Sift.ParseReference(referenceKeyValue.Value, dataStore);
                        break;
                    default:
                        throw new ApplicationException(
                            $"Found an unhandled reference in the protein function prediction object: {ad.Key}");
                }
            }
        }
    }
}
