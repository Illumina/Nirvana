using System.IO;
using System.Linq;
using VariantAnnotation.DataStructures;
using VariantAnnotation.FileHandling;

namespace UnitTests.Utilities
{
    internal static class CacheUtilities
    {
        /// <summary>
        /// Returns the specified transcript from a cache file
        /// </summary>
        internal static Transcript GetTranscript(string cacheFile, string transcriptId)
        {
            var transcriptIntervalTree = new IntervalTree<Transcript>();
            var regulatoryIntervalTree = new IntervalTree<RegulatoryFeature>();
            var dataStore = new NirvanaDataStore();

            using (var reader = new NirvanaDatabaseReader(Path.Combine("Resources", "Caches", cacheFile)))
            {
                reader.PopulateData(dataStore, transcriptIntervalTree, regulatoryIntervalTree);
            }

            return dataStore.Transcripts.FirstOrDefault(transcript => transcript.StableId == transcriptId);
        }
    }
}
