using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using CacheUtils.UpdateMiniCacheFiles.DataStructures;
using CacheUtils.UpdateMiniCacheFiles.Utilities;
using VariantAnnotation.DataStructures;
using VariantAnnotation.FileHandling;
using VariantAnnotation.FileHandling.TranscriptCache;

namespace CacheUtils.UpdateMiniCacheFiles.Updaters
{
    public sealed class RegulatoryUpdater : IUpdater
    {
        private readonly string _regulatoryId;
        public ushort RefIndex { get; }
        public string TranscriptDataSource { get; }

        private static readonly Regex FilenameRegex = new Regex("^(.+?)_(chr[^_]+)_(\\D+)(\\d+)_reg\\.ndb", RegexOptions.Compiled);

        /// <summary>
        /// constructor
        /// </summary>
        public RegulatoryUpdater(string id, ushort refIndex, string transcriptDataSource)
        {
            _regulatoryId        = id;
            RefIndex             = refIndex;
            TranscriptDataSource = transcriptDataSource;
        }

        public static Match GetMatch(string ndbPath) => FilenameRegex.Match(ndbPath);

        /// <summary>
        /// updates a regulatory region 
        /// mini-cache filename example: ENSR00000079256_chr1_Ensembl72_reg.ndb
        /// </summary>
        public UpdateStatus Update(DataBundle bundle, string outputCacheDir, ushort desiredVepVersion, List<string> outputFiles)
        {
            var regulatoryElement = MiniCacheUtilities.GetDesiredRegulatoryElement(bundle, _regulatoryId);
            if (regulatoryElement == null) return UpdateStatus.IdNotFound;

            bundle.Load(regulatoryElement.ReferenceIndex);

            var outputCache = CreateCache(bundle.Cache.Header, new[] { regulatoryElement });

            var outputStub = GetOutputStub(outputCacheDir, _regulatoryId,
                bundle.Sequence.Renamer.UcscReferenceNames[regulatoryElement.ReferenceIndex], TranscriptDataSource,
                desiredVepVersion);

            var refIndices = new HashSet<ushort> { regulatoryElement.ReferenceIndex };

            MiniCacheUtilities.WriteTranscriptCache(outputCache, outputStub, outputFiles);
            MiniCacheUtilities.WriteEmptyBases(bundle, refIndices, outputStub, outputFiles);

            return UpdateStatus.Current;
        }

        private static GlobalCache CreateCache(IFileHeader oldHeader, RegulatoryElement[] regulatoryElements)
        {
            var header = new FileHeader(CacheConstants.Identifier, CacheConstants.SchemaVersion,
                CacheConstants.DataVersion, oldHeader.TranscriptSource, DateTime.Now.Ticks, oldHeader.GenomeAssembly,
                oldHeader.Custom);

            return new GlobalCache(header, null, regulatoryElements, null, null, null, null);
        }

        /// <summary>
        /// returns a new mini-cache path
        /// mini-cache filename example: ENSR00000079256_chr1_Ensembl72_reg.ndb
        /// </summary>
        private static string GetOutputStub(string rootDir, string transcriptId, string chromosome,
            string transcriptDataSource, ushort desiredVepVersion)
        {
            return Path.Combine(rootDir, $"{transcriptId}_{chromosome}_{transcriptDataSource}{desiredVepVersion}_reg");
        }
    }
}
