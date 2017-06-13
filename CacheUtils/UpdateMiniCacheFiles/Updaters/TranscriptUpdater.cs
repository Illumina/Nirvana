using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using CacheUtils.UpdateMiniCacheFiles.DataStructures;
using CacheUtils.UpdateMiniCacheFiles.Utilities;

namespace CacheUtils.UpdateMiniCacheFiles.Updaters
{
    public sealed class TranscriptUpdater : IUpdater
    {
        private readonly string _transcriptId;
        public ushort RefIndex { get; }
        public string TranscriptDataSource { get; }

        private static readonly Regex FilenameRegex = new Regex("^(.+?)_(chr[^_]+)_(\\D+)(\\d+)\\.ndb", RegexOptions.Compiled);

        /// <summary>
        /// constructor
        /// </summary>
        public TranscriptUpdater(string id, ushort refIndex, string transcriptDataSource)
        {
            _transcriptId        = id;
            RefIndex             = refIndex;
            TranscriptDataSource = transcriptDataSource;
        }

        public static Match GetMatch(string ndbPath) => FilenameRegex.Match(ndbPath);

        /// <summary>
        /// updates a transcript 
        /// mini-cache filename example: ENST00000255416_chr1_Ensembl79.ndb
        /// </summary>
        public UpdateStatus Update(DataBundle bundle, string outputCacheDir, ushort desiredVepVersion, List<string> outputFiles)
        {
            var transcript = MiniCacheUtilities.GetDesiredTranscript(bundle, _transcriptId, RefIndex);
            if (transcript == null) return UpdateStatus.IdNotFound;

            var packets = new List<TranscriptPacket> { new TranscriptPacket(transcript) };

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

            var sift = PredictionUtilities.GetStaging(bundle, true, packets, numRefSeqs);
            var polyphen = PredictionUtilities.GetStaging(bundle, false, packets, numRefSeqs);
            PredictionUtilities.FixIndices(packets);

            var outputCache = MiniCacheUtilities.CreateCache(bundle.Cache.Header, packets);

            var outputStub = GetOutputStub(outputCacheDir, _transcriptId,
                bundle.Sequence.Renamer.UcscReferenceNames[transcript.ReferenceIndex], TranscriptDataSource,
                desiredVepVersion);

            MiniCacheUtilities.WriteTranscriptCache(outputCache, outputStub, outputFiles);
            MiniCacheUtilities.WritePredictionCaches(sift, polyphen, outputStub, outputFiles);
            MiniCacheUtilities.WriteBases(bundle, packets, outputStub, outputFiles);

            return UpdateStatus.Current;
        }

        /// <summary>
        /// returns a new mini-cache path
        /// mini-cache filename example: ENST00000255416_UF_chr1_Ensembl79.ndb
        /// </summary>
        private static string GetOutputStub(string rootDir, string id, string chromosome, string transcriptDataSource,
            ushort desiredVepVersion)
        {
            return Path.Combine(rootDir, $"{id}_{chromosome}_{transcriptDataSource}{desiredVepVersion}");
        }
    }
}
