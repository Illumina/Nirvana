using System;
using System.IO;
using System.Text.RegularExpressions;
using Illumina.VariantAnnotation.DataStructures;
using Illumina.VariantAnnotation.FileHandling;

namespace UpdateMiniCacheFiles
{
    public class PositionRangeUpdater : IUpdater
    {
        #region members

        private readonly string _rootCacheDirectory;
        private readonly string _rootUnfilteredCacheDirectory;
        private readonly ushort _desiredVepVersion;

        private const int DownstreamLength = 5000;
        private const int UpstreamLength = 5000;

        #endregion

        // constructor
        public PositionRangeUpdater(string rootCacheDirectory, string rootUnfilteredCacheDirectory,
            ushort desiredVepVersion)
        {
            _rootCacheDirectory = rootCacheDirectory;
            _rootUnfilteredCacheDirectory = rootUnfilteredCacheDirectory;
            _desiredVepVersion = desiredVepVersion;
        }

        /// <summary>
        /// updates a position 
        /// mini-cache filename example: chr1_713044_755966_RefSeq79_pos.ndb
        /// </summary>
        public UpdateStatus Update(string oldMiniCachePath, Match match)
        {
            var chromosome           = match.Groups[1].Value;
            var start                = match.Groups[2].Value;
            var end                  = match.Groups[3].Value;
            var transcriptDataSource = match.Groups[4].Value;

            var cachePath = GetTranscriptCachePath(transcriptDataSource, chromosome, false);
            var newMiniCachePath = GetPositionRangeMiniCachePath(Path.GetDirectoryName(oldMiniCachePath), chromosome, start, end, transcriptDataSource);

            // sanity check: make sure the cache file exists
            if (!File.Exists(cachePath))
            {
                throw new FileNotFoundException($"The following file does not exist: {cachePath}");
            }

            // grab the desired transcript
            NirvanaDataStore desiredDataStore;
            IntervalTree<Transcript> intervalTree;
            DataStoreUtilities.GetDataStore(cachePath, out desiredDataStore, out intervalTree);

            // create a new variant containing the specified variant
            var variant = new VariantFeature();

            string vcfLine = $"{chromosome}\t{start}\t.\tC\t<CN0>\t100\tPASS\tEND={end};SVTYPE=CNV";

            variant.ParseVcfLine(vcfLine);

            var transcriptInterval = new IntervalTree<Transcript>.Interval(variant.ReferenceName,
                variant.VcfReferenceBegin - UpstreamLength, variant.VcfReferenceEnd + DownstreamLength);
            intervalTree.GetAllOverlappingValues(transcriptInterval, desiredDataStore.Transcripts);

            if (desiredDataStore.Transcripts.Count == 0) return UpdateStatus.IdNotFound;

            // set the cache header
            desiredDataStore.CacheHeader = new NirvanaDatabaseHeader(
                desiredDataStore.CacheHeader.ReferenceSequenceName, DateTime.UtcNow.Ticks,
                desiredDataStore.CacheHeader.VepReleaseTicks, _desiredVepVersion,
                desiredDataStore.CacheHeader.SchemaVersion, desiredDataStore.CacheHeader.DataVersion,
                desiredDataStore.CacheHeader.GenomeAssembly, desiredDataStore.CacheHeader.TranscriptDataSource);

            DataStoreUtilities.WriteDataStore(desiredDataStore, newMiniCachePath);
            if (oldMiniCachePath != newMiniCachePath) File.Delete(oldMiniCachePath);

            return UpdateStatus.Updated;
        }

        /// <summary>
        /// returns the transcript cache path
        /// </summary>
        private string GetTranscriptCachePath(string transcriptDataSource, string chromosome, bool useUnfiltered)
        {
            var rootDirectory = useUnfiltered ? _rootUnfilteredCacheDirectory : _rootCacheDirectory;
            var cachePath = Path.Combine(rootDirectory, transcriptDataSource, _desiredVepVersion.ToString(),
                chromosome + ".ndb");

            // sanity check: make sure the cache path exists
            if (!File.Exists(cachePath)) throw new FileNotFoundException(cachePath);

            return cachePath;
        }

        /// <summary>
        /// returns a new mini-cache path
        /// mini-cache filename example: chr1_713044_755966_RefSeq79_pos.ndb
        /// </summary>
        private string GetPositionRangeMiniCachePath(string rootDir, string chromosome, string start, string end, string transcriptDataSource)
        {
            return
                Path.Combine(rootDir,
                    $"{chromosome}_{start}_{end}_{transcriptDataSource}{_desiredVepVersion}_pos.ndb");
        }
    }
}
