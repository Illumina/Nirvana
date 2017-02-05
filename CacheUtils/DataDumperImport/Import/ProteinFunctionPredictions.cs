using System.Collections.Generic;
using CacheUtils.DataDumperImport.DataStructures;
using CacheUtils.DataDumperImport.Utilities;
using ErrorHandling.Exceptions;

namespace CacheUtils.DataDumperImport.Import
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
        public static DataStructures.VEP.ProteinFunctionPredictions Parse(ObjectValue objectValue)
        {
            var predictions          = new DataStructures.VEP.ProteinFunctionPredictions();

            // loop over all of the key/value pairs in the protein function predictions object
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
                            throw new GeneralException($"Could not handle the PolyPhen key: [{ad.GetType()}]");
                        }
                        break;
                    case PolyPhenHumVarKey:
                        // used by default
                        var polyPhenHumVarNode = ad as ObjectKeyValue;
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
                            throw new GeneralException(
                                $"Could not transform the AbstractData object into an ObjectKeyValue: [{ad.GetType()}]");
                        }
                        break;
                    case SiftKey:
                        var siftNode = ad as ObjectKeyValue;
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
                            throw new GeneralException(
                                $"Could not transform the AbstractData object into an ObjectKeyValue: [{ad.GetType()}]");
                        }
                        break;
                    default:
                        throw new GeneralException($"Unknown key found: {ad.Key}");
                }
            }

            return predictions;
        }

        /// <summary>
        /// parses the relevant data from each protein function prediction object
        /// </summary>
        public static void ParseReference(ObjectValue objectValue, DataStructures.VEP.ProteinFunctionPredictions cache, ImportDataStore dataStore)
        {
            // loop over all of the key/value pairs in the cache object
            foreach (AbstractData ad in objectValue)
            {
                if (!DumperUtilities.IsReference(ad)) continue;

                // handle each key
                var referenceKeyValue = ad as ReferenceKeyValue;
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
                        throw new GeneralException(
                            $"Found an unhandled reference in the protein function prediction object: {ad.Key}");
                }
            }
        }
    }
}
