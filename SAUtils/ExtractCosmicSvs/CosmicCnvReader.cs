using System;
using System.Collections.Generic;
using System.IO;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Sequence;

namespace SAUtils.ExtractCosmicSvs
{
    public sealed class CosmicCnvReader:IDisposable
    {
        private readonly StreamReader _reader;
        private readonly IDictionary<string, IChromosome> _refToChrom;
        private readonly GenomeAssembly _assembly;

        private int _idIndex                    = -1;  
        private int _primarySiteIndex           = -1;
        private int _siteSubtypeOneIndex        = -1;
        private int _siteSubtypeTwoIndex        = -1;
        private int _siteSubtypeThreeIndex      = -1;
        private int _primaryHistologyIndex      = -1;
        private int _histologySubtypeOneIndex   = -1;
        private int _histologySubtypeTwoIndex   = -1;
        private int _histologySubtypeThreeIndex = -1;
        private int _copyNumberIndex            = -1;
        private int _cnvTypeIndex               = -1;
        private int _assemblyIndex              = -1;
        private int _chromStartStopIndex        = -1;
        private int _studyIdIndex               = -1;


        //CNV_ID  ID_GENE gene_name       ID_SAMPLE       ID_TUMOUR       Primary site    Site subtype 1  Site subtype 2  Site subtype 3  Primary histology       Histology subtype 1     Histology subtype 2     Histology subtype 3     SAMPLE_NAME     TOTAL_CN        MINOR_ALLELE    MUT_TYPE        ID_STUDY        GRCh    Chromosome:G_Start..G_Stop

        private const string CnvIdTag                 = "CNV_ID";
        private const string PrimarySiteTag           = "Primary site";
        private const string SiteSubtypeOneTag        = "Site subtype 1";
        private const string SiteSubtypeTwoTag        = "Site subtype 2";
        private const string SiteSubtypeThreeTag      = "Site subtype 3";
        private const string PrimaryHistologyTag      = "Primary histology";
        private const string HistologySubtypeOneTag   = "Histology subtype 1";
        private const string HistologySubtypeTwoTag   = "Histology subtype 2";
        private const string HistologySubtypeThreeTag = "Histology subtype 3";
        private const string TotalCopyNumberTag       = "TOTAL_CN";
        private const string CnvTypeTag               = "MUT_TYPE";
        private const string AssemblyTag              = "GRCh";
        private const string ChromStartStopTag        = "Chromosome:G_Start..G_Stop";
        private const string StudyIdTag               = "ID_STUDY";

        public CosmicCnvReader(Stream cnvStream, IDictionary<string, IChromosome> refNameToChorm, GenomeAssembly assembly)
        {
            _reader = new StreamReader(cnvStream);
            _refToChrom = refNameToChorm; 
            _assembly = assembly;
        }

        public IEnumerable<CosmicCnvItem> GetEntries()
        {
            var cnvDictionary = new Dictionary<int, CosmicCnvItem>();
            string line;
            var isFirstLine = true;
            while ((line = _reader.ReadLine()) != null)
            {
                // Skip empty lines.
                if (string.IsNullOrWhiteSpace(line)) continue;
                // Skip comments.
                if (isFirstLine)
                {
                    GetColumnIndices(line);
                    isFirstLine = false;
                    continue;
                }

                try
                {
                    var cnvItem = ExtractCosmicCnv(line);
                    if (cnvItem == null) continue;
                    if (cnvDictionary.TryGetValue(cnvItem.CNVId, out var value))
                        value.Merge(cnvItem);
                    else cnvDictionary[cnvItem.CNVId] = cnvItem;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Console.WriteLine(line);
                    throw;
                }
                
            }

            Console.WriteLine($"Found {cnvDictionary.Count} unique cosmic cnvs");
            return cnvDictionary.Values;
        }

