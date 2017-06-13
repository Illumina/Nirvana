using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CacheUtils.UpdateMiniCacheFiles.DataStructures;
using CacheUtils.UpdateMiniCacheFiles.Utilities;

namespace CacheUtils.UpdateMiniCacheFiles.Updaters
{
    public sealed class PositionUpdater : IUpdater
    {
        private readonly int _position;
        private readonly string _refAllele;
        private readonly string _altAllele;
        public ushort RefIndex { get; }
        public string TranscriptDataSource { get; }

        private static readonly Regex FilenameRegex = new Regex("^(chr[^_]+)_(\\d+)_([^_]+)_([^_]+)_(\\D+)(\\d+)_pos\\.ndb", RegexOptions.Compiled);

        /// <summary>
        /// constructor
        /// </summary>
        public PositionUpdater(ushort refIndex, int position, string refAllele, string altAllele,
            string transcriptDataSource)
        {
            RefIndex             = refIndex;
            _position            = position;
            _refAllele           = refAllele;
            _altAllele           = altAllele;
            TranscriptDataSource = transcriptDataSource;
        }

        public static Match GetMatch(string ndbPath) => FilenameRegex.Match(ndbPath);

        /// <summary>
        /// updates a position 
        /// mini-cache filename example: chr1_59758869_T_G_UF_RefSeq79_pos.ndb
        /// </summary>
        public UpdateStatus Update(DataBundle bundle, string outputCacheDir, ushort desiredVepVersion, List<string> outputFiles)
        {
            bundle.Load(RefIndex);

            string vcfLine  = $"{bundle.Sequence.Renamer.UcscReferenceNames[RefIndex]}\t{_position}\t.\t{_refAllele}\t{_altAllele}\t20\tPASS\t.";
            var transcripts = MiniCacheUtilities.GetTranscriptsByVcf(bundle, vcfLine);
            if (transcripts.Length == 0) return UpdateStatus.IdNotFound;

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
            
            var numRefSeqs  = bundle.Sequence.Renamer.NumRefSeqs;

            var sift     = PredictionUtilities.GetStaging(bundle, true, packets, numRefSeqs);
            var polyphen = PredictionUtilities.GetStaging(bundle, false, packets, numRefSeqs);
            PredictionUtilities.FixIndices(packets);

            var outputCache = MiniCacheUtilities.CreateCache(bundle.Cache.Header, packets);

            var outputStub = GetOutputStub(outputCacheDir, bundle.Sequence.Renamer.UcscReferenceNames[RefIndex],
                _position, _refAllele, _altAllele, TranscriptDataSource, desiredVepVersion);

            MiniCacheUtilities.WriteTranscriptCache(outputCache, outputStub, outputFiles);
            MiniCacheUtilities.WritePredictionCaches(sift, polyphen, outputStub, outputFiles);
            MiniCacheUtilities.WriteBases(bundle, packets, outputStub, outputFiles);

            return UpdateStatus.Current;
        }

        /// <summary>
        /// returns a new mini-cache path
        /// mini-cache filename example: chr1_115256529_G_TAA_UF_RefSeq79_pos.ndb
        /// </summary>
        private static string GetOutputStub(string rootDir, string chromosome, int position, string refAllele,
            string altAllele, string transcriptDataSource, ushort desiredVepVersion)
        {
            return Path.Combine(rootDir, $"{chromosome}_{position}_{refAllele}_{altAllele}_{transcriptDataSource}{desiredVepVersion}_pos");
        }
    }
}