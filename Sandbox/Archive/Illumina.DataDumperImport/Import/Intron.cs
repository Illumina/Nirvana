using System;
using System.Collections.Generic;
using Illumina.DataDumperImport.Utilities;
using DS = Illumina.DataDumperImport.DataStructures;

namespace Illumina.DataDumperImport.Import
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
        private static DS.VEP.Intron Parse(DS.ObjectValue objectValue, DS.ImportDataStore dataStore)
        {
            var intron = new DS.VEP.Intron();

            // loop over all of the key/value pairs in the intron object
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
                    case Transcript.EndKey:
                        intron.End = DumperUtilities.GetInt32(ad);
                        break;
                    case Transcript.SliceKey:
                        var sliceNode = ad as DS.ObjectKeyValue;
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
                            throw new ApplicationException(
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
                        throw new ApplicationException($"Unknown key found: {ad.Key}");
                }
            }

            return intron;
        }

        /// <summary>
        /// parses the relevant data from each intron object
        /// </summary>
        private static void ParseReference(DS.ObjectValue objectValue, DS.VEP.Intron intron, DS.ImportDataStore dataStore)
        {
            // loop over all of the key/value pairs in the intron object
            foreach (DS.AbstractData ad in objectValue)
            {
                // skip normal entries
                if (!DumperUtilities.IsReference(ad)) continue;

                // handle each key
                switch (ad.Key)
                {
                    case Transcript.SliceKey:
                        var referenceKeyValue = ad as DS.ReferenceKeyValue;
                        if (referenceKeyValue != null) intron.Slice = Slice.ParseReference(referenceKeyValue.Value, dataStore);
                        break;
                    default:
                        throw new ApplicationException($"Found an unhandled reference in the intron object: {ad.Key}");
                }
            }
        }

        /// <summary>
        /// parses the relevant data from each intron object
        /// </summary>
        public static DS.VEP.Intron[] ParseList(List<DS.AbstractData> abstractDataList, DS.ImportDataStore dataStore)
        {
            var introns = new DS.VEP.Intron[abstractDataList.Count];

            // loop over all of the introns
            for (int intronIndex = 0; intronIndex < abstractDataList.Count; intronIndex++)
            {
                var objectValue = abstractDataList[intronIndex] as DS.ObjectValue;
                if (objectValue == null)
                {
                    throw new ApplicationException(
                        $"Could not transform the AbstractData object into an ObjectValue: [{abstractDataList[intronIndex].GetType()}]");
                }
                introns[intronIndex] = Parse(objectValue, dataStore);
            }

            return introns;
        }

        /// <summary>
        /// points to a introns that have already been created
        /// </summary>
        public static void ParseListReference(List<DS.AbstractData> abstractDataList, DS.VEP.Intron[] introns, DS.ImportDataStore dataStore)
        {
            // loop over all of the introns
            for (int intronIndex = 0; intronIndex < abstractDataList.Count; intronIndex++)
            {
                var intronNode = abstractDataList[intronIndex];

                var objectValue = intronNode as DS.ObjectValue;
                if (objectValue == null)
                {
                    throw new ApplicationException(
                        $"Could not transform the AbstractData object into an ObjectValue: [{abstractDataList[intronIndex].GetType()}]");
                }

                ParseReference(objectValue, introns[intronIndex], dataStore);
            }
        }
    }
}
