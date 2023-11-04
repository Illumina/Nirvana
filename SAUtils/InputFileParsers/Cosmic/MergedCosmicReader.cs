using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Compression.Utilities;
using Genome;
using OptimizedCore;
using SAUtils.DataStructures;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Providers;
using Variants;

namespace SAUtils.InputFileParsers.Cosmic
{
    public sealed class MergedCosmicReader 
    {
        private readonly StreamReader _vcfFileReader;
        private readonly StreamReader _tsvFileReader;
        private string _geneName;
        private int? _sampleCount;

        private int _cosmicIdIndex         = -1;
        private int _primarySiteIndex      = -1;
        private int _primaryHistologyIndex = -1;
        private int _tumorIdIndex          = -1;
        private int _tierIndex             = -1;

        private const string TumorIdTag = "ID_tumour";

        private readonly Dictionary<string, Chromosome> _refChromDict;
        private readonly ISequenceProvider _sequenceProvider;
        private readonly Dictionary<string, HashSet<CosmicItem.CosmicTumor>> _tumors;

        public MergedCosmicReader(string vcfFile, string tsvFile, ISequenceProvider sequenceProvider)
        {
            _vcfFileReader = GZipUtilities.GetAppropriateStreamReader(vcfFile);
            _tsvFileReader = GZipUtilities.GetAppropriateStreamReader(tsvFile);
            _sequenceProvider = sequenceProvider;
            _refChromDict  = _sequenceProvider.RefNameToChromosome;
            _tumors = new Dictionary<string, HashSet<CosmicItem.CosmicTumor>>();
        }
        
        public IEnumerable<CosmicItem> GetItems()
        {
            // taking up all tumors in to the dictionary
            using (_tsvFileReader)
            {
                string line;
                while ((line = _tsvFileReader.ReadLine()) != null)
                {
                    if (IsHeaderLine(line))
                        GetColumnIndexes(line); // the first line is supposed to be a the header line
                    else AddCosmicTumor(line);
                }
            }

            using (_vcfFileReader)
            {
                string line;
                while ((line = _vcfFileReader.ReadLine()) != null)
                {
                    // Skip empty lines.
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    // Skip comments.
                    if (line.OptimizedStartsWith('#')) continue;
                    var cosmicItems = ExtractCosmicItems(line);
                    if (cosmicItems == null) continue;

                    foreach (var cosmicItem in cosmicItems)
                    {
                        yield return cosmicItem;
                    }
                }
            }
        }

        private void AddCosmicTumor(string line)
        {
            var columns = line.OptimizedSplit('\t');

            string cosmicId    = columns[_cosmicIdIndex];
            string tumorId     = columns[_tumorIdIndex];
            string site        = GetString(columns[_primarySiteIndex]);
            string histology   = GetString(columns[_primaryHistologyIndex]);
            string tier        = GetString(columns[_tierIndex]);

            if (string.IsNullOrEmpty(cosmicId)) return;

            var tumor = new CosmicItem.CosmicTumor(tumorId, histology, site, tier);
            if (_tumors.TryGetValue(cosmicId, out var tumorSet))
                tumorSet.Add(tumor);
            else _tumors[cosmicId] = new HashSet<CosmicItem.CosmicTumor> { tumor };
        }

        private string GetString(string value)
        {
            if (string.IsNullOrEmpty(value) || value == "NS")
                return null;
            value = value.Replace('_', ' ');
            return value;
        }

        private static bool IsHeaderLine(string line) => line.Contains(TumorIdTag);

