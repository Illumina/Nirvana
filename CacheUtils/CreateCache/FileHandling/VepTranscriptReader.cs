using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CacheUtils.CombineAndUpdateGenes.DataStructures;
using CacheUtils.DataDumperImport.FileHandling;
using ErrorHandling.Exceptions;
using VariantAnnotation.DataStructures;
using VariantAnnotation.DataStructures.Intervals;
using VariantAnnotation.DataStructures.Transcript;
using VariantAnnotation.FileHandling.Compression;
using VariantAnnotation.Interface;
using VariantAnnotation.Utilities;

namespace CacheUtils.CreateCache.FileHandling
{
    public sealed class VepTranscriptReader : IDisposable
    {
        #region members

        private readonly StreamReader _reader;
        private readonly string _filePath;
        public readonly GlobalImportHeader Header;

        private bool _hasLists;
        private List<SimpleInterval> _introns;
        private List<SimpleInterval> _microRnas;
        private List<string> _peptideSeqs;

        private IIntervalForest<MutableGene> _mergedGeneForest;

        private readonly List<MutableGene> _overlappingGenes = new List<MutableGene>();

        #endregion

        #region IDisposable

        private bool _isDisposed;

        /// <summary>
        /// public implementation of Dispose pattern callable by consumers. 
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// protected implementation of Dispose pattern. 
        /// </summary>
        private void Dispose(bool disposing)
        {
            lock (this)
            {
                if (_isDisposed) return;

                if (disposing)
                {
                    // Free any other managed objects here. 
                    _reader.Dispose();
                }

                // Free any unmanaged objects here. 
                _isDisposed = true;
            }
        }

        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        public VepTranscriptReader(string filePath)
        {
            // sanity check
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"The specified transcripts file ({filePath}) does not exist.");
            }