        internal void GetColumnIndices(string headerLine)
        {
            //CNV_ID  ID_GENE gene_name       ID_SAMPLE       ID_TUMOUR       Primary site    Site subtype 1  Site subtype 2  Site subtype 3  Primary histology       Histology subtype 1     Histology subtype 2     Histology subtype 3     SAMPLE_NAME     TOTAL_CN        MINOR_ALLELE    MUT_TYPE        ID_STUDY        GRCh    Chromosome:G_Start..G_Stop

            _idIndex                    = -1;
            _primarySiteIndex           = -1;
            _siteSubtypeOneIndex        = -1;
            _siteSubtypeTwoIndex        = -1;
            _siteSubtypeThreeIndex      = -1;
            _primaryHistologyIndex      = -1;
            _histologySubtypeOneIndex   = -1;
            _histologySubtypeTwoIndex   = -1;
            _histologySubtypeThreeIndex = -1;
            _copyNumberIndex            = -1;
            _cnvTypeIndex               = -1;
            _assemblyIndex              = -1;
            _chromStartStopIndex        = -1;
            _studyIdIndex               = -1;

            var columns = headerLine.Split('\t');
            for (int i = 0; i < columns.Length; i++)
            {
                switch (columns[i])
                {
                    case CnvIdTag:
                        _idIndex = i;
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

                    case TotalCopyNumberTag:
                        _copyNumberIndex = i;
                        break;
                    case CnvTypeTag:
                        _cnvTypeIndex = i;
                        break;
                    case AssemblyTag:
                        _assemblyIndex = i;
                        break;
                    case ChromStartStopTag:
                        _chromStartStopIndex = i;
                        break;
                    case StudyIdTag:
                        _studyIdIndex = i;
                        break;
                }
            }

            if (_primarySiteIndex == -1 || _siteSubtypeThreeIndex == -1 || _siteSubtypeOneIndex == -1 || _siteSubtypeTwoIndex == -1)
                throw new InvalidDataException("Column for some site(s) could not be detected");
            if (_primaryHistologyIndex == -1 || _histologySubtypeOneIndex == -1 || _histologySubtypeTwoIndex == -1 || _histologySubtypeThreeIndex == -1)
                throw new InvalidDataException("Column for some histology(ies) could not be detected");
            if (_copyNumberIndex == -1 || _assemblyIndex == -1 || _chromStartStopIndex == -1 || _cnvTypeIndex == -1)
                throw new InvalidDataException("Column for some CNV details could not be detected");
            if (_studyIdIndex ==-1)
                throw new InvalidDataException("No study Id column detected");

        }

        private CosmicCnvItem ExtractCosmicCnv(string line)
        {
            
            var splits = line.Split('\t');

            if (splits.Length == 1) return null;

            var assembly = GenomeAssembly.Unknown;
            var assemblyString = splits[_assemblyIndex];

            if (assemblyString == "37") assembly = GenomeAssembly.GRCh37;
            if (assemblyString == "38") assembly = GenomeAssembly.GRCh38;

            if (assembly != _assembly) return null;

            var cnvId = int.Parse(splits[_idIndex]);

            var studyId = int.Parse(splits[_studyIdIndex]);

            var cancerTypes = new Dictionary<string, int>();

            TryAddValue(cancerTypes, splits[_primaryHistologyIndex]);
            TryAddValue(cancerTypes, splits[_histologySubtypeOneIndex]);
            TryAddValue(cancerTypes, splits[_histologySubtypeTwoIndex]);
            TryAddValue(cancerTypes, splits[_histologySubtypeThreeIndex]);

            var tissueTypes = new Dictionary<string, int>();

            TryAddValue(tissueTypes, splits[_primarySiteIndex]);
            TryAddValue(tissueTypes, splits[_siteSubtypeOneIndex]);
            TryAddValue(tissueTypes, splits[_siteSubtypeTwoIndex]);
            TryAddValue(tissueTypes, splits[_siteSubtypeThreeIndex]);

            if (! int.TryParse(splits[_copyNumberIndex], out var copyNumber))
            {
                copyNumber = -1;
            }

            var cnvType = VariantType.copy_number_variation;
            if (splits[_cnvTypeIndex] == "gain") cnvType = VariantType.copy_number_gain;
            if (splits[_cnvTypeIndex] == "loss") cnvType = VariantType.copy_number_loss;

            (string chrom, int start, int end) = GetChromStartStop(splits[_chromStartStopIndex]);

            return new CosmicCnvItem(cnvId, _refToChrom[chrom], start, end, cnvType, copyNumber, cancerTypes, tissueTypes, studyId);
        }

        private static (string, int, int) GetChromStartStop(string chromPos)
        {
            //17:18358950..18464587 Chromosome:G_Start..G_Stop
            var splits = chromPos.Split(':', '.');
            var chrom = splits[0];
            if (chrom == "25") chrom = "MT";
            return (chrom, int.Parse(splits[1]), int.Parse(splits[3]));
        }

        
        private static void TryAddValue(Dictionary<string, int> cancerTypes, string type)
        {
            if (string.IsNullOrEmpty(type) || type == "NS") return;

            cancerTypes[type]=1;//we don't care about overriding the old count since this is for one study. So counts should not add up
        }

        public void Dispose()
        {
            _reader?.Dispose();
        }
    }
}