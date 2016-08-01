using System;
using System.Collections.Generic;
using System.Linq;
using Illumina.DataDumperImport.Utilities;
using Illumina.ErrorHandling.Exceptions;
using Illumina.VariantAnnotation.DataStructures;
using Illumina.VariantAnnotation.FileHandling;

namespace ExtractRegulatoryFeatures
{
    public class RegulatoryFeatureExtractor
    {
        #region members

        private readonly NirvanaDataStore _dataStore;
        private readonly IntervalTree<RegulatoryFeature> _regulatoryFeatureIntervalTree;

        #endregion

        // constructor
        public RegulatoryFeatureExtractor(string inputCachePath)
        {
            _dataStore                     = new NirvanaDataStore();
            _regulatoryFeatureIntervalTree = new IntervalTree<RegulatoryFeature>();

            LoadData(inputCachePath);
        }

        /// <summary>
        /// loads all of the transcripts contained in the specified cache file
        /// </summary>
        private void LoadData(string inputCachePath)
        {
            using (var reader = new NirvanaDatabaseReader(inputCachePath))
            {
                var transcriptIntervalTree = new IntervalTree<Transcript>();
                reader.PopulateData(_dataStore, transcriptIntervalTree, _regulatoryFeatureIntervalTree);
            }
        }

        /// <summary>
        /// locates the transcript from our datastore and adds it to the list
        /// </summary>
        private void GetRegulatoryFeature(string regulatoryFeatureId, List<RegulatoryFeature> overlappingRegulatoryFeatures)
        {
            bool foundRegulatoryFeature = false;

            foreach (var regulatoryFeature in _dataStore.RegulatoryFeatures.Where(regulatory => regulatory.StableId == regulatoryFeatureId))
            {
                overlappingRegulatoryFeatures.Add(regulatoryFeature);
                foundRegulatoryFeature = true;
                break;
            }

            if (!foundRegulatoryFeature)
            {
                throw new UserErrorException("Could not find " + regulatoryFeatureId + " in the datastore.");
            }

            // display all the overlapping regulatory features
            Console.WriteLine("Found the regulatory feature:");
            foreach (var regulatoryFeature in overlappingRegulatoryFeatures) Console.WriteLine("- {0}", regulatoryFeature.StableId);
        }

        /// <summary>
        /// extracts all of the variants that overlap with this variant
        /// </summary>
        public void Extract(string regulatoryFeatureId, string outputCachePath)
        {
            var overlappingDataStore = new NirvanaDataStore {
				Genes              = new List<Gene>(),
				Transcripts        = new List<Transcript>(),
                RegulatoryFeatures = new List<RegulatoryFeature>()
            };

            GetRegulatoryFeature(regulatoryFeatureId, overlappingDataStore.RegulatoryFeatures);

            // write the overlapping transcripts to an output file
            DataStoreUtilities.PopulateTranscriptObjects(overlappingDataStore);

            // set the cache header
            overlappingDataStore.CacheHeader = new NirvanaDatabaseHeader(_dataStore.CacheHeader.ReferenceSequenceName,
                DateTime.UtcNow.Ticks, _dataStore.CacheHeader.VepReleaseTicks, _dataStore.CacheHeader.VepVersion,
                _dataStore.CacheHeader.SchemaVersion, _dataStore.CacheHeader.DataVersion,
                _dataStore.CacheHeader.GenomeAssembly, _dataStore.CacheHeader.TranscriptDataSource);
            
            // write the Nirvana database file
            using (var writer = new NirvanaDatabaseWriter(outputCachePath))
            {
                writer.Write(overlappingDataStore, _dataStore.CacheHeader.ReferenceSequenceName);
            }
        }
    }
}
