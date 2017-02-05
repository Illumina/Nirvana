using System.Collections.Generic;
using CacheUtils.DataDumperImport.DataStructures;
using CacheUtils.DataDumperImport.Utilities;
using ErrorHandling.Exceptions;

namespace CacheUtils.DataDumperImport.Import
{
    internal static class Intron
    {
        #region members

        private static readonly HashSet<string> KnownKeys;

        #endregion

        // constructor
        static Intron()
        {
            KnownKeys = new HashSet<string>
            {
                Transcript.EndKey,
                Transcript.SliceKey,
                Transcript.StartKey,
                Transcript.StrandKey
            };
        }

        /// <summary>
        /// returns a new exon given an ObjectValue
        /// </summary>
        private static DataStructures.VEP.Intron Parse(ObjectValue objectValue, ImportDataStore dataStore)
        {
            var intron = new DataStructures.VEP.Intron();

            // loop over all of the key/value pairs in the intron object
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
                    case Transcript.EndKey:
                        intron.End = DumperUtilities.GetInt32(ad);
                        break;
                    case Transcript.SliceKey:
                        var sliceNode = ad as ObjectKeyValue;
                        if (sliceNode != null)
                        {
                            var newSlice = Slice.Parse(sliceNode.Value, dataStore.CurrentReferenceIndex);
                            // DS.VEP.Slice oldSlice;
                            // if (dataStore.Slices.TryGetValue(newSlice, out oldSlice))
                            //{
                            //    intron.Slice = oldSlice;
                            //}
                            // else
                            //{
                            intron.Slice = newSlice;
                            //    dataStore.Slices[newSlice] = newSlice;
                            //}
                        }
                        else if (DumperUtilities.IsReference(ad))
                        {
                            // skip references until the second pass
                        }
                        else
                        {
                            throw new GeneralException(
                                $"Could not transform the AbstractData object into an ObjectKeyValue or ReferenceKeyValue: [{ad.GetType()}]");
                        }
                        break;
                    case Transcript.StartKey:
                        intron.Start = DumperUtilities.GetInt32(ad);
                        break;
                    case Transcript.StrandKey:
                        TranscriptUtilities.GetStrand(ad);
                        break;
                    default:
                        throw new GeneralException($"Unknown key found: {ad.Key}");
                }
            }

            return intron;
        }

        /// <summary>
        /// parses the relevant data from each intron object
        /// </summary>
        private static void ParseReference(ObjectValue objectValue, DataStructures.VEP.Intron intron, ImportDataStore dataStore)
        {
            // loop over all of the key/value pairs in the intron object
            foreach (AbstractData ad in objectValue)
            {
                // skip normal entries
                if (!DumperUtilities.IsReference(ad)) continue;

                // handle each key
                switch (ad.Key)
                {
                    case Transcript.SliceKey:
                        var referenceKeyValue = ad as ReferenceKeyValue;
                        if (referenceKeyValue != null) intron.Slice = Slice.ParseReference(referenceKeyValue.Value, dataStore);
                        break;
                    default:
                        throw new GeneralException($"Found an unhandled reference in the intron object: {ad.Key}");
                }
            }
        }

        /// <summary>
        /// parses the relevant data from each intron object
        /// </summary>
        public static DataStructures.VEP.Intron[] ParseList(List<AbstractData> abstractDataList, ImportDataStore dataStore)
        {
            var introns = new DataStructures.VEP.Intron[abstractDataList.Count];

            // loop over all of the introns
            for (int intronIndex = 0; intronIndex < abstractDataList.Count; intronIndex++)
            {
                var objectValue = abstractDataList[intronIndex] as ObjectValue;
                if (objectValue == null)
                {
                    throw new GeneralException(
                        $"Could not transform the AbstractData object into an ObjectValue: [{abstractDataList[intronIndex].GetType()}]");
                }
                introns[intronIndex] = Parse(objectValue, dataStore);
            }

            return introns;
        }

        /// <summary>
        /// points to a introns that have already been created
        /// </summary>
        public static void ParseListReference(List<AbstractData> abstractDataList, DataStructures.VEP.Intron[] introns, ImportDataStore dataStore)
        {
            // loop over all of the introns
            for (int intronIndex = 0; intronIndex < abstractDataList.Count; intronIndex++)
            {
                var intronNode = abstractDataList[intronIndex];

                var objectValue = intronNode as ObjectValue;
                if (objectValue == null)
                {
                    throw new GeneralException(
                        $"Could not transform the AbstractData object into an ObjectValue: [{abstractDataList[intronIndex].GetType()}]");
                }

                ParseReference(objectValue, introns[intronIndex], dataStore);
            }
        }
    }
}
