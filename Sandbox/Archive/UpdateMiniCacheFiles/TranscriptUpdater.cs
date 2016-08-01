using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Illumina.VariantAnnotation.DataStructures;
using Illumina.VariantAnnotation.FileHandling;

namespace UpdateMiniCacheFiles
{
    public class TranscriptUpdater : IUpdater
    {
        #region members

        private readonly string _rootCacheDirectory;
        private readonly string _rootUnfilteredCacheDirectory;
        private readonly ushort _desiredVepVersion;

        #endregion

        // constructor
        public TranscriptUpdater(string rootCacheDirectory, string rootUnfilteredCacheDirectory, ushort desiredVepVersion)
        {
            _rootCacheDirectory           = rootCacheDirectory;
            _rootUnfilteredCacheDirectory = rootUnfilteredCacheDirectory;
            _desiredVepVersion            = desiredVepVersion;
        }

        /// <summary>
        /// updates a transcript 
        /// mini-cache filename example: ENST00000255416_UF_chr1_Ensembl79.ndb
        /// </summary>
        public UpdateStatus Update(string oldMiniCachePath, Match match)
        {
            var transcriptId         = match.Groups[1].Value;
            var useUnfiltered        = !string.IsNullOrEmpty(match.Groups[2].Value);
            var chromosome           = match.Groups[3].Value;
            var transcriptDataSource = match.Groups[4].Value;

            var cachePath        = GetTranscriptCachePath(transcriptDataSource, chromosome, useUnfiltered);
            var newMiniCachePath = GetTranscriptMiniCachePath(Path.GetDirectoryName(oldMiniCachePath), transcriptId, chromosome, transcriptDataSource, useUnfiltered);

            // sanity check: make sure the cache file exists
            if (!File.Exists(cachePath))
            {
                throw new FileNotFoundException($"The following file does not exist: {cachePath}");
            }

            // grab the desired transcript
            NirvanaDataStore dataStore;
            IntervalTree<Transcript> intervalTree;
            DataStoreUtilities.GetDataStore(cachePath, out dataStore, out intervalTree);

            bool foundTranscript = false;
            var desiredDataStore = new NirvanaDataStore
            {
                Genes              = new List<Gene>(),
                Transcripts        = new List<Transcript>(),
                RegulatoryFeatures = new List<RegulatoryFeature>()
            };

            foreach (var transcript in dataStore.Transcripts.Where(transcript => transcript.StableId == transcriptId))
            {
                desiredDataStore.Transcripts.Add(transcript);
                foundTranscript = true;
                break;
            }

            // populate the genes list
            desiredDataStore.Genes = Illumina.DataDumperImport.Utilities.DataStoreUtilities.GetGenesSubset(dataStore.Genes, desiredDataStore.Transcripts);

            if (!foundTranscript) return UpdateStatus.IdNotFound;

            // set the cache header
            desiredDataStore.CacheHeader = new NirvanaDatabaseHeader(dataStore.CacheHeader.ReferenceSequenceName,
                DateTime.UtcNow.Ticks, dataStore.CacheHeader.VepReleaseTicks, _desiredVepVersion,
                dataStore.CacheHeader.SchemaVersion, dataStore.CacheHeader.DataVersion,
                dataStore.CacheHeader.GenomeAssembly, dataStore.CacheHeader.TranscriptDataSource);

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
            var cachePath = Path.Combine(rootDirectory, transcriptDataSource, _desiredVepVersion.ToString(), chromosome + ".ndb");

            // sanity check: make sure the cache path exists
            if (!File.Exists(cachePath)) throw new FileNotFoundException(cachePath);

            return cachePath;
        }

        /// <summary>
        /// returns a new mini-cache path
        /// mini-cache filename example: ENST00000255416_UF_chr1_Ensembl79.ndb
        /// </summary>
        private string GetTranscriptMiniCachePath(string rootDir, string transcriptId, string chromosome, string transcriptDataSource, bool useUnfiltered)
        {
            return Path.Combine(rootDir,
                transcriptId + '_' + (useUnfiltered ? "UF_" : "") + chromosome + "_" + transcriptDataSource +
                _desiredVepVersion + ".ndb");
        }
    }
}
