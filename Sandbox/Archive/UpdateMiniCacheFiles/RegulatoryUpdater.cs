using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Illumina.VariantAnnotation.DataStructures;
using Illumina.VariantAnnotation.FileHandling;

namespace UpdateMiniCacheFiles
{
    public class RegulatoryUpdater : IUpdater
    {
        #region members

        private readonly string _rootCacheDirectory;
        private readonly string _rootUnfilteredCacheDirectory;
        private readonly ushort _desiredVepVersion;

        #endregion

        // constructor
        public RegulatoryUpdater(string rootCacheDirectory, string rootUnfilteredCacheDirectory, ushort desiredVepVersion)
        {
            _rootCacheDirectory           = rootCacheDirectory;
            _rootUnfilteredCacheDirectory = rootUnfilteredCacheDirectory;
            _desiredVepVersion            = desiredVepVersion;
        }

        /// <summary>
        /// updates a regulatory region 
        /// mini-cache filename example: ENSR00000079256_chr1_Ensembl72_reg.ndb
        /// </summary>
        public UpdateStatus Update(string oldMiniCachePath, Match match)
        {
            var regulatoryFeatureId  = match.Groups[1].Value;
            var chromosome           = match.Groups[2].Value;
            var transcriptDataSource = match.Groups[3].Value;

            var cachePath        = GetTranscriptCachePath(transcriptDataSource, chromosome, false);
            var newMiniCachePath = GetRegulatoryMiniCachePath(Path.GetDirectoryName(oldMiniCachePath), regulatoryFeatureId, chromosome, transcriptDataSource);

            // sanity check: make sure the cache file exists
            if (!File.Exists(cachePath))
            {
                throw new FileNotFoundException($"The following file does not exist: {cachePath}");
            }

            // grab the desired transcript
            NirvanaDataStore dataStore;
            IntervalTree<Transcript> intervalTree;
            DataStoreUtilities.GetDataStore(cachePath, out dataStore, out intervalTree);

            bool foundRegulatoryFeature = false;
            var desiredDataStore = new NirvanaDataStore
            {
                Genes              = new List<Gene>(),
                Transcripts        = new List<Transcript>(),
                RegulatoryFeatures = new List<RegulatoryFeature>()
            };

            foreach (var regulatoryFeature in dataStore.RegulatoryFeatures.Where(regulatory => regulatory.StableId == regulatoryFeatureId))
            {
                desiredDataStore.RegulatoryFeatures.Add(regulatoryFeature);
                foundRegulatoryFeature = true;
                break;
            }

            if (!foundRegulatoryFeature) return UpdateStatus.IdNotFound;

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
        /// mini-cache filename example: ENSR00000079256_chr1_Ensembl72_reg.ndb
        /// </summary>
        private string GetRegulatoryMiniCachePath(string rootDir, string transcriptId, string chromosome, string transcriptDataSource)
        {
            return Path.Combine(rootDir,
                transcriptId + '_' + chromosome + "_" + transcriptDataSource + _desiredVepVersion + "_reg.ndb");
        }
    }
}
