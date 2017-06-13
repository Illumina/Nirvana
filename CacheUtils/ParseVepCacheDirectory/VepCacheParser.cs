using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CacheUtils.DataDumperImport.DataStructures;
using CacheUtils.DataDumperImport.DataStructures.VEP;
using CacheUtils.DataDumperImport.FileHandling;
using CacheUtils.DataDumperImport.Utilities;
using ErrorHandling.Exceptions;
using VariantAnnotation.DataStructures.Transcript;
using Exon              = CacheUtils.DataDumperImport.DataStructures.VEP.Exon;
using Gene              = VariantAnnotation.DataStructures.Gene;
using Intron            = CacheUtils.DataDumperImport.DataStructures.VEP.Intron;
using RegulatoryFeature = CacheUtils.DataDumperImport.Import.RegulatoryFeature;
using SimpleInterval    = VariantAnnotation.DataStructures.Intervals.SimpleInterval;
using Translation       = CacheUtils.DataDumperImport.DataStructures.VEP.Translation;

namespace CacheUtils.ParseVepCacheDirectory
{
    public sealed class VepCacheParser
    {
        #region members

        private readonly ImportDataStore _tempDataStore;
        private readonly ImportDataStore _nonUniquedataStore;
        private readonly ImportDataStore _uniqueDataStore;

        private const string MotifFeatureKey = "MotifFeature";
        private const string RegulatoryFeatureKey = "RegulatoryFeature";

        private readonly FeatureStatistics _transcriptStatistics;
        private readonly FeatureStatistics _regulatoryStatistics;
        private readonly FeatureStatistics _geneStatistics;
        private readonly FeatureStatistics _intronStatistics;
        private readonly FeatureStatistics _exonStatistics;
        private readonly FeatureStatistics _mirnaStatistics;
        private readonly FeatureStatistics _siftStatistics;
        private readonly FeatureStatistics _polyphenStatistics;
        private readonly FeatureStatistics _cdnaStatistics;
        private readonly FeatureStatistics _peptideStatistics;

        private int _currentGeneIndexOffset;
        private int _currentExonIndexOffset;
        private int _currentIntronIndexOffset;
        private int _currentMicroRnaIndexOffset;
        private int _currentCdnaIndexOffset;
        private int _currentPeptideIndexOffset;

        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        public VepCacheParser(TranscriptDataSource ds)
        {
            ImportDataStore.TranscriptSource = ds;

            _uniqueDataStore    = new ImportDataStore();
            _nonUniquedataStore = new ImportDataStore();
            _tempDataStore      = new ImportDataStore();

            _regulatoryStatistics = new FeatureStatistics("Regulatory");
            _transcriptStatistics = new FeatureStatistics("Transcripts");
            _geneStatistics       = new FeatureStatistics("Genes");
            _intronStatistics     = new FeatureStatistics("Introns");
            _exonStatistics       = new FeatureStatistics("Exons");
            _mirnaStatistics      = new FeatureStatistics("miRNAs");
            _siftStatistics       = new FeatureStatistics("SIFT matrices");
            _polyphenStatistics   = new FeatureStatistics("PolyPhen matrices");
            _cdnaStatistics       = new FeatureStatistics("cDNA seqs");
            _peptideStatistics    = new FeatureStatistics("Peptide seqs");
        }

