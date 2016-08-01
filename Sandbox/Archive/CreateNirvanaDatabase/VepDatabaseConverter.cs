using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Illumina.DataDumperImport.FileHandling;
using Illumina.DataDumperImport.Utilities;
using Illumina.VariantAnnotation.DataStructures;
using Illumina.VariantAnnotation.FileHandling;
using DS = Illumina.DataDumperImport.DataStructures;
using Import = Illumina.DataDumperImport.Import;

namespace CreateNirvanaDatabase
{
    public class VepDatabaseConverter
    {
        #region members

        private readonly DS.ImportDataStore _tempDataStore;
        private readonly DS.ImportDataStore _dataStore;

        private readonly Dictionary<string, ushort> _referenceSequenceIndices;
        private readonly List<ReferenceMetadata> _referenceMetadataList;

        private static ushort _vepVersion;
        private readonly TranscriptDataSource _transcriptDataSource;

        private const string MotifFeatureKey = "MotifFeature";
        private const string RegulatoryFeatureKey = "RegulatoryFeature";

        private readonly long _vepReleaseTicks;

        private HashSet<string> _lrgEntries;

        #endregion

        // constructor
        public VepDatabaseConverter(ushort vepVersion, TranscriptDataSource transcriptDataSource, bool doNotFilterTranscripts, string inputCompressedReferencePath, string vepReleaseDate)
        {
            _vepVersion = vepVersion;
            _transcriptDataSource = transcriptDataSource;

            _tempDataStore = new DS.ImportDataStore();
            _dataStore = new DS.ImportDataStore();

            _vepReleaseTicks = DateTime.Parse(vepReleaseDate).Ticks;

            // load the reference
            var compressedSequenceReader = new CompressedSequenceReader(inputCompressedReferencePath);
            _referenceMetadataList = compressedSequenceReader.RefMetadataList;
            _referenceSequenceIndices = new Dictionary<string, ushort>();

            // ========================
            // configure our whitelists
            // ========================

            // Transcript IDs in the VEP builds:
            //
            // RefSeq  VEP72: #*, ENSEST*, CCDS*, NC_, NM_, NP_, NR_, XM_, XP_
            // Ensembl VEP72: ENSE0*, ENSG0*, ENSP0*, ENST0*
            //
            // RefSeq  VEP79: #*, CCDS*, ENSE0*, ENSG0*, ENSP0*, ENST*, [gene names], id*, LOC*, NC_*, NM_*, NP_*, NR_*, XM_*, XP_*, XR_*
            // Ensembl VEP79: ENSE0*, ENSG0*, ENSP0*, ENST0*
            //
            // RefSeq 2015-04-20: NG_*, NP_*, XP_*, YP_*, NR_*, NM_*, XR_*, XM_*
            //
            // N.B. Elliot Margulies suggested we keep the LOC entries in RefSeq VEP79

            // set the whitelists
            if (!doNotFilterTranscripts)
            {
                switch (transcriptDataSource)
                {
                    case TranscriptDataSource.Ensembl:
                        _dataStore.EnableWhiteList(new List<string> { "ENSE0", "ENSG0", "ENSP0", "ENST0" });
                        break;
                    case TranscriptDataSource.RefSeq:
                        _dataStore.EnableWhiteList(new List<string> { "LOC", "NC_", "NG_", "NP_", "XP_", "NR_", "NM_", "XR_", "XM_" });
                        break;
                    default:
                        throw new ApplicationException("Unhandled import mode found: " + transcriptDataSource);
                }
            }
        }

        #region Reference Index functions

        private ushort GetReferenceIndex(string referenceName)
        {
            ushort referenceIndex;
            if (!_referenceSequenceIndices.TryGetValue(referenceName, out referenceIndex))
            {
                throw new ApplicationException($"Unable to find the reference index for the specified reference: {referenceName}");
            }

            return referenceIndex;
        }

        private void AddReferenceSequence(string referenceName, ushort refIndex)
        {
            if (!string.IsNullOrEmpty(referenceName))
            {
                _referenceSequenceIndices[referenceName] = refIndex;
            }
        }

