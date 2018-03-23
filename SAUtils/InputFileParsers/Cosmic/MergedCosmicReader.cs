using System;
using System.Collections.Generic;
using System.IO;
using SAUtils.DataStructures;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Sequence;

namespace SAUtils.InputFileParsers.Cosmic
{
    public sealed class MergedCosmicReader 
    {
        #region members

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

        private const string MutationIdTag       = "Mutation ID";
        private const string PrimarySiteTag      = "Primary site";
        private const string SiteSubtypeOneTag   = "Site subtype 1";
        private const string SiteSubtypeTwoTag   = "Site subtype 2";
        private const string SiteSubtypeThreeTag = "Site subtype 3";

        private const string PrimaryHistologyTag      = "Primary histology";
        private const string HistologySubtypeOneTag   = "Histology subtype 1";
        private const string HistologySubtypeTwoTag   = "Histology subtype 2";
        private const string HistologySubtypeThreeTag = "Histology subtype 3";

        private const string StudyIdTag = "ID_STUDY";

        private readonly IDictionary<string, IChromosome> _refChromDict;
        private readonly Dictionary<string, HashSet<CosmicItem.CosmicStudy>> _studies;

        #endregion

        // constructor
        public MergedCosmicReader(StreamReader vcfFileReader, StreamReader tsvFileReader, IDictionary<string, IChromosome> refChromDict)
        {
            _vcfFileReader = vcfFileReader;
            _tsvFileReader = tsvFileReader;
            _refChromDict  = refChromDict;
            _studies       = new Dictionary<string, HashSet<CosmicItem.CosmicStudy>>();
        }

        
        public IEnumerable<CosmicItem> GetCosmicItems()
        {
            //taking up all studies in to the dictionary
            using (_tsvFileReader)
            {
                string line;
                while ((line = _tsvFileReader.ReadLine()) != null)
                {
                    if (IsHeaderLine(line))
                        GetColumnIndexes(line);//the first line is supposed to be a the header line
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
                    if (line.StartsWith("#")) continue;
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
            var columns = line.Split('\t');

            var mutationId  = columns[_mutationIdIndex];
            var studyId     = columns[_studyIdIndex];
            var sites       = GetSites(columns);
            var histologies = GetHistologies(columns);

            if (string.IsNullOrEmpty(mutationId)) return;

            var study = new CosmicItem.CosmicStudy(studyId, histologies, sites);
            if (_studies.TryGetValue(mutationId, out var studySet))
                studySet.Add(study);
            else _studies[mutationId] = new HashSet<CosmicItem.CosmicStudy> { study };
        }

        private IEnumerable<string> GetHistologies(string[] columns)
        {
            var histologies = new HashSet<string>();

            TryAddValue(columns[_primaryHistologyIndex], histologies);
            TryAddValue(columns[_histologySubtypeOneIndex], histologies);
            TryAddValue(columns[_histologySubtypeTwoIndex], histologies);
            TryAddValue(columns[_histologySubtypeThreeIndex], histologies);

            return histologies;
        }

        private IEnumerable<string> GetSites(string[] columns)
        {
            var sites = new HashSet<string>();

            TryAddValue(columns[_primarySiteIndex], sites);
            TryAddValue(columns[_siteSubtypeOneIndex], sites);
            TryAddValue(columns[_siteSubtypeTwoIndex], sites);
            TryAddValue(columns[_siteSubtypeThreeIndex], sites);

            return sites;
        }

        private void TryAddValue(string value, HashSet<string> sites)
        {
           if (!string.IsNullOrEmpty(value) && value != "NS")
                sites.Add(value);
        }

        private static bool IsHeaderLine(string line)
        {
            return line.Contains(StudyIdTag);
        }

        private void GetColumnIndexes(string headerLine)
        {
            //Gene name       Accession Number        Gene CDS length HGNC ID Sample name     ID_sample       ID_tumour       Primary site    Site subtype 1  Site subtype 2  Site subtype 3  Primary histology       Histology subtype 1     Histology subtype 2     Histology subtype 3     Genome-wide screen      Mutation ID     Mutation CDS    Mutation AA     Mutation Description    Mutation zygosity       LOH     GRCh    Mutation genome position        Mutation strand SNP     FATHMM prediction       FATHMM score    Mutation somatic status Pubmed_PMID     ID_STUDY        Sample source   Tumour origin   Age

            _mutationIdIndex       = -1;
            _studyIdIndex          = -1;
            _primarySiteIndex      = -1;
            _primaryHistologyIndex = -1;

            var columns = headerLine.Split('\t');
            for (int i = 0; i < columns.Length; i++)
            {
                switch (columns[i])
                {
                    case MutationIdTag:
                        _mutationIdIndex = i;
                        break;
                    case StudyIdTag:
                        _studyIdIndex = i;
                        break;
                    case PrimarySiteTag:
                        _primarySiteIndex = i;
                        break;
                    case SiteSubtypeOneTag:
                        _siteSubtypeOneIndex = i;
                        break;
                    case SiteSubtypeTwoTag:
                        _siteSubtypeTwoIndex = i;
                        break;
                    case SiteSubtypeThreeTag:
                        _siteSubtypeThreeIndex = i;
                        break;

                    case PrimaryHistologyTag:
                        _primaryHistologyIndex = i;
                        break;
                    case HistologySubtypeOneTag:
                        _histologySubtypeOneIndex = i;
                        break;
                    case HistologySubtypeTwoTag:
                        _histologySubtypeTwoIndex = i;
                        break;
                    case HistologySubtypeThreeTag:
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

        internal List<CosmicItem> ExtractCosmicItems(string vcfLine)
        {
            var splitLine = vcfLine.Split(new[] { '\t' }, 8);

            var chromosomeName = splitLine[VcfCommon.ChromIndex];
            if (!_refChromDict.ContainsKey(chromosomeName)) return null;

            var chromosome = _refChromDict[chromosomeName];

            var position   = int.Parse(splitLine[VcfCommon.PosIndex]);
            var cosmicId   = splitLine[VcfCommon.IdIndex];
            var refAllele  = splitLine[VcfCommon.RefIndex];
            var altAlleles = splitLine[VcfCommon.AltIndex].Split(',');
            var infoField  = splitLine[VcfCommon.InfoIndex];

            Clear();

            ParseInfoField(infoField);

            var cosmicItems = new List<CosmicItem>();

            foreach (var altAllele in altAlleles)
            {
                if (_studies.TryGetValue(cosmicId, out var studies))
                {
                    cosmicItems.Add(new CosmicItem(chromosome, position, cosmicId, refAllele, altAllele, _geneName, studies, _sampleCount));
                }
                else cosmicItems.Add(new CosmicItem(chromosome, position, cosmicId, refAllele, altAllele, _geneName, null, _sampleCount));
                
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

            var infoItems = infoFields.Split(';');
            foreach (var infoItem in infoItems)
            {
                if (string.IsNullOrEmpty(infoItem)) continue;

                var infoKeyValue = infoItem.Split('=');
                if (infoKeyValue.Length == 2)//sanity check
                {
                    var key = infoKeyValue[0];
                    var value = infoKeyValue[1];

                    SetInfoField(key, value);
                }
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