        private void GetColumnIndexes(string headerLine)
        {
            //Gene name       Accession Number        Gene CDS length HGNC ID Sample name     ID_sample       ID_tumour       Primary site    Site subtype 1  Site subtype 2  Site subtype 3  Primary histology       Histology subtype 1     Histology subtype 2     Histology subtype 3     Genome-wide screen      GENOMIC_MUTATION_ID     LEGACY_MUTATION_ID      MUTATION_ID     Mutation CDS    Mutation AA     Mutation Description    Mutation zygosity       LOH     GRCh    Mutation genome position        Mutation strand SNP     Resistance Mutation     FATHMM prediction       FATHMM score    Mutation somatic status Pubmed_PMID     ID_STUDY        Sample Type     Tumour origin   Age     Tier    HGVSP   HGVSC   HGVSG

            _cosmicIdIndex         = -1;
            _tumorIdIndex          = -1;
            _primarySiteIndex      = -1;
            _primaryHistologyIndex = -1;
            _tierIndex             = -1;

            var columns = headerLine.OptimizedSplit('\t');
            for (int i = 0; i < columns.Length; i++)
            {
                switch (columns[i])
                {
                    case "GENOMIC_MUTATION_ID":
                        _cosmicIdIndex = i;
                        break;
                    case TumorIdTag:
                        _tumorIdIndex = i;
                        break;
                    case "Primary site":
                        _primarySiteIndex = i;
                        break;
                    case "Primary histology":
                        _primaryHistologyIndex = i;
                        break;
                    case "Tier":
                        _tierIndex = i;
                        break;
                }
            }

            if (_cosmicIdIndex == -1)
                throw new InvalidDataException("Column for Cosmic Id could not be detected");
            if (_tumorIdIndex == -1)
                throw new InvalidDataException("Column for tumor Id could not be detected");
            if (_primarySiteIndex == -1)
                throw new InvalidDataException("Column for primary site could not be detected");
            if (_primaryHistologyIndex == -1)
                throw new InvalidDataException("Column for primary histology could not be detected");
            if (_tierIndex == -1)
                throw new InvalidDataException("Column for tier could not be decteded");
        }

        private const int MaxVariantLength= 1000;
        internal List<CosmicItem> ExtractCosmicItems(string vcfLine)
        {
            var splitLine = vcfLine.Split(new[] { '\t' }, 8);
            //skipping large variants
            if (splitLine[VcfCommon.RefIndex].Length > MaxVariantLength || splitLine[VcfCommon.AltIndex].Length > MaxVariantLength) return null;

            string chromosomeName = splitLine[VcfCommon.ChromIndex];
            if (!_refChromDict.ContainsKey(chromosomeName)) return null;

            var chromosome    = _refChromDict[chromosomeName];
            int position      = int.Parse(splitLine[VcfCommon.PosIndex]);
            string cosmicId   = splitLine[VcfCommon.IdIndex];
            string refAllele  = splitLine[VcfCommon.RefIndex];
            var altAlleles    = splitLine[VcfCommon.AltIndex].OptimizedSplit(',');
            string infoField  = splitLine[VcfCommon.InfoIndex];

            Clear();

            ParseInfoField(infoField);

            var cosmicItems = new List<CosmicItem>();

            foreach (string altAllele in altAlleles)
            {
                var (shiftedPos, shiftedRef, shiftedAlt) = VariantUtils.TrimAndLeftAlign(position, refAllele,
                    altAllele, _sequenceProvider.Sequence);

                cosmicItems.Add(_tumors.TryGetValue(cosmicId, out var studies)
                    ? new CosmicItem(chromosome, shiftedPos, cosmicId, shiftedRef, shiftedAlt, _geneName, studies,
                        _sampleCount)
                    : new CosmicItem(chromosome, shiftedPos, cosmicId, shiftedRef, shiftedAlt, _geneName, null,
                        _sampleCount));
            }

            return cosmicItems;
        }

        private void Clear()
        {
            _geneName    = null;
            _sampleCount = null;
        }

        private void ParseInfoField(string infoFields)
        {
            if (infoFields == "" || infoFields == ".") return;
            var infoItems = infoFields.OptimizedSplit(';');

            foreach (string infoItem in infoItems)
            {
                if (string.IsNullOrEmpty(infoItem)) continue;

                (string key, string value) = infoItem.OptimizedKeyValue();

                // sanity check
                if (value != null) SetInfoField(key, value);
            }
        }

        private void SetInfoField(string vcfId, string value)
        {
            switch (vcfId)
            {
                case "GENE":
                    _geneName = value;
                    break;
                case "CNT":
                    _sampleCount = Convert.ToInt32(value);
                    break;
            }
        }       
    }
}