        public void AddReferenceSequences()
        {
            ushort numRefSeqs = (ushort)_referenceMetadataList.Count;

            for (ushort refIndex = 0; refIndex < numRefSeqs; refIndex++)
            {
                var refMetadata = _referenceMetadataList[refIndex];
                AddReferenceSequence(refMetadata.UcscName, refIndex);
                AddReferenceSequence(refMetadata.EnsemblName, refIndex);
            }
        }

        #endregion

        #region NULL finding

        private static void FindNulls(DS.VEP.Transcript transcript)
        {
            bool foundError = transcript.BioType == BioType.Unknown ||
                              transcript.TransExons == null ||
                              transcript.Gene == null ||
                              transcript.VariantEffectCache == null ||
                              transcript.Slice == null;

            if (foundError)
            {
                Console.WriteLine(transcript);
                throw new ApplicationException("Found a null object in the transcript.");
            }

            // search deeper
            if (transcript.Translation != null) FindNulls(transcript.Translation);
            FindNulls(transcript.TransExons);
            FindNulls(transcript.VariantEffectCache);
        }

        private static void FindNulls(DS.VEP.Translation translation)
        {
            bool foundError = translation.EndExon == null ||
                              translation.StartExon == null ||
                              translation.Transcript == null;

            if (foundError)
            {
                Console.WriteLine(translation);
                throw new ApplicationException("Found a null object in the translation.");
            }
        }

        private static void FindNulls(DS.VEP.Exon[] exons)
        {
            for (int i = 0; i < exons.Length; i++)
            {
                if (exons[i] == null)
                {
                    throw new ApplicationException($"Found a null object in the exon {i}.");
                }
            }
        }

        private static void FindNulls(DS.VEP.VariantEffectFeatureCache cache)
        {
            bool foundError = cache.Mapper == null;

            if (foundError)
            {
                Console.WriteLine(cache);
                throw new ApplicationException("Found a null object in the cache.");
            }

            // search deeper
            if (cache.Introns != null) foreach (var intron in cache.Introns) FindNulls(intron);
            FindNulls(cache.Mapper);
        }

        private static void FindNulls(DS.VEP.TranscriptMapper mapper)
        {
            bool foundError = mapper.ExonCoordinateMapper == null;

            if (foundError)
            {
                Console.WriteLine(mapper);
                throw new ApplicationException("Found a null object in the transcript mapper.");
            }
        }

        private static void FindNulls(DS.VEP.Intron intron)
        {
            bool foundError = intron.Slice == null;

            if (foundError)
            {
                Console.WriteLine(intron);
                throw new ApplicationException("Found a null object in the intron.");
            }
        }

        #endregion

        /// <summary>
        /// resets the current canonical transcript identifiers and recomputes which transcripts are canonical
        /// </summary>
        private void AssignCanonicalTranscripts()
        {
            // sanity check: we only want to re-assign canonical transcripts for RefSeq
            if (_transcriptDataSource != TranscriptDataSource.RefSeq) return;

            // sanity check: make sure we have the LRG files
            if (_lrgEntries == null)
            {
                throw new FileNotFoundException("The LRG data file is needed for recomputing canonical transcripts. The path was not provided at the command-line.");
            }

            Console.WriteLine("\nRecomputing canonical transcripts:");

            // assign each of the transcripts according to their Entrez gene ID
            var genes = AggregateGenes();
            var canonicalTranscripts = GetCanonicalTranscripts(genes);
            AddCanonicalFlags(canonicalTranscripts);
        }

