using System;
using System.Collections.Generic;
using System.Linq;
using Illumina.DataDumperImport.Utilities;
using Illumina.ErrorHandling.Exceptions;
using Illumina.VariantAnnotation.DataStructures;
using Illumina.VariantAnnotation.FileHandling;

namespace ExtractTranscripts
{
    public class TranscriptExtractor
    {
        #region members

        private readonly NirvanaDataStore _dataStore;
        private readonly IntervalTree<Transcript> _transcriptIntervalTree;

        private const int DownstreamLength = 5000;
        private const int UpstreamLength   = 5000;

        private string _transcriptTarget;
        private string _vcfLine;
        private string _referenceName;
        private int _referencePosition;
        private string _refAllele;
        private List<string> _altAlleles;

        private bool _useTranscriptTarget;
        private bool _useVariantTarget;
        private bool _useVcfLine;

        #endregion

        // constructor
        public TranscriptExtractor(string inputCachePath)
        {
            _dataStore              = new NirvanaDataStore();
            _transcriptIntervalTree = new IntervalTree<Transcript>();

            LoadData(inputCachePath);
        }

        /// <summary>
        /// sets the vcf line. If enabled, the vcf line will be used to grab the overlapping transcripts.
        /// </summary>
        public void SetVcfLine(string vcfLine)
        {
            _vcfLine    = vcfLine.Replace("\\t", "\t");
            _useVcfLine = true;
        }

        /// <summary>
        /// sets the transcript target. If enabled, only the specified transcript will be saved
        /// </summary>
        public void SetTranscriptTarget(string transcriptId)
        {
            _transcriptTarget    = transcriptId;
            _useTranscriptTarget = true;
        }

        /// <summary>
        /// sets the vcf line. If enabled, the vcf line will be used to grab the overlapping transcripts.
        /// </summary>
        public void SetVariantTarget(string referenceName, int referencePosition, string refAllele, List<string> altAlleles)
        {
            _referenceName     = referenceName;
            _referencePosition = referencePosition;
            _refAllele         = refAllele;
            _altAlleles        = altAlleles;
            _useVariantTarget  = true;
        }

        /// <summary>
        /// loads all of the transcripts contained in the specified cache file
        /// </summary>
        private void LoadData(string inputCachePath)
        {
            using (var reader = new NirvanaDatabaseReader(inputCachePath))
            {
                reader.PopulateData(_dataStore, _transcriptIntervalTree);
            }
        }

        /// <summary>
        /// locates the transcript from our datastore and adds it to the list
        /// </summary>
        private void GetTranscript(string transcriptId, List<Transcript> overlappingTranscripts)
        {
            bool foundTranscript = false;

            foreach (var transcript in _dataStore.Transcripts.Where(transcript => transcript.StableId == transcriptId))
            {
                overlappingTranscripts.Add(transcript);
                foundTranscript = true;
                break;
            }

            if (!foundTranscript)
            {
                throw new UserErrorException("Could not find " + transcriptId + " in the datastore.");
            }

            // display all the overlapping transcripts
            Console.WriteLine("Found the transcript:");
            foreach (var transcript in overlappingTranscripts) Console.WriteLine("- {0}", transcript.StableId);
        }

        /// <summary>
        /// constructs the vcf line and returns all the overlapping transcripts
        /// </summary>
        private void GetOverlappingTranscripts(string referenceName, int referencePosition, string refAllele, List<string> altAlleles, List<Transcript> overlappingTranscripts)
        {
            string vcfLine =
                $"{referenceName}\t{referencePosition}\t.\t{refAllele}\t{string.Join(",", altAlleles)}\t20\tPASS\t.";

            GetOverlappingTranscripts(vcfLine, overlappingTranscripts);
        }

        /// <summary>
        /// parses the vcf line and returns all the overlapping transcripts
        /// </summary>
        private void GetOverlappingTranscripts(string vcfLine, List<Transcript> overlappingTranscripts)
        {
            // create a new variant containing the specified variant
            var variant = new VariantFeature();
            variant.ParseVcfLine(vcfLine);

            // display the variant
            Console.WriteLine(variant);

            var transcriptInterval = new IntervalTree<Transcript>.Interval(variant.ReferenceName, variant.VcfReferenceBegin - UpstreamLength, variant.VcfReferenceEnd + DownstreamLength);
            _transcriptIntervalTree.GetAllOverlappingValues(transcriptInterval, overlappingTranscripts);

            // display all the overlapping transcripts
            Console.WriteLine("Found {0} overlapping transcripts:", overlappingTranscripts.Count);
            foreach (var transcript in overlappingTranscripts) Console.WriteLine("- {0}", transcript.StableId);
        }

        /// <summary>
        /// extracts all of the variants that overlap with this variant
        /// </summary>
        public void Extract(string outputCachePath)
        {
            var overlappingDataStore = new NirvanaDataStore {
                Genes              = new List<Gene>(),
                Transcripts        = new List<Transcript>(),
                RegulatoryFeatures = new List<RegulatoryFeature>()
            };

            if (_useTranscriptTarget)   GetTranscript(_transcriptTarget, overlappingDataStore.Transcripts);
            else if (_useVcfLine)       GetOverlappingTranscripts(_vcfLine, overlappingDataStore.Transcripts);
            else if (_useVariantTarget) GetOverlappingTranscripts(_referenceName, _referencePosition, _refAllele, _altAlleles, overlappingDataStore.Transcripts);

            // write the overlapping transcripts to an output file
            DataStoreUtilities.PopulateTranscriptObjects(overlappingDataStore);

            // populate the genes list
            overlappingDataStore.Genes = DataStoreUtilities.GetGenesSubset(_dataStore.Genes, overlappingDataStore.Transcripts);

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