            // open the vcf file and parse the header
            _filePath = filePath;
            _reader   = GZipUtilities.GetAppropriateStreamReader(filePath);
            Header    = GetHeader();
        }

        /// <summary>
        /// returns the file header
        /// </summary>
        private GlobalImportHeader GetHeader()
        {
            string line = _reader.ReadLine();

            if (!IsValidFile(line))
            {
                throw new GeneralException($"The transcript file ({_filePath}) has an invalid header.");
            }

            line = _reader.ReadLine();

            if (line == null)
            {
                throw new GeneralException($"The transcript file ({_filePath}) has an invalid header.");
            }
                
            var cols = line.Split('\t');
            if (cols.Length != GlobalImportCommon.NumHeaderColumns)
            {
                throw new GeneralException($"Expected {GlobalImportCommon.NumHeaderColumns} columns in the header but found {cols.Length}");
            }

            var vepVersion       = ushort.Parse(cols[0]);
            var vepReleaseTicks  = long.Parse(cols[1]);
            var transcriptSource = (TranscriptDataSource)byte.Parse(cols[2]);
            var genomeAssembly   = (GenomeAssembly)byte.Parse(cols[3]);

            return new GlobalImportHeader(vepVersion, vepReleaseTicks, transcriptSource, genomeAssembly);
        }

        /// <summary>
        /// returns true if this is a valid transcript file
        /// </summary>
        private static bool IsValidFile(string line)
        {
            string expectedString = $"{GlobalImportCommon.Header}\t{(byte)GlobalImportCommon.FileType.Transcript}";
            return line == expectedString;
        }

        /// <summary>
        /// adds the gene list to our reader
        /// </summary>
        public void AddLists(List<SimpleInterval> introns, List<SimpleInterval> microRnas,
            List<string> peptideSeqs, IIntervalForest<MutableGene> mergedGeneForest)
        {
            _introns          = introns;
            _microRnas        = microRnas;
            _peptideSeqs      = peptideSeqs;
            _mergedGeneForest = mergedGeneForest;
            _hasLists         = true;
        }

        /// <summary>
        /// retrieves the next variantFeature. Returns false if there are no more variants available
        /// </summary>
        public Transcript GetLightTranscript()
        {
            if (!_hasLists) throw new GeneralException("No lists have been supplied to the transcript reader.");

            // ================
            // read the ID line
            // ================

            string line = _reader.ReadLine();
            if (line == null) return null;
            
            var cols = line.Split('\t');
            if (cols.Length != 7) throw new GeneralException($"Expected 7 columns but found {cols.Length} when parsing the transcript entry: {line}");

            var lineType          = cols[0];
            var transcriptId      = CompactId.Convert(cols[1]);
            var transcriptVersion = GetMaxVersion(cols[2], cols[1]);
            var proteinId         = CompactId.Convert(cols[3]);
            var proteinVersion    = GetMaxVersion(cols[4], cols[3]);
            var geneId            = cols[5];
            var bioType           = (BioType)byte.Parse(cols[6]);

            if (lineType != "Transcript") throw new GeneralException($"Expected the Transcript lineType, but found: {line}");

            // ========================
            // read the transcript info
            // ========================

            line = _reader.ReadLine();
            if (line == null) return null;

            cols = line.Split('\t');
            if (cols.Length != 11) throw new GeneralException($"Expected 11 columns but found {cols.Length} when parsing the transcript info entry: {line}");

            var referenceIndex    = ushort.Parse(cols[0]);
            var start             = int.Parse(cols[1]);
            var end               = int.Parse(cols[2]);
            var codingRegionStart = int.Parse(cols[3]);
            var codingRegionEnd   = int.Parse(cols[4]);
            var cdnaCodingStart   = int.Parse(cols[5]);
            var cdnaCodingEnd     = int.Parse(cols[6]);
            var totalExonLength   = int.Parse(cols[7]);
            var isCanonical       = cols[8] == "Y";
            var startExonPhase    = GetExonPhase(cols[9]);

            var gene = GetGene(referenceIndex, start, end, transcriptId.ToString(), geneId).ToGene();

            // read the internal indices
            line = _reader.ReadLine();
            if (line == null) return null;

            cols = line.Split('\t');
            if (cols.Length != 4) throw new GeneralException($"Expected 4 columns but found {cols.Length} when parsing the transcript internal indices: {line}");

            // ReSharper disable once UnusedVariable
            var cdnaSeqIndex    = int.Parse(cols[0]);
            var peptideSeqIndex = int.Parse(cols[1]);
            var siftIndex       = int.Parse(cols[2]);
            var polyPhenIndex   = int.Parse(cols[3]);

            // =================================
            // read the exons, introns, & miRNAs
            // =================================

            SkipItems("Exons");
            var introns = GetItems("Introns", _introns);
            var peptide = peptideSeqIndex != -1 ? _peptideSeqs[peptideSeqIndex] : null;

            // ==================
            // read the cDNA maps
            // ==================

            line = _reader.ReadLine();
            if (line == null) return null;

            cols = line.Split('\t');

            lineType = cols[0];
            var numCdnaMaps = int.Parse(cols[1]);

            if (lineType != "cDNA maps") throw new GeneralException($"Expected the cDNA maps lineType, but found: {line}");

            CdnaCoordinateMap[] cdnaMaps = null;

            if (numCdnaMaps > 0)
            {
                cdnaMaps = new CdnaCoordinateMap[numCdnaMaps];

                for (int i = 0; i < numCdnaMaps; i++)
                {
                    line = _reader.ReadLine();

	                cols = line?.Split('\t') ?? throw new GeneralException("Found null line while parsing CDNA maps.");

                    if (cols.Length != 4) throw new GeneralException($"Expected 4 columns but found {cols.Length} when parsing the cDNA map entry: {line}");

                    var genomicStart = int.Parse(cols[0]);
                    var genomicEnd   = int.Parse(cols[1]);
                    var cdnaStart    = int.Parse(cols[2]);
                    var cdnaEnd      = int.Parse(cols[3]);

                    cdnaMaps[i] = new CdnaCoordinateMap(genomicStart, genomicEnd, cdnaStart, cdnaEnd);
                }
            }

            // ===============
            // read the miRNAs
            // ===============

            var microRnas = GetItems("miRNAs", _microRnas);

            // ===================
            // put it all together
            // ===================

            var codingRegion = new CdnaCoordinateMap(codingRegionStart, codingRegionEnd, cdnaCodingStart, cdnaCodingEnd);

            var translation = codingRegionStart != -1
                ? new Translation(codingRegion, proteinId, proteinVersion, peptide)
                : null;

            return new Transcript(referenceIndex, start, end, transcriptId, transcriptVersion, translation, bioType,
                gene, totalExonLength, startExonPhase, isCanonical, introns, microRnas, cdnaMaps, siftIndex,
                polyPhenIndex, Header.TranscriptSource);
        }

        /// <summary>
        /// Retrieves the maximum version. Handles situations where VEP sets the transcript
        /// version to 1, but embeds a version in the RefSeq ID: NM_178221.2
        /// </summary>
        private static byte GetMaxVersion(string transcriptVersion, string id)
        {
            var idVersion = FormatUtilities.SplitVersion(id).Item2;
            var version   = byte.Parse(transcriptVersion);
            return idVersion > version ? idVersion : version;
        }

        private MutableGene GetGene(ushort refIndex, int transcriptStart, int transcriptEnd, string transcriptId, string geneId)
        {
            _mergedGeneForest.GetAllOverlappingValues(refIndex, transcriptStart, transcriptEnd, _overlappingGenes);

            if (_overlappingGenes.Count == 0)
                throw new GeneralException(
                    $"Did not find any genes that overlap with this transcript: {transcriptId}");

            var genesWithCorrectGeneId = FilterOnGeneId(_overlappingGenes, geneId);
            if (genesWithCorrectGeneId.Count == 1) return genesWithCorrectGeneId[0];

            Console.WriteLine();
            throw new GeneralException("Unable to link the merged gene back to the original gene");
        }

        private static List<MutableGene> FilterOnGeneId(List<MutableGene> overlappingGenes, string geneId)
        {
            return overlappingGenes.Where(gene => gene.EntrezGeneId.ToString() == geneId || gene.EnsemblId.ToString() == geneId).ToList();
        }

        private void SkipItems(string expectedLineType)
        {
            string line = _reader.ReadLine();
            if (line == null) return;

            var cols = line.Split('\t');

            var lineType = cols[0];
            var numItems = int.Parse(cols[1]);

            if (lineType != expectedLineType)
            {
                throw new GeneralException($"Expected the {expectedLineType} lineType, but found: {line}");
            }

            if (numItems == 0) return;

            for (int i = 0; i < numItems; i++)
            {
                line = _reader.ReadLine();
                if (line == null) return;
            }
        }

        private T[] GetItems<T>(string expectedLineType, List<T> l)
        {
            string line = _reader.ReadLine();
            if (line == null) return null;

            var cols = line.Split('\t');

            var lineType = cols[0];
            var numItems = int.Parse(cols[1]);

            if (lineType != expectedLineType)
            {
                throw new GeneralException($"Expected the {expectedLineType} lineType, but found: {line}");
            }

            if (numItems == 0) return null;

            var items = new T[numItems];

            for (int i = 0; i < numItems; i++)
            {
                line = _reader.ReadLine();
                if (line == null) return null;
                var itemIndex = int.Parse(line);

                items[i] = l[itemIndex];
            }

            return items;
        }

        /// <summary>
        /// returns the exon phase given a string
        /// </summary>
        private static byte GetExonPhase(string s)
        {
            if (s == "") return 0;
            var i = int.Parse(s);
            return i == -1 ? (byte)0 : (byte)i;
        }
    }
}