        /// <summary>
        /// returns our transcript prefix whitelist given a data source
        /// </summary>
        private static string[] GetTranscriptPrefixWhiteList(TranscriptDataSource ds)
        {
            // Transcript IDs in the VEP builds:
            //
            // RefSeq  VEP72: #*, ENSEST*, CCDS*, NC_, NM_, NP_, NR_, XM_, XP_
            // Ensembl VEP72: ENSE0*, ENSG0*, ENSP0*, ENST0*
            //
            // RefSeq  VEP79: #*, CCDS*, ENSE0*, ENSG0*, ENSP0*, ENST*, [gene names], id*, LOC*, NC_*, NM_*, NP_*, NR_*, XM_*, XP_*, XR_*
            // Ensembl VEP79: ENSE0*, ENSG0*, ENSP0*, ENST0*
            //
            // RefSeq 2015-04-20: NG_*, NP_*, XP_*, YP_*, NR_*, NM_*, XR_*, XM_*
            string[] whiteList;

            switch (ds)
            {
                case TranscriptDataSource.Ensembl:
                    whiteList = new[] { "ENSE0", "ENSG0", "ENSP0", "ENST0" };
                    break;
                case TranscriptDataSource.RefSeq:
                    whiteList = new[] { "NG_", "NM_", "NP_", "NR_", "XM_", "XP_", "XR_", "YP_" };
                    break;
                default:
                    throw new GeneralException($"Unhandled import mode found: {ds}");
            }

            return whiteList;
        }

        #region NULL finding

        private static void FindNulls(DataDumperImport.DataStructures.VEP.Transcript transcript)
        {
            bool foundError = transcript.BioType == BioType.Unknown ||
                              transcript.TransExons == null ||
                              transcript.Gene == null ||
                              transcript.VariantEffectCache == null ||
                              transcript.Slice == null;

            if (foundError)
            {
                Console.WriteLine(transcript);
                throw new GeneralException("Found a null object in the transcript.");
            }

            // search deeper
            if (transcript.Translation != null) FindNulls(transcript.Translation);
            FindNulls(transcript.TransExons);
            FindNulls(transcript.VariantEffectCache);
            FindNulls(transcript.Slice);
        }

        private static void FindNulls(Slice slice)
        {
            bool foundError = slice.CoordinateSystem == null;

            if (foundError)
            {
                Console.WriteLine(slice);
                throw new GeneralException("Found a null object in the slice.");
            }
        }

        private static void FindNulls(Translation translation)
        {
            bool foundError = translation.EndExon == null ||
                              translation.StartExon == null ||
                              translation.Transcript == null;

            if (foundError)
            {
                Console.WriteLine(translation);
                throw new GeneralException("Found a null object in the translation.");
            }
        }

        private static void FindNulls(Exon[] exons)
        {
            for (int i = 0; i < exons.Length; i++)
            {
                if (exons[i] == null)
                {
                    throw new GeneralException($"Found a null object in the exon {i}.");
                }
            }
        }

        private static void FindNulls(VariantEffectFeatureCache cache)
        {
            bool foundError = cache.Mapper == null;

            if (foundError)
            {
                Console.WriteLine(cache);
                throw new GeneralException("Found a null object in the cache.");
            }

            // search deeper
            if (cache.Introns != null) foreach (var intron in cache.Introns) FindNulls(intron);
            FindNulls(cache.Mapper);
        }

        private static void FindNulls(TranscriptMapper mapper)
        {
            bool foundError = mapper.ExonCoordinateMapper == null;

            if (foundError)
            {
                Console.WriteLine(mapper);
                throw new GeneralException("Found a null object in the transcript mapper.");
            }
        }

        private static void FindNulls(Intron intron)
        {
            bool foundError = intron.Slice == null;

            if (foundError)
            {
                Console.WriteLine(intron);
                throw new GeneralException("Found a null object in the intron.");
            }

            // search deeper
            FindNulls(intron.Slice);
        }

        #endregion

