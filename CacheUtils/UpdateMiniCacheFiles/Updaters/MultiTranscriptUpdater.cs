using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CacheUtils.UpdateMiniCacheFiles.DataStructures;
using CacheUtils.UpdateMiniCacheFiles.Utilities;

namespace CacheUtils.UpdateMiniCacheFiles.Updaters
{
    public sealed class MultiTranscriptUpdater : IUpdater
    {
        private readonly List<string> _ids;
        // ReSharper disable once UnassignedGetOnlyAutoProperty
        public ushort RefIndex { get; }
        public string TranscriptDataSource { get; }

        private static readonly Regex FilenameRegex = new Regex("^(.+?)_(chr[^_]+)_(\\D+)(\\d+)\\.ndb", RegexOptions.Compiled);

        /// <summary>
        /// constructor
        /// </summary>
        public MultiTranscriptUpdater(List<string> ids, string transcriptDataSource)
        {
            _ids = ids;
            TranscriptDataSource = transcriptDataSource;
        }

        // ReSharper disable once UnusedMember.Global
        public static Match GetMatch(string ndbPath) => FilenameRegex.Match(ndbPath);

        /// <summary>
        /// updates multiple transcripts
        /// mini-cache filename example: ENST00000255416_ENST00000255417_ENST00000255418_Ensembl79_multi.ndb
        /// </summary>
        public UpdateStatus Update(DataBundle bundle, string outputCacheDir, ushort desiredVepVersion, List<string> outputFiles)
        {
            var packets = MiniCacheUtilities.GetDesiredTranscripts(bundle, _ids);
            if (packets.Count == 0) return UpdateStatus.IdNotFound;

            var refIndices = new HashSet<ushort>();
            foreach (var packet in packets)
            {
                bundle.Load(packet.ReferenceIndex);
                refIndices.Add(packet.ReferenceIndex);

                packet.SiftPrediction = packet.Transcript.SiftIndex == -1
                    ? null
                    : bundle.SiftCache.Predictions[packet.Transcript.SiftIndex];

                packet.PolyPhenPrediction = packet.Transcript.PolyPhenIndex == -1
                    ? null
                    : bundle.PolyPhenCache.Predictions[packet.Transcript.PolyPhenIndex];
            }
            
            var numRefSeqs  = bundle.Sequence.Renamer.NumRefSeqs;

            var sift     = PredictionUtilities.GetStaging(bundle, true, packets, numRefSeqs);
            var polyphen = PredictionUtilities.GetStaging(bundle, false, packets, numRefSeqs);
            PredictionUtilities.FixIndices(packets);

            var outputCache = MiniCacheUtilities.CreateCache(bundle.Cache.Header, packets);

            var outputStub = GetOutputStub(outputCacheDir, packets, TranscriptDataSource, desiredVepVersion);

            MiniCacheUtilities.WriteTranscriptCache(outputCache, outputStub, outputFiles);
            MiniCacheUtilities.WritePredictionCaches(sift, polyphen, outputStub, outputFiles);
            MiniCacheUtilities.WriteEmptyBases(bundle, refIndices, outputStub, outputFiles);

            return UpdateStatus.Current;
        }

        /// <summary>
        /// returns a new mini-cache path
        /// mini-cache filename example: ENST00000255416_ENST00000255417_ENST00000255418_Ensembl79_multi.ndb
        /// </summary>
        private static string GetOutputStub(string rootDir, List<TranscriptPacket> packets, string transcriptDataSource,
            ushort desiredVepVersion)
        {
            var ids = packets.Select(packet => packet.Transcript.Id.ToString()).ToList();

            var transcripts = string.Join("_", ids);
            return Path.Combine(rootDir, $"{transcripts}_{transcriptDataSource}{desiredVepVersion}_multi");
        }
    }
}
