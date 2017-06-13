using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using CacheUtils.UpdateMiniCacheFiles.Updaters;
using CacheUtils.UpdateMiniCacheFiles.Utilities;
using ErrorHandling.Exceptions;
using VariantAnnotation.DataStructures.Transcript;
using VariantAnnotation.Utilities;

namespace CacheUtils.UpdateMiniCacheFiles.DataStructures
{
    public class CacheFile
    {
        public readonly MiniCacheType Type;
        public readonly string CachePath;
        public readonly TranscriptDataSource TranscriptDataSource;
        public readonly IUpdater Updater;
        public readonly ushort ReferenceIndex;

        public readonly List<string> InputFiles  = new List<string>();
        public readonly List<string> OutputFiles = new List<string>();

        public UpdateStatus Status = UpdateStatus.Unknown;

        /// <summary>
        /// constructor
        /// </summary>
        private CacheFile(string path, ushort refIndex, TranscriptDataSource ds, MiniCacheType type, IUpdater updater)
        {
            CachePath            = path;
            TranscriptDataSource = ds;
            Type                 = type;
            ReferenceIndex       = refIndex;
            Updater              = updater;

            AddInputFiles(FileOperations.GetFullPathWithoutExtension(path));
        }

        private void AddInputFiles(string inputStub)
        {
            AddInputFile(inputStub + ".ndb");
            AddInputFile(inputStub + ".sift");
            AddInputFile(inputStub + ".polyphen");
            AddInputFile(inputStub + ".bases");
        }

        private void AddInputFile(string inputPath)
        {
            if(File.Exists(inputPath)) InputFiles.Add(inputPath);
        }

        public static CacheFile Create(string ndbPath, ChromosomeRenamer renamer)
        {
            // transcript id
            var cacheFile = TryMatchFilename(ndbPath, TranscriptUpdater.GetMatch, MiniCacheType.Transcript, renamer);
            if (cacheFile != null) return cacheFile;

            // regulatory
            cacheFile = TryMatchFilename(ndbPath, RegulatoryUpdater.GetMatch, MiniCacheType.Regulatory, renamer);
            if (cacheFile != null) return cacheFile;

            // position
            cacheFile = TryMatchFilename(ndbPath, PositionUpdater.GetMatch, MiniCacheType.Position, renamer);
            if (cacheFile != null) return cacheFile;

            // position range
            cacheFile = TryMatchFilename(ndbPath, PositionRangeUpdater.GetMatch, MiniCacheType.PositionRange, renamer);
            if (cacheFile != null) return cacheFile;

            // unknown
            return new CacheFile(ndbPath, 0, TranscriptDataSource.None, MiniCacheType.Unknown, null);
        }

        private static CacheFile TryMatchFilename(string ndbPath, Func<string, Match> matcher, MiniCacheType type,
            ChromosomeRenamer renamer)
        {
            string filename = Path.GetFileName(ndbPath);
            if (filename == null) return null;

            var match = matcher(filename);
            if (!match.Success) return null;

            IUpdater updater;
            string id, transcriptDataSource;
            int position;
            ushort refIndex;

            switch (type)
            {
                case MiniCacheType.Transcript:
                    var tuple            = FormatUtilities.SplitVersion(match.Groups[1].Value);
                    id                   = tuple.Item1;
                    refIndex             = renamer.GetReferenceIndex(match.Groups[2].Value);
                    transcriptDataSource = match.Groups[3].Value;
                    updater              = new TranscriptUpdater(id, refIndex, transcriptDataSource);
                    break;

                case MiniCacheType.Regulatory:
                    id                   = match.Groups[1].Value;
                    refIndex             = renamer.GetReferenceIndex(match.Groups[2].Value);
                    transcriptDataSource = match.Groups[3].Value;
                    updater              = new RegulatoryUpdater(id, refIndex, transcriptDataSource);
                    break;

                case MiniCacheType.Position:
                    refIndex             = renamer.GetReferenceIndex(match.Groups[1].Value);
                    position             = int.Parse(match.Groups[2].Value);
                    string refAllele     = match.Groups[3].Value;
                    string altAllele     = match.Groups[4].Value;
                    transcriptDataSource = match.Groups[5].Value;
                    updater              = new PositionUpdater(refIndex, position, refAllele, altAllele, transcriptDataSource);
                    break;

                case MiniCacheType.PositionRange:
                    refIndex             = renamer.GetReferenceIndex(match.Groups[1].Value);
                    position             = int.Parse(match.Groups[2].Value);
                    int endPosition      = int.Parse(match.Groups[3].Value);
                    transcriptDataSource = match.Groups[4].Value;
                    updater              = new PositionRangeUpdater(refIndex, position, endPosition, transcriptDataSource);
                    break;

                default:
                    throw new GeneralException($"Unexpected mini-cache type encountered: {type}");
            }

            return new CacheFile(ndbPath, updater.RefIndex, ConvertTranscriptDataSource(updater.TranscriptDataSource),
                type, updater);
        }

        private static TranscriptDataSource ConvertTranscriptDataSource(string ds)
        {
            TranscriptDataSource ret;

            switch (ds.ToLower())
            {
                case "ensembl":
                    ret = TranscriptDataSource.Ensembl;
                    break;
                case "refseq":
                    ret = TranscriptDataSource.RefSeq;
                    break;
                case "both":
                    ret = TranscriptDataSource.BothRefSeqAndEnsembl;
                    break;
                default:
                    throw new UserErrorException($"Unknown transcript data source was specified: {ds}");
            }

            return ret;
        }
    }
}