        /// <summary>
        /// parses the data from the current reader and then uses the specified parser.
        /// </summary>
        private void ParseTranscriptDumpFilePass(DataDumperReader reader, Action<ObjectValue, int, ImportDataStore> parser)
        {
            var childNode = reader.RootNode.GetChild();

            var referenceSequenceNode = childNode as ObjectValue;

            if (referenceSequenceNode != null)
            {
                // loop over each reference sequence
                foreach (AbstractData ad in referenceSequenceNode)
                {
                    var transcriptNodes = ad as ListObjectKeyValue;

                    if (transcriptNodes != null)
                    {
                        // loop over each transcript
                        int transcriptIndex = 0;
                        foreach (AbstractData abTranscriptNode in transcriptNodes)
                        {
                            var transcriptNode = abTranscriptNode as ObjectValue;

                            // sanity check: make sure this node is an object value
                            if (transcriptNode == null)
                            {
                                Console.WriteLine("Expected a transcript object value node, but the current node is not an object value.");
                                Environment.Exit(1);
                            }

                            // sanity check: make sure this is a transcript data type
                            if (transcriptNode != null && transcriptNode.DataType != DataDumperImport.Import.Transcript.DataType)
                            {
                                Console.WriteLine("Expected a transcript node, but the current data type is: [{0}]", transcriptNode.DataType);
                                Environment.Exit(1);
                            }

                            parser(transcriptNode, transcriptIndex, _tempDataStore);
                            transcriptIndex++;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// parses the data from the current reader and then uses the specified parser.
        /// </summary>
        private void ParseRegulatoryDumpFilePass(DataDumperReader reader, Action<ObjectValue, int, ImportDataStore> parser)
        {
            var childNode = reader.RootNode.GetChild();
            var referenceSequenceNode = childNode as ObjectValue;

            if (referenceSequenceNode != null)
            {
                foreach (AbstractData ad in referenceSequenceNode)
                {
                    var objectKeyValue = ad as ObjectKeyValue;
                    if (objectKeyValue == null) throw new GeneralException("Unable to cast AbstractData as ObjectKeyValue");

                    foreach (AbstractData featureGroup in objectKeyValue.Value)
                    {
                        switch (featureGroup.Key)
                        {
                            case MotifFeatureKey:
                                // skip
                                break;
                            case RegulatoryFeatureKey:
                                ParseRegulatoryFeatures(featureGroup, parser);
                                break;
                            default:
                                throw new GeneralException("Found an unexpected feature group (" + featureGroup.Key + ") in the regulatory regions file.");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// parses a list of regulatory elements and adds them to the import data store
        /// </summary>
        private void ParseRegulatoryFeatures(AbstractData featureGroup, Action<ObjectValue, int, ImportDataStore> parser)
        {
            var regulatoryFeatureNodes = featureGroup as ListObjectKeyValue;

            if (regulatoryFeatureNodes != null)
            {
                // loop over each regulatory element
                int regulatoryFeatureIndex = 0;
                foreach (AbstractData abRegulatoryFeatureNode in regulatoryFeatureNodes)
                {
                    var regulatoryFeatureNode = abRegulatoryFeatureNode as ObjectValue;

                    // sanity check: make sure this node is an object value
                    if (regulatoryFeatureNode == null)
                    {
                        Console.WriteLine("Expected a regulatory element object value node, but the current node is not an object value.");
                        Environment.Exit(1);
                    }

                    // sanity check: make sure this is a regulatory element data type
                    if (regulatoryFeatureNode != null && regulatoryFeatureNode.DataType != RegulatoryFeature.DataType)
                    {
                        Console.WriteLine("Expected a regulatory element node, but the current data type is: [{0}]", regulatoryFeatureNode.DataType);
                        Environment.Exit(1);
                    }

                    parser(regulatoryFeatureNode, regulatoryFeatureIndex, _tempDataStore);
                    regulatoryFeatureIndex++;
                }
            }
        }

        /// <summary>
        /// parses the Perl Dumper file and imports the data into our .NET
        /// native data structures
        /// </summary>
        private void ParseTranscriptDumpFile(string dumpPath)
        {
            Console.WriteLine("- processing {0}", Path.GetFileName(dumpPath));

            // sanity check
            if (!File.Exists(dumpPath))
            {
                throw new FileNotFoundException($"The specified Perl dumper file ({dumpPath}) does not exist.");
            }

            using (var reader = new DataDumperReader(dumpPath))
            {
                // first pass: initial parsing
                ParseTranscriptDumpFilePass(reader, DataDumperImport.Import.Transcript.Parse);

                // second pass: setting references
                ParseTranscriptDumpFilePass(reader, DataDumperImport.Import.Transcript.ParseReferences);

                // sanity check: look for null elements in the transcripts
                foreach (var transcript in _tempDataStore.Transcripts) FindNulls(transcript);
            }
        }

        /// <summary>
        /// parses the Perl Dumper file and imports the data into our .NET
        /// native data structures
        /// </summary>
        private void ParseRegulatoryDumpFile(string dumpPath)
        {
            Console.WriteLine("- processing {0}", Path.GetFileName(dumpPath));

            // sanity check
            if (!File.Exists(dumpPath))
            {
                throw new FileNotFoundException($"The specified Perl dumper file ({dumpPath}) does not exist.");
            }

            using (var reader = new DataDumperReader(dumpPath))
            {
                ParseRegulatoryDumpFilePass(reader, RegulatoryFeature.Parse);
            }
        }

        public void ParseDumpDirectory(ushort refIndex, string refSeqDirectory, StreamWriter transcriptWriter,
            StreamWriter regulatoryWriter, StreamWriter geneWriter, StreamWriter intronWriter, StreamWriter exonWriter,
            StreamWriter mirnaWriter, BinaryWriter siftWriter, BinaryWriter polyphenWriter, StreamWriter cdnaWriter,
            StreamWriter peptideWriter)
        {
            var transcriptFiles = Directory.GetFiles(refSeqDirectory, "*_transcripts_data_dumper.txt.gz");
            var regulatoryFiles = Directory.GetFiles(refSeqDirectory, "*_reg_regulatory_regions_data_dumper.txt.gz");

            _tempDataStore.CurrentReferenceIndex      = refIndex;
            _uniqueDataStore.CurrentReferenceIndex    = refIndex;
            _nonUniquedataStore.CurrentReferenceIndex = refIndex;

            ParseRegulatoryFiles(regulatoryFiles);
            ParseTranscriptFiles(transcriptFiles);

            // merge the transcripts and regulatory regions
            var whiteList = GetTranscriptPrefixWhiteList(ImportDataStore.TranscriptSource);

            var transcriptMerger = new TranscriptMerger(whiteList);
            transcriptMerger.Merge(_nonUniquedataStore, _uniqueDataStore, _transcriptStatistics);

            var regulatoryRegionMerger = new RegulatoryRegionMerger();
            regulatoryRegionMerger.Merge(_nonUniquedataStore, _uniqueDataStore, _regulatoryStatistics);

            // calculate the gene indices
            var genes = ExtractGenes();
            var geneIndices = GetIndices(genes, _currentGeneIndexOffset);

            // calculate the intron indices
            var introns = ExtractIntrons();
            var intronIndices = GetIndices(introns, _currentIntronIndexOffset);

            // calculate the exon indices
            var exons = ExtractExons();
            var exonIndices = GetIndices(exons, _currentExonIndexOffset);

            // calculate the miRNA indices
            var mirnas = ExtractMicroRnas();
            var mirnaIndices = GetIndices(mirnas, _currentMicroRnaIndexOffset);

            // calculate the SIFT indices
            var sifts = ExtractSifts();
            var siftIndices = GetIndices(sifts, 0);

            // calculate the PolyPhen indices
            var polyphens = ExtractPolyPhens();
            var polyphenIndices = GetIndices(polyphens, 0);

            // calculate the cDNA indices
            var cdnaSeqs = ExtractCdnas();
            var cdnaIndices = GetIndices(cdnaSeqs, _currentCdnaIndexOffset);

            // calculate the peptide indices
            var peptideSeqs = ExtractPeptides();
            var peptideIndices = GetIndices(peptideSeqs, _currentPeptideIndexOffset);

            // convert the cDNA maps
            ConvertCdnaMaps();

            // dump the data from our temporary data stores to the writer
            AddIndicesToTranscripts(geneIndices, intronIndices, exonIndices, mirnaIndices, siftIndices, polyphenIndices,
                cdnaIndices, peptideIndices);

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Serialization:");
            Console.ResetColor();
            Console.WriteLine(new string('=', 60));

            SerializeTranscriptData(transcriptWriter);
            SerializeRegulatoryData(regulatoryWriter);

            // dump the genes to the writer
            SerializeData("genes", geneWriter, genes);
            _currentGeneIndexOffset += genes.Count;

            // dump the introns to the writer
            SerializeData("introns", intronWriter, introns);
            _currentIntronIndexOffset += introns.Count;

            // dump the exons to the writer
            SerializeData("exons", exonWriter, exons);
            _currentExonIndexOffset += exons.Count;

            // dump the miRNAs to the writer
            SerializeData("miRNAs", mirnaWriter, mirnas);
            _currentMicroRnaIndexOffset += mirnas.Count;

            // dump the Sifts to the writer
            SerializeProteinFunctionPrediction("Sifts", siftWriter, sifts, _uniqueDataStore.CurrentReferenceIndex);

            // dump the PolyPhens to the writer
            SerializeProteinFunctionPrediction("PolyPhens", polyphenWriter, polyphens, _uniqueDataStore.CurrentReferenceIndex);

            // dump the cDNA seqs to the writer
            SerializeData("cDNA seqs", cdnaWriter, cdnaSeqs);
            _currentCdnaIndexOffset += cdnaSeqs.Count;

            // dump the peptide seqs to the writer
            SerializeData("peptide seqs", peptideWriter, peptideSeqs);
            _currentPeptideIndexOffset += peptideSeqs.Count;

            // delete all of the master data
            _tempDataStore.Clear();
            _nonUniquedataStore.Clear();
            _uniqueDataStore.Clear();
        }

        private void ParseTranscriptFiles(string[] transcriptFiles)
        {
            // parse the transcript files
            foreach (string dumpPath in GetSortedFiles(transcriptFiles))
            {
                ParseTranscriptDumpFile(dumpPath);

                // copy the data (this is to maintain the references)
                // all transcript filtering is performed in here as well
                _nonUniquedataStore.CopyDataFrom(_tempDataStore);

                Console.WriteLine("- data store: {0}", _nonUniquedataStore);

                _tempDataStore.Clear();
            }
        }

        private static string[] GetSortedFiles(string[] filePaths)
        {
            var sortedPaths = new SortedDictionary<int, string>();

            foreach (string filePath in filePaths)
            {
                var fileName = Path.GetFileName(filePath);
                if (fileName == null) continue;

                int hyphenPos = fileName.IndexOf("-", StringComparison.Ordinal);
                if(hyphenPos == -1) throw new GeneralException($"Could not find the hyphen in: [{fileName}]");

                int position = int.Parse(fileName.Substring(0, hyphenPos));
                sortedPaths[position] = filePath;
            }

            return sortedPaths.Values.ToArray();
        }

        private void ParseRegulatoryFiles(string[] regulatoryFiles)
        {
            // parse the regulatory files
            foreach (string dumpPath in GetSortedFiles(regulatoryFiles))
            {
                ParseRegulatoryDumpFile(dumpPath);

                // copy the data (this is to maintain the references)
                // all transcript filtering is performed in here as well
                _nonUniquedataStore.CopyDataFrom(_tempDataStore);

                Console.WriteLine("- data store: {0}", _nonUniquedataStore);

                _tempDataStore.Clear();
            }
        }

        private void AddIndicesToTranscripts(Dictionary<Gene, int> geneIndices,
            Dictionary<SimpleInterval, int> intronIndices, Dictionary<LegacyExon, int> exonIndices,
            Dictionary<SimpleInterval, int> mirnaIndices, Dictionary<string, int> siftIndices,
            Dictionary<string, int> polyphenIndices, Dictionary<string, int> cdnaIndices,
            Dictionary<string, int> peptideIndices)
        {
            foreach (var transcript in _uniqueDataStore.Transcripts)
            {
                transcript.GeneIndex = GetIndex("gene", transcript.StableId, transcript.FinalGene, geneIndices);

                transcript.ExonIndices = new int[transcript.FinalExons.Length];
                for (int i = 0; i < transcript.FinalExons.Length; i++)
                {
                    transcript.ExonIndices[i] = GetIndex("exon", transcript.StableId, transcript.FinalExons[i],
                        exonIndices);
                }

                if (transcript.FinalIntrons != null)
                {
                    transcript.IntronIndices = new int[transcript.FinalIntrons.Length];
                    for (int i = 0; i < transcript.FinalIntrons.Length; i++)
                    {
                        transcript.IntronIndices[i] = GetIndex("intron", transcript.StableId, transcript.FinalIntrons[i],
                            intronIndices);
                    }
                }

                if (transcript.FinalMicroRnas != null)
                {
                    transcript.MicroRnaIndices = new int[transcript.FinalMicroRnas.Length];
                    for (int i = 0; i < transcript.FinalMicroRnas.Length; i++)
                    {
                        transcript.MicroRnaIndices[i] = GetIndex("miRNA", transcript.StableId, transcript.FinalMicroRnas[i],
                            mirnaIndices);
                    }
                }

                if (transcript.VariantEffectCache?.ProteinFunctionPredictions?.Sift?.Matrix != null)
                {
                    transcript.SiftIndex = GetIndex("Sift", transcript.StableId, transcript.VariantEffectCache.ProteinFunctionPredictions.Sift.Matrix, siftIndices);
                }

                if (transcript.VariantEffectCache?.ProteinFunctionPredictions?.PolyPhen?.Matrix != null)
                {
                    transcript.PolyPhenIndex = GetIndex("PolyPhen", transcript.StableId, transcript.VariantEffectCache.ProteinFunctionPredictions.PolyPhen.Matrix, polyphenIndices);
                }

                if (transcript.VariantEffectCache?.TranslateableSeq != null)
                {
                    transcript.CdnaSeqIndex = GetIndex("cDNA", transcript.StableId, transcript.VariantEffectCache.TranslateableSeq, cdnaIndices);
                }

                if (transcript.VariantEffectCache?.Peptide != null)
                {
                    transcript.PeptideSeqIndex = GetIndex("Peptide", transcript.StableId, transcript.VariantEffectCache.Peptide, peptideIndices);
                }
            }
        }

        private static int GetIndex<T>(string description, string id, T key, Dictionary<T, int> indices)
        {
            int index;
            if (!indices.TryGetValue(key, out index))
            {
                throw new GeneralException($"Unable to find the {description} in {id}");
            }

            return index;
        }

        /// <summary>
        /// returns a unique set of genes from the transcripts
        /// </summary>
        private List<Gene> ExtractGenes()
        {
            var genes = new HashSet<Gene>();
            int numAdded = 0;

            foreach (var transcript in _uniqueDataStore.Transcripts)
            {
                genes.Add(transcript.FinalGene);
                numAdded++;
            }

            _geneStatistics.Increment(genes.Count, numAdded);

            return genes.OrderBy(x => x.ReferenceIndex).ThenBy(x => x.Start).ThenBy(x => x.End).ToList();
        }

        private List<LegacyExon> ExtractExons()
        {
            var exonSet = new HashSet<LegacyExon>();
            int numAdded = 0;

            foreach (var transcript in _uniqueDataStore.Transcripts)
            {
                var exons = new List<LegacyExon>();

                foreach (var exon in transcript.TransExons)
                {
                    var newExon = exon.Convert();
                    exons.Add(newExon);
                    exonSet.Add(newExon);
                    numAdded++;
                }

                transcript.FinalExons = exons.OrderBy(x => x.Start).ThenBy(x => x.End).ToArray();
            }

            _exonStatistics.Increment(exonSet.Count, numAdded);

            return exonSet.OrderBy(x => x.Start).ThenBy(x => x.End).ToList();
        }

        private List<SimpleInterval> ExtractIntrons()
        {
            var intronSet = new HashSet<SimpleInterval>();
            int numAdded = 0;

            foreach (var transcript in _uniqueDataStore.Transcripts)
            {
                var introns = new List<SimpleInterval>();

                var transcriptIntrons = transcript.VariantEffectCache?.Introns;

                if (transcriptIntrons != null)
                {
                    foreach (var intron in transcriptIntrons)
                    {
                        var newIntron = intron.Convert();
                        introns.Add(newIntron);
                        intronSet.Add(newIntron);
                        numAdded++;
                    }

                    transcript.FinalIntrons = introns.OrderBy(x => x.Start).ThenBy(x => x.End).ToArray();
                }
                else
                {
                    transcript.FinalIntrons = null;
                }
            }

            _intronStatistics.Increment(intronSet.Count, numAdded);

            return intronSet.OrderBy(x => x.Start).ThenBy(x => x.End).ToList();
        }

        private List<SimpleInterval> ExtractMicroRnas()
        {
            var microRnaSet = new HashSet<SimpleInterval>();
            int numAdded = 0;

            foreach (var transcript in _uniqueDataStore.Transcripts)
            {
                var microRnas = new List<SimpleInterval>();

                if (transcript.MicroRnas != null)
                {
                    foreach (var mirna in transcript.MicroRnas)
                    {
                        microRnas.Add(mirna);
                        microRnaSet.Add(mirna);
                        numAdded++;
                    }

                    transcript.FinalMicroRnas = microRnas.OrderBy(x => x.Start).ThenBy(x => x.End).ToArray();
                }
                else
                {
                    transcript.FinalMicroRnas = null;
                }
            }

            _mirnaStatistics.Increment(microRnaSet.Count, numAdded);

            return microRnaSet.OrderBy(x => x.Start).ThenBy(x => x.End).ToList();
        }

        private List<string> ExtractSifts()
        {
            var stringSet = new HashSet<string>();
            int numAdded = 0;

            foreach (var transcript in _uniqueDataStore.Transcripts)
            {
                var matrix = transcript.VariantEffectCache?.ProteinFunctionPredictions?.Sift?.Matrix;

                if (matrix != null)
                {
                    stringSet.Add(matrix);
                    numAdded++;
                }
            }

            _siftStatistics.Increment(stringSet.Count, numAdded);

            return stringSet.OrderBy(x => x.Length).ToList();
        }

        private List<string> ExtractPolyPhens()
        {
            var stringSet = new HashSet<string>();
            int numAdded = 0;

            foreach (var transcript in _uniqueDataStore.Transcripts)
            {
                var matrix = transcript.VariantEffectCache?.ProteinFunctionPredictions?.PolyPhen?.Matrix;

                if (matrix != null)
                {
                    stringSet.Add(matrix);
                    numAdded++;
                }
            }

            _polyphenStatistics.Increment(stringSet.Count, numAdded);

            return stringSet.OrderBy(x => x.Length).ToList();
        }

        private List<string> ExtractCdnas()
        {
            var cdnas = new HashSet<string>();
            int numAdded = 0;

            foreach (var transcript in _uniqueDataStore.Transcripts)
            {
                var cdnaSeq = transcript.VariantEffectCache?.TranslateableSeq;

                if (cdnaSeq != null)
                {
                    cdnas.Add(cdnaSeq);
                    numAdded++;
                }
            }

            _cdnaStatistics.Increment(cdnas.Count, numAdded);

            return cdnas.OrderBy(x => x).ToList();
        }

        private List<string> ExtractPeptides()
        {
            var peptides = new HashSet<string>();
            int numAdded = 0;

            foreach (var transcript in _uniqueDataStore.Transcripts)
            {
                var peptideSeq = transcript.VariantEffectCache?.Peptide;

                if (peptideSeq != null)
                {
                    peptides.Add(peptideSeq);
                    numAdded++;
                }
            }

            _peptideStatistics.Increment(peptides.Count, numAdded);

            return peptides.OrderBy(x => x).ToList();
        }

        private void ConvertCdnaMaps()
        {
            foreach (var transcript in _uniqueDataStore.Transcripts)
            {
                var cdnaMaps = new List<CdnaCoordinateMap>();

                foreach (var mapperPairs in transcript.VariantEffectCache.Mapper.ExonCoordinateMapper.PairGenomic.Genomic)
                {
                    var newCdnaMap = PairGenomic.ConvertMapperPair(mapperPairs);
                    cdnaMaps.Add(newCdnaMap);
                }

                transcript.FinalCdnaMaps = cdnaMaps.OrderBy(x => x.GenomicStart).ThenBy(x => x.GenomicEnd).ToArray();
            }
        }

        private static Dictionary<T, int> GetIndices<T>(List<T> l, int offset)
        {
            var indices = new Dictionary<T, int>();
            for (int i = 0; i < l.Count; i++) indices[l[i]] = i + offset;
            return indices;
        }

        /// <summary>
        /// serializes all the VEP transcript data to the writer
        /// </summary>
        private void SerializeTranscriptData(StreamWriter writer)
        {
            Console.Write("- serializing transcript features... ");

            foreach (var transcript in _uniqueDataStore.Transcripts.OrderBy(x => x.ReferenceIndex).ThenBy(x => x.Start).ThenBy(x => x.End))
            {
                writer.Write(transcript);
            }

            writer.Flush();
            Console.WriteLine("{0} features written.", _uniqueDataStore.Transcripts.Count);
        }

        /// <summary>
        /// serializes all the VEP regulatory data to the writer
        /// </summary>
        private void SerializeRegulatoryData(StreamWriter writer)
        {
            Console.Write("- serializing regulatory elements... ");

            foreach (var regulatoryFeature in _uniqueDataStore.RegulatoryFeatures.OrderBy(x => x.ReferenceIndex).ThenBy(x => x.Start).ThenBy(x => x.End))
            {
                writer.WriteLine(regulatoryFeature);
            }

            writer.Flush();
            Console.WriteLine("{0} features written.", _uniqueDataStore.RegulatoryFeatures.Count);
        }

        /// <summary>
        /// serializes all the list values to the writer
        /// </summary>
        private static void SerializeData<T>(string description, StreamWriter writer, List<T> values)
        {
            Console.Write("- serializing {0}... ", description);

            int numValues = values.Count;
            for (int i = 0; i < numValues; i++)
            {
                writer.WriteLine($"{i}\t{values[i]}");
            }

            writer.Flush();
            Console.WriteLine("{0} features written.", values.Count);
        }

        /// <summary>
        /// serializes all the VEP gene data to the writer
        /// </summary>
        private static void SerializeProteinFunctionPrediction(string description, BinaryWriter writer, List<string> matrices, ushort refIndex)
        {
            Console.Write("- serializing {0}... ", description);
            foreach (var matrix in matrices) ProteinFunctionPredictions.Serialize(writer, matrix, refIndex);
            writer.Flush();
            Console.WriteLine("{0} matrices written.", matrices.Count);
        }

        public void DumpStatistics()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("De-duplication statistics:");
            Console.ResetColor();
            Console.WriteLine(new string('=', 60));

            Console.WriteLine(_regulatoryStatistics);
            Console.WriteLine(_transcriptStatistics);
            Console.WriteLine(_geneStatistics);
            Console.WriteLine(_intronStatistics);
            Console.WriteLine(_exonStatistics);
            Console.WriteLine(_mirnaStatistics);
            Console.WriteLine(_siftStatistics);
            Console.WriteLine(_polyphenStatistics);
            Console.WriteLine(_cdnaStatistics);
            Console.WriteLine(_peptideStatistics);
        }
    }
}
