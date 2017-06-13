using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CacheUtils.UpdateMiniCacheFiles.DataStructures;
using CacheUtils.UpdateMiniCacheFiles.Utilities;

namespace CacheUtils.UpdateMiniCacheFiles.Updaters
{
    public sealed class PositionRangeUpdater : IUpdater
    {
        private readonly int _position;
        private readonly int _endPosition;
        public ushort RefIndex { get; }
        public string TranscriptDataSource { get; }

        private static readonly Regex FilenameRegex = new Regex("^(chr[^_]+)_(\\d+)_(\\d+)_(\\D+)(\\d+)_pos\\.ndb", RegexOptions.Compiled);

        /// <summary>
        /// constructor
        /// </summary>
        public PositionRangeUpdater(ushort refIndex, int position, int endPosition, string transcriptDataSource)
        {
            RefIndex             = refIndex;
            _position            = position;
            _endPosition         = endPosition;
            TranscriptDataSource = transcriptDataSource;
        }

        public static Match GetMatch(string ndbPath) => FilenameRegex.Match(ndbPath);

        /// <summary>
        /// updates a position 
        /// mini-cache filename example: chr1_713044_755966_RefSeq79_pos.ndb
        /// </summary>
        public UpdateStatus Update(DataBundle bundle, string outputCacheDir, ushort desiredVepVersion, List<string> outputFiles)
        {
            bundle.Load(RefIndex);

            string vcfLine = $"{bundle.Sequence.Renamer.UcscReferenceNames[RefIndex]}\t{_position}\t.\tC\t<CN0>\t100\tPASS\tEND={_endPosition};SVTYPE=CNV";
            var transcripts = MiniCacheUtilities.GetTranscriptsByVcf(bundle, vcfLine);
            if (transcripts == null) return UpdateStatus.IdNotFound;

            var packets = transcripts.Select(transcript => new TranscriptPacket(transcript)).ToList();

            foreach (var packet in packets)
            {
                bundle.Load(packet.ReferenceIndex);

                packet.SiftPrediction = packet.Transcript.SiftIndex == -1
                    ? null
                    : bundle.SiftCache.Predictions[packet.Transcript.SiftIndex];

                packet.PolyPhenPrediction = packet.Transcript.PolyPhenIndex == -1
                    ? null
                    : bundle.PolyPhenCache.Predictions[packet.Transcript.PolyPhenIndex];
            }

            var numRefSeqs = bundle.Sequence.Renamer.NumRefSeqs;

            var sift = PredictionUtilities.GetStaging(bundle, true, packets, numRefSeqs);
            var polyphen = PredictionUtilities.GetStaging(bundle, false, packets, numRefSeqs);
            PredictionUtilities.FixIndices(packets);

            var outputCache = MiniCacheUtilities.CreateCache(bundle.Cache.Header, packets);

            var outputStub = GetOutputStub(outputCacheDir, bundle.Sequence.Renamer.UcscReferenceNames[RefIndex],
                _position, _endPosition, TranscriptDataSource, desiredVepVersion);

            MiniCacheUtilities.WriteTranscriptCache(outputCache, outputStub, outputFiles);
            MiniCacheUtilities.WritePredictionCaches(sift, polyphen, outputStub, outputFiles);
            MiniCacheUtilities.WriteBases(bundle, packets, outputStub, outputFiles);

            return UpdateStatus.Current;
        }

        /// <summary>
        /// returns a new mini-cache path
        /// mini-cache filename example: chr1_713044_755966_RefSeq79_pos.ndb
        /// </summary>
        private static string GetOutputStub(string rootDir, string chromosome, int start, int end,
            string transcriptDataSource, ushort desiredVepVersion)
        {
            return Path.Combine(rootDir, $"{chromosome}_{start}_{end}_{transcriptDataSource}{desiredVepVersion}_pos");
        }
    }
}
