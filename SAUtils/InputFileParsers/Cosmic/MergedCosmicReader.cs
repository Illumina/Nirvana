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

        private int _mutationIdIndex       = -1;
        private int _primarySiteIndex      = -1;
        private int _siteSubtypeOneIndex   = -1;
        private int _siteSubtypeTwoIndex   = -1;
        private int _siteSubtypeThreeIndex = -1;

        private int _primaryHistologyIndex      = -1;
        private int _histologySubtypeOneIndex   = -1;
        private int _histologySubtypeTwoIndex   = -1;
        private int _histologySubtypeThreeIndex = -1;

        private int _studyIdIndex = -1;

        private const string StudyIdTag = "ID_STUDY";

        private readonly IDictionary<string, IChromosome> _refChromDict;
        private readonly ISequenceProvider _sequenceProvider;
        private readonly Dictionary<string, HashSet<CosmicItem.CosmicStudy>> _studies;

        public MergedCosmicReader(string vcfFile, string tsvFile, ISequenceProvider sequenceProvider)
        {
            _vcfFileReader = GZipUtilities.GetAppropriateStreamReader(vcfFile);
            _tsvFileReader = GZipUtilities.GetAppropriateStreamReader(tsvFile);
            _sequenceProvider = sequenceProvider;
            _refChromDict  = _sequenceProvider.RefNameToChromosome;
            _studies       = new Dictionary<string, HashSet<CosmicItem.CosmicStudy>>();
        }
        
        public IEnumerable<CosmicItem> GetItems()
        {
            // taking up all studies in to the dictionary
            using (_tsvFileReader)
            {
                string line;
                while ((line = _tsvFileReader.ReadLine()) != null)
                {
                    if (IsHeaderLine(line))
                        GetColumnIndexes(line); // the first line is supposed to be a the header line
                    else AddCosmicStudy(line);
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

        private void AddCosmicStudy(string line)
        {
            var columns = line.OptimizedSplit('\t');

            string mutationId  = columns[_mutationIdIndex];
            string studyId     = columns[_studyIdIndex];
            var sites          = GetSites(columns);
            var histologies    = GetHistologies(columns);
            
            if (string.IsNullOrEmpty(mutationId)) return;

            var study = new CosmicItem.CosmicStudy(studyId, histologies, sites);
            if (_studies.TryGetValue(mutationId, out var studySet))
                studySet.Add(study);
            else _studies[mutationId] = new HashSet<CosmicItem.CosmicStudy> { study };
        }

        private IList<string> GetHistologies(string[] columns)
        {
            var histologies = new HashSet<string>();
            var primaryHistology = columns[_primaryHistologyIndex].Replace('_', ' ');
            TryAddValue(primaryHistology, histologies);
            //TryAddValue(columns[_histologySubtypeOneIndex], histologies);
            //TryAddValue(columns[_histologySubtypeTwoIndex], histologies);
            //TryAddValue(columns[_histologySubtypeThreeIndex], histologies);

            return histologies.ToList();
        }

        private IList<string> GetSites(string[] columns)
        {
            var sites = new HashSet<string>();

            var primarySite = columns[_primarySiteIndex].Replace('_', ' ');
            TryAddValue(primarySite, sites);
            //TryAddValue(columns[_siteSubtypeOneIndex], sites);
            //TryAddValue(columns[_siteSubtypeTwoIndex], sites);
            //TryAddValue(columns[_siteSubtypeThreeIndex], sites);

            return sites.ToList();
        }

        private static void TryAddValue(string value, ISet<string> sites)
        {
           if (!string.IsNullOrEmpty(value) && value != "NS")
                sites.Add(value);
        }

        private static bool IsHeaderLine(string line) => line.Contains(StudyIdTag);

        private void GetColumnIndexes(string headerLine)
        {
            //Gene name       Accession Number        Gene CDS length HGNC ID Sample name     ID_sample       ID_tumour       Primary site    Site subtype 1  Site subtype 2  Site subtype 3  Primary histology       Histology subtype 1     Histology subtype 2     Histology subtype 3     Genome-wide screen      Mutation ID     Mutation CDS    Mutation AA     Mutation Description    Mutation zygosity       LOH     GRCh    Mutation genome position        Mutation strand SNP     FATHMM prediction       FATHMM score    Mutation somatic status Pubmed_PMID     ID_STUDY        Sample source   Tumour origin   Age

            _mutationIdIndex       = -1;
            _studyIdIndex          = -1;
            _primarySiteIndex      = -1;
            _primaryHistologyIndex = -1;

            var columns = headerLine.OptimizedSplit('\t');
            for (int i = 0; i < columns.Length; i++)
            {
                switch (columns[i])
                {
                    case "Mutation ID":
                        _mutationIdIndex = i;
                        break;
                    case StudyIdTag:
                        _studyIdIndex = i;
                        break;
                    case "Primary site":
                        _primarySiteIndex = i;
                        break;
                    case "Site subtype 1":
                        _siteSubtypeOneIndex = i;
                        break;
                    case "Site subtype 2":
                        _siteSubtypeTwoIndex = i;
                        break;
                    case "Site subtype 3":
                        _siteSubtypeThreeIndex = i;
                        break;
                    case "Primary histology":
                        _primaryHistologyIndex = i;
                        break;
                    case "Histology subtype 1":
                        _histologySubtypeOneIndex = i;
                        break;
                    case "Histology subtype 2":
                        _histologySubtypeTwoIndex = i;
                        break;
                    case "Histology subtype 3":
                        _histologySubtypeThreeIndex = i;
                        break;
                }
            }

            if (_mutationIdIndex == -1)
                throw new InvalidDataException("Column for mutation Id could not be detected");
            if (_studyIdIndex == -1)
                throw new InvalidDataException("Column for study Id could not be detected");
            if (_primarySiteIndex == -1)
                throw new InvalidDataException("Column for primary site could not be detected");
            if (_primaryHistologyIndex == -1)
                throw new InvalidDataException("Column for primary histology could not be detected");
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

                cosmicItems.Add(_studies.TryGetValue(cosmicId, out var studies)
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