        /// <summary>
        /// clears the canonical flags from all transcripts and adds the new canonical transcripts
        /// </summary>
        private void AddCanonicalFlags(SortedDictionary<int, string> canonicalTranscripts)
        {
            int numTranscriptsMarkedCanonical = 0;
            Console.Write("- adding canonical flags... ");

            foreach (var transcript in _dataStore.Transcripts)
            {
                transcript.IsCanonical = false;

                int geneId;
                if (!int.TryParse(transcript.GeneStableId, out geneId)) continue;

                string canonicalTranscriptId;
                if (canonicalTranscripts.TryGetValue(geneId, out canonicalTranscriptId))
                {
                    if (transcript.StableId == canonicalTranscriptId)
                    {
                        transcript.IsCanonical = true;
                        numTranscriptsMarkedCanonical++;
                    }
                }
            }

            Console.WriteLine("{0} transcripts were marked canonical.\n", numTranscriptsMarkedCanonical);
        }

        /// <summary>
        /// returns a mapping of gene ID to canonical transcript ID
        /// </summary>
        private static SortedDictionary<int, string> GetCanonicalTranscripts(SortedDictionary<int, HashSet<TempTranscript>> genes)
        {
            // - Order all of the overlapping transcripts by cds length
            // - Pick the longest transcript that has an associated Locus Reference Genome (LRG) sequence
            // - If no LRGs exist for the set of transcripts, pick the longest transcript that is coding
            // - If there is a tie, pick the transcript with the smaller accession id number
            var canonicalTranscripts = new SortedDictionary<int, string>();
            int numGenesWithCanonicalTranscripts = 0;

            foreach (var kvp in genes)
            {
                // ====================================
                // examine normal transcripts (NM & NR)
                // ====================================

                var normalTranscripts = GetNmNrTranscripts(kvp.Value);

                if (normalTranscripts.Count > 0)
                {
                    canonicalTranscripts[kvp.Key] = normalTranscripts[0].TranscriptId;
                    numGenesWithCanonicalTranscripts++;
                }
            }

            Console.WriteLine("- # genes with canonical transcripts: {0} ({1})", numGenesWithCanonicalTranscripts, canonicalTranscripts.Count);
            return canonicalTranscripts;
        }

        /// <summary>
        /// returns a sorted list of all the transcripts that have an NM_ or NR_ prefix
        /// </summary>
        private static List<TempTranscript> GetNmNrTranscripts(HashSet<TempTranscript> transcripts)
        {
            var selectedTranscripts =
                transcripts.Where(
                    transcript => transcript.TranscriptId.StartsWith("NM_") ||
                    transcript.TranscriptId.StartsWith("NR_")).ToList();

            return selectedTranscripts.OrderByDescending(x => x.IsLrg).ThenByDescending(x => x.CdsLength).ThenByDescending(x => x.TranscriptLength).ThenBy(x => x.AccessionNumber).ToList();
        }

        /// <summary>
        /// returns a dictionary that aggregates the transcripts by gene
        /// </summary>
        private SortedDictionary<int, HashSet<TempTranscript>> AggregateGenes()
        {
            var genes = new SortedDictionary<int, HashSet<TempTranscript>>();
            Console.Write("- aggregating transcripts by gene... ");

            foreach (var transcript in _dataStore.Transcripts)
            {
                int geneId;
                if (!int.TryParse(transcript.GeneStableId, out geneId))
                {
                    if (!transcript.GeneStableId.StartsWith("ENSESTG") && !transcript.GeneStableId.StartsWith("CCDS"))
                    {
                        Console.WriteLine("BAD gene ID: {0}", transcript.GeneStableId);
                    }

                    continue;
                }

                int cdsLength = (transcript.CompDnaCodingStart == -1) || (transcript.CompDnaCodingEnd == -1) ?
                    0 : transcript.CompDnaCodingEnd - transcript.CompDnaCodingStart + 1;

                int transcriptLength = transcript.End - transcript.Start + 1;
                var isLrg = _lrgEntries.Contains(Import.Transcript.RemoveVersion(transcript.StableId));

                var tempTranscript = new TempTranscript(transcript.StableId, transcriptLength, cdsLength, isLrg);

                HashSet<TempTranscript> prevTranscripts;
                if (genes.TryGetValue(geneId, out prevTranscripts))
                {
                    prevTranscripts.Add(tempTranscript);
                }
                else
                {
                    prevTranscripts = new HashSet<TempTranscript> { tempTranscript };
                    genes[geneId] = prevTranscripts;
                }
            }

            Console.WriteLine("{0} genes found.", genes.Count);

            return genes;
        }

