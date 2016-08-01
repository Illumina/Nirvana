using Illumina.VariantAnnotation.DataStructures;
using Illumina.VariantAnnotation.FileHandling;

namespace UpdateMiniCacheFiles
{
    internal static class DataStoreUtilities
    {
        /// <summary>
        /// retrieves the datastore for the specified cache file
        /// </summary>
        internal static void GetDataStore(string cachePath, out NirvanaDataStore dataStore, out IntervalTree<Transcript> transcriptIntervalTree)
        {
            dataStore              = new NirvanaDataStore();
            transcriptIntervalTree = new IntervalTree<Transcript>();

            using (var reader = new NirvanaDatabaseReader(cachePath))
            {
                reader.PopulateData(dataStore, transcriptIntervalTree);
            }
        }

        internal static void WriteDataStore(NirvanaDataStore dataStore, string outputPath)
        {
            // write the overlapping transcripts to an output file
            Illumina.DataDumperImport.Utilities.DataStoreUtilities.PopulateTranscriptObjects(dataStore);

            // write the Nirvana database file
            using (var writer = new NirvanaDatabaseWriter(outputPath))
            {
                writer.Write(dataStore, dataStore.CacheHeader.ReferenceSequenceName);
            }
        }
    }
}