        /// <summary>
        /// parses the data from the current reader and then uses the specified parser.
        /// </summary>
        private void ParseTranscriptDumpFilePass(DataDumperReader reader, Action<DS.ObjectValue, int, DS.ImportDataStore> parser)
        {
            var childNode = reader.RootNode.GetChild();

            var referenceSequenceNode = childNode as DS.ObjectValue;

            if (referenceSequenceNode != null)
            {
                // loop over each reference sequence
                foreach (DS.AbstractData ad in referenceSequenceNode)
                {
                    // Console.WriteLine("- processing reference sequence: [{0}]", ad.Key);

                    // look up the reference index
                    _tempDataStore.CurrentReferenceIndex = GetReferenceIndex(ad.Key);

                    var transcriptNodes = ad as DS.ListObjectKeyValue;

                    if (transcriptNodes != null)
                    {
                        // loop over each transcript
                        int transcriptIndex = 0;
                        foreach (DS.AbstractData abTranscriptNode in transcriptNodes)
                        {
                            // Console.WriteLine("transcript index: {0}", transcriptIndex);
                            var transcriptNode = abTranscriptNode as DS.ObjectValue;

                            // sanity check: make sure this node is an object value
                            if (transcriptNode == null)
                            {
                                Console.WriteLine("Expected a transcript object value node, but the current node is not an object value.");
                                Environment.Exit(1);
                            }

                            // sanity check: make sure this is a transcript data type
                            if (transcriptNode.DataType != Import.Transcript.DataType)
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
        private void ParseRegulatoryDumpFilePass(DataDumperReader reader, Action<DS.ObjectValue, int, DS.ImportDataStore> parser)
        {
            var childNode = reader.RootNode.GetChild();
            var referenceSequenceNode = childNode as DS.ObjectValue;

            if (referenceSequenceNode != null)
            {
                // loop over each reference sequence
                foreach (DS.AbstractData ad in referenceSequenceNode)
                {
                    // look up the reference index
                    _tempDataStore.CurrentReferenceIndex = GetReferenceIndex(ad.Key);

                    // loop over all of the values in the reference sequence
                    foreach (DS.AbstractData featureGroup in (ad as DS.ObjectKeyValue).Value)
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
                                throw new ApplicationException("Found an unexpected feature group (" + featureGroup.Key + ") in the regulatory regions file.");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// parses a list of regulatory features and adds them to the import data store
        /// </summary>
        private void ParseRegulatoryFeatures(DS.AbstractData featureGroup, Action<DS.ObjectValue, int, DS.ImportDataStore> parser)
        {
            var regulatoryFeatureNodes = featureGroup as DS.ListObjectKeyValue;

            if (regulatoryFeatureNodes != null)
            {
                // loop over each regulatory feature
                int regulatoryFeatureIndex = 0;
                foreach (DS.AbstractData abRegulatoryFeatureNode in regulatoryFeatureNodes)
                {
                    var regulatoryFeatureNode = abRegulatoryFeatureNode as DS.ObjectValue;

                    // sanity check: make sure this node is an object value
                    if (regulatoryFeatureNode == null)
                    {
                        Console.WriteLine("Expected a regulatory feature object value node, but the current node is not an object value.");
                        Environment.Exit(1);
                    }

                    // sanity check: make sure this is a regulatory feature data type
                    if (regulatoryFeatureNode.DataType != Import.RegulatoryFeature.DataType)
                    {
                        Console.WriteLine("Expected a regulatory feature node, but the current data type is: [{0}]", regulatoryFeatureNode.DataType);
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
                ParseTranscriptDumpFilePass(reader, Import.Transcript.Parse);

                // second pass: setting references
                ParseTranscriptDumpFilePass(reader, Import.Transcript.ParseReferences);

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
                ParseRegulatoryDumpFilePass(reader, Import.RegulatoryFeature.Parse);
            }
        }

        public static string GetDatabasePath(string currentRefSeq, string outputDir)
        {
            return Path.Combine(outputDir, $"{currentRefSeq}.ndb");
        }

        public void ParseDumpDirectory(string refSeqDirectory, string currentRefSeq, string outputDir, GenomeAssembly genomeAssembly)
        {
            string[] transcriptFiles = Directory.GetFiles(refSeqDirectory, "*_transcripts_data_dumper.txt.gz");
            string[] regulatoryFiles = Directory.GetFiles(refSeqDirectory, "*_reg_regulatory_regions_data_dumper.txt.gz");
            string databasePath = GetDatabasePath(currentRefSeq, outputDir);

            using (var writer = new NirvanaDatabaseWriter(databasePath))
            {
                // parse the regulatory files
                foreach (string dumpPath in regulatoryFiles)
                {
                    ParseRegulatoryDumpFile(dumpPath);

                    // copy the data (this is to maintain the references)
                    // all transcript filtering is performed in here as well
                    _dataStore.CopyDataFrom(_tempDataStore);

                    Console.WriteLine("- data store: {0}", _dataStore);

                    _tempDataStore.Clear();
                }

                // parse the transcript files
                foreach (string dumpPath in transcriptFiles)
                {
                    ParseTranscriptDumpFile(dumpPath);

                    // copy the data (this is to maintain the references)
                    // all transcript filtering is performed in here as well
                    _dataStore.CopyDataFrom(_tempDataStore);

                    Console.WriteLine("- data store: {0}", _dataStore);

                    _tempDataStore.Clear();
                }

                // assign canonical transcripts (only activated for RefSeq)
                AssignCanonicalTranscripts();

                // convert the data from our temporary data stores to the actual Nirvana data structures
                NirvanaDataStore dataStore = DataStoreUtilities.ConvertData(_dataStore, _transcriptDataSource);
                dataStore.CacheHeader = new NirvanaDatabaseHeader(currentRefSeq, DateTime.UtcNow.Ticks, _vepReleaseTicks,
                    _vepVersion, NirvanaDatabaseCommon.SchemaVersion, NirvanaDatabaseCommon.DataVersion,
                    genomeAssembly, _transcriptDataSource);

                Console.WriteLine();
                Console.WriteLine("# exons:               {0}", dataStore.Exons.Count);
                Console.WriteLine("# introns:             {0}", dataStore.Introns.Count);
                Console.WriteLine("# cDNA maps:           {0}", dataStore.CdnaCoordinateMaps.Count);
                Console.WriteLine("# miRNAs:              {0}", dataStore.MicroRnas.Count);
                Console.WriteLine("# sifts:               {0}", dataStore.Sifts.Count);
                Console.WriteLine("# polyphens:           {0}", dataStore.PolyPhens.Count);
                Console.WriteLine("# transcripts:         {0}", dataStore.Transcripts.Count);
                Console.WriteLine("# regulatory features: {0}", dataStore.RegulatoryFeatures.Count);

                // write the Nirvana database file
                writer.Write(dataStore, currentRefSeq);

                // delete all of the master data
                _dataStore.Clear();
            }
        }

        /// <summary>
        /// loads the data in the gene symbols file
        /// </summary>
        public void LoadHgncIds(string geneSymbolsPath)
        {
            const int numExpectedCols = 10;
            int numEntries = 0;

            Console.Write("- loading HGNC ids... ");

            using (var reader = GZipUtilities.GetAppropriateStreamReader(geneSymbolsPath))
            {
                while (true)
                {
                    var line = reader.ReadLine();
                    if (string.IsNullOrEmpty(line)) break;

                    // skip comments
                    if (line.StartsWith("#")) continue;

                    var cols = line.Split('\t');
                    if (cols.Length != numExpectedCols)
                    {
                        throw new ApplicationException($"Expected {numExpectedCols} columns, but found {cols.Length}: [{line}]");
                    }

                    // localize the columns
                    var geneSymbol = cols[0];
                    var geneSymbolSource = cols[1];
                    var transcriptId = Import.Transcript.RemoveVersion(cols[2]);

                    int geneId;
                    if (!int.TryParse(cols[4], out geneId))
                    {
                        throw new ApplicationException($"Unable to convert geneId: [{cols[4]}] -{line}");
                    }

                    int? hgncId = null;
                    if (!string.IsNullOrEmpty(cols[5])) hgncId = int.Parse(cols[5]);

                    // create the gene info object
                    var geneInfo = new DS.GeneInfo
                    {
                        GeneId = geneId,
                        GeneSymbol = geneSymbol,
                        GeneSymbolSource = GeneSymbolSourceUtilities.GetGeneSymbolSourceFromString(geneSymbolSource),
                        HgncId = hgncId
                    };

                    Import.Transcript.AddGeneIdToSymbol(geneInfo.GeneId, geneInfo);
                    if(hgncId != null) Import.Transcript.AddHgncIdToSymbol((int)hgncId, geneInfo);
                    Import.Transcript.AddAccessionToGeneId(transcriptId, geneInfo.GeneId);

                    numEntries++;
                }
            }

            Console.WriteLine("{0} entries loaded.", numEntries);
        }

        /// <summary>
        /// loads the data in the gene symbols file
        /// </summary>
        public void LoadGeneSymbols(string geneSymbolsPath)
        {
            const int numExpectedCols = 6;
            int numEntries = 0;

            Console.Write("- loading gene symbols... ");

            using (var reader = GZipUtilities.GetAppropriateStreamReader(geneSymbolsPath))
            {
                while (true)
                {
                    var line = reader.ReadLine();
                    if (string.IsNullOrEmpty(line)) break;

                    // skip comments
                    if (line.StartsWith("#")) continue;

                    var cols = line.Split('\t');
                    if (cols.Length != numExpectedCols)
                    {
                        throw new ApplicationException($"Expected {numExpectedCols} columns, but found {cols.Length}: [{line}]");
                    }

                    // create the gene info object
                    int? hgncId = null;
                    if (!string.IsNullOrEmpty(cols[3])) hgncId = int.Parse(cols[3]);

                    var geneInfo = new DS.GeneInfo
                    {
                        GeneId = int.Parse(cols[0]),
                        GeneSymbol = cols[1],
                        GeneSymbolSource = GeneSymbolSourceUtilities.GetGeneSymbolSourceFromString(cols[2]),
                        HgncId = hgncId
                    };

                    Import.Transcript.AddGeneIdToSymbol(geneInfo.GeneId, geneInfo);
                    if (hgncId != null) Import.Transcript.AddHgncIdToSymbol((int)hgncId, geneInfo);

                    numEntries++;

                    // assign it to all of the accessions
                    var accessionString = cols[5];
                    if (string.IsNullOrEmpty(accessionString)) continue;

                    var accessions = accessionString.Split('|');
                    foreach (var accession in accessions)
                    {
                        var strippedAccession = Import.Transcript.RemoveVersion(accession);
                        Import.Transcript.AddAccessionToGeneId(strippedAccession, geneInfo.GeneId);
                    }
                }
            }

            Console.WriteLine("{0} entries loaded.", numEntries);
        }

        /// <summary>
        /// loads the data in the LRG data file
        /// </summary>
        public void LoadLrgData(string lrgPath)
        {
            Console.Write("- loading LRG transcript data... ");

            _lrgEntries = new HashSet<string>();

            using (var reader = new StreamReader(lrgPath))
            {
                reader.ReadLine();

                while (true)
                {
                    var line = reader.ReadLine();
                    if (string.IsNullOrEmpty(line)) break;

                    var cols = line.Split('\t');
                    if (cols.Length != 10)
                    {
                        throw new ApplicationException($"Expected 10 columns, but found {cols.Length}: [{line}]");
                    }

                    var lrgId = Import.Transcript.RemoveVersion(cols[5]);
                    _lrgEntries.Add(lrgId);
                }
            }

            Console.WriteLine("finished.");
        }
    }
}
