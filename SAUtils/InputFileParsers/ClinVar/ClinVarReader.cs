using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using VariantAnnotation.DataStructures.SupplementaryAnnotations;
using VariantAnnotation.FileHandling;

namespace SAUtils.InputFileParsers.ClinVar
{
    public class ClinVarAdditionalInfo
    {
        public HashSet<long> PubmedIds = new HashSet<long>();
        public long LastEvaluatedDate = long.MinValue;
        public HashSet<int> RcvAccessions = new HashSet<int>();
    }

    /// <summary>
    /// Parser for ClinVar file
    /// </summary>
    public sealed class ClinVarReader : IEnumerable<ClinVarItem>
    {
        #region members

        private readonly FileInfo _clinVarFileInfo;
        internal readonly Dictionary<long, ClinVarAdditionalInfo> AdditionalInfos;
        private readonly Dictionary<int, ClinVarAdditionalInfo> _accessionDictionary;
        private const int DictSize = 132977;

        internal int LastEvaluationAlleleIdIndex = -1;
        internal int LastEvaluatedColumnIndex = -1;
        internal int AccessionIndex = -1;
        internal int CitationIdColumnIndex = -1;
        internal int CitationSourceColumnIndex = -1;
        internal int CitationAlleleIdColumnIndex = -1;
        private int _noAccession;

        #endregion

        #region IEnumerable implementation

        public IEnumerator<ClinVarItem> GetEnumerator()
        {
            return GetClinVarItems().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        // constructor
        public ClinVarReader(FileInfo clinVarFileInfo, FileInfo citationFileInfo = null, FileInfo variantSummaryFileInfo = null) : this(DictSize)
        {
            _clinVarFileInfo = clinVarFileInfo;

            ExtractCitations(citationFileInfo);
            ExtractLastEvaluationDate(variantSummaryFileInfo);

            CreateAccessionDictionary();
        }

        internal ClinVarReader(int size = 1)
        {
            //empty constructor for unit tests
            AdditionalInfos = new Dictionary<long, ClinVarAdditionalInfo>(size);//initializing to the number of lines in citation file
            _accessionDictionary = new Dictionary<int, ClinVarAdditionalInfo>(size);
        }

        public HashSet<long> GetPubmedIds(uint alleleId)
        {
            if (AdditionalInfos.ContainsKey(alleleId)) return AdditionalInfos[alleleId].PubmedIds;
            return null;
        }

        public long GetLastEvaluated(uint alleleId)
        {
            if (AdditionalInfos.ContainsKey(alleleId)) return AdditionalInfos[alleleId].LastEvaluatedDate;
            return 0;
        }

        public HashSet<int> GetAccession(int alleleId)
        {
            if (AdditionalInfos.ContainsKey(alleleId)) return AdditionalInfos[alleleId].RcvAccessions;
            return null;
        }

        internal void CreateAccessionDictionary()
        {
            foreach (var additionalInfo in AdditionalInfos)
            {
                if (additionalInfo.Value.RcvAccessions.Count == 0)
                {
                    //e.g. 38835 does not have any accession
                    //Console.WriteLine("Warning!! No RCV accession for AlleleID: "+additionalInfo.Key);
                    _noAccession++;
                }

                foreach (var accession in additionalInfo.Value.RcvAccessions)
                {
                    if (_accessionDictionary.ContainsKey(accession))
                    {
                        _accessionDictionary[accession].PubmedIds.UnionWith(additionalInfo.Value.PubmedIds);

                        //you may have multiple last evaluation dates for an accession. e.g. RCV000001981;
                        _accessionDictionary[accession].LastEvaluatedDate =
                            _accessionDictionary[accession].LastEvaluatedDate > additionalInfo.Value.LastEvaluatedDate
                                ? _accessionDictionary[accession].LastEvaluatedDate
                                : additionalInfo.Value.LastEvaluatedDate;
                    }
                    else
                    {
                        _accessionDictionary[accession] = new ClinVarAdditionalInfo { PubmedIds = new HashSet<long>() };
                        _accessionDictionary[accession].PubmedIds.UnionWith(additionalInfo.Value.PubmedIds);
                        _accessionDictionary[accession].LastEvaluatedDate = additionalInfo.Value.LastEvaluatedDate;
                    }
                }
            }
            Console.WriteLine("Warning!! {0} allele ids did not have any corresponding accession id", _noAccession);
        }

        public ClinVarAdditionalInfo GetAdditionalInfo(string accession)
        {
            //RCV000000013.1
            var accessionId = Convert.ToInt32(accession.Substring(3).Split('.')[0]);
            if (_accessionDictionary.ContainsKey(accessionId))
                return _accessionDictionary[accessionId];

            return null;
        }

        internal void ExtractLastEvaluationDate(FileInfo variantSummaryFileInfo)
        {
            if (variantSummaryFileInfo == null) return;
            using (var reader = GZipUtilities.GetAppropriateStreamReader(variantSummaryFileInfo.FullName))
            {
                while (true)
                {
                    // grab the next line
                    string line = reader.ReadLine();
                    if (line == null) break;

                    // skip comments and empty lines
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    try
                    {
                        if (line.StartsWith("#")) ParseLastEvaluatedHeaderLine(line);
                        else ParseLastEvaluatedDataLine(line);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("invalid data in line:");
                        Console.WriteLine(line);
                        throw;
                    }
                }
            }

        }

        internal void ExtractCitations(FileInfo citationFileInfo)
        {
            if (citationFileInfo == null) return;

            using (var reader = GZipUtilities.GetAppropriateStreamReader(citationFileInfo.FullName))
            {
                while (true)
                {
                    // grab the next line
                    string line = reader.ReadLine();
                    if (line == null) break;

                    // skip comments and empty lines
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    if (line.StartsWith("#")) ParseCitationHeaderLine(line);
                    else ParseCitationDataLine(line);
                }
            }

        }

        internal void ParseCitationHeaderLine(string line)
        {
            //#AlleleID       VariationID     rs      nsv     citation_source citation_id
            line = line.TrimStart('#');
            var fieldNames = line.Split('\t');
            for (int i = 0; i < fieldNames.Length; i++)
            {
                switch (fieldNames[i])
                {
                    case "AlleleID":
                        CitationAlleleIdColumnIndex = i;
                        break;
                    case "citation_source":
                        CitationSourceColumnIndex = i;
                        break;
                    case "citation_id":
                        CitationIdColumnIndex = i;
                        break;
                }
            }
        }

        internal void ParseCitationDataLine(string line)
        {
            if (CitationIdColumnIndex == -1 || CitationAlleleIdColumnIndex == -1)
                throw new InvalidDataException("Data column not found");
            var fields = line.Split('\t');

            if (fields[CitationSourceColumnIndex] != "PubMed") return;//we only want pubmed ids

            var alleleId = Convert.ToInt64(fields[CitationAlleleIdColumnIndex]);
            var pubmedId = Convert.ToInt64(fields[CitationIdColumnIndex]);

            if (AdditionalInfos.ContainsKey(alleleId))
            {
                if (AdditionalInfos[alleleId].PubmedIds == null)
                    AdditionalInfos[alleleId].PubmedIds = new HashSet<long>();

                AdditionalInfos[alleleId].PubmedIds.Add(pubmedId);

            }
            else AdditionalInfos[alleleId] = new ClinVarAdditionalInfo()
            {
                PubmedIds = new HashSet<long>() { pubmedId }
            };

        }

        internal void ParseLastEvaluatedDataLine(string line)
        {
            if (LastEvaluatedColumnIndex == -1 || LastEvaluationAlleleIdIndex == -1 || AccessionIndex == -1)
                throw new InvalidDataException("Data column not found for last evaluated information");
            var fields = line.Split('\t');

            var alleleId = Convert.ToInt64(fields[LastEvaluationAlleleIdIndex]);
            var rcvAccessions = new HashSet<int>();




            foreach (var accession in fields[AccessionIndex].Split(';'))
            {
                if (!string.IsNullOrEmpty(accession))
                    rcvAccessions.Add(Convert.ToInt32(accession.Substring(3)));
            }

            var lastEvaluation = ParseDate(fields[LastEvaluatedColumnIndex]);

            if (AdditionalInfos.ContainsKey(alleleId))
            {
                AdditionalInfos[alleleId].LastEvaluatedDate = lastEvaluation;
                AdditionalInfos[alleleId].RcvAccessions.UnionWith(rcvAccessions);
            }
            else
                AdditionalInfos[alleleId] = new ClinVarAdditionalInfo()
                {
                    LastEvaluatedDate = lastEvaluation,
                    RcvAccessions = rcvAccessions
                };
        }

        internal long ParseDate(string s)
        {
            if (s == "-") return long.MinValue;
            //Jun 29, 2010
            return DateTime.Parse(s).Ticks;

        }


        internal void ParseLastEvaluatedHeaderLine(string line)
        {
            //#AlleleID       Type    Name    GeneID  GeneSymbol      ClinicalSignificance    RS# (dbSNP)     nsv (dbVar)     RCVaccession    TestedInGTR     PhenotypeIDs    Origin  Assembly        Chromosome      Start   Stop    Cytogenetic     ReviewStatus    HGVS(c.)        HGVS(p.)        NumberSubmitters        LastEvaluated   Guidelines      OtherIDs        VariantID       ReferenceAllele AlternateAllele SubmitterCategories     ChromosomeAccession

            line = line.TrimStart('#');
            var fieldNames = line.Split('\t');
            for (int i = 0; i < fieldNames.Length; i++)
            {
                switch (fieldNames[i])
                {
                    case "AlleleID":
                        LastEvaluationAlleleIdIndex = i;
                        break;
                    case "RCVaccession":
                        AccessionIndex = i;
                        break;
                    case "LastEvaluated":
                        LastEvaluatedColumnIndex = i;
                        break;
                }
            }
        }

        /// <summary>
        /// returns a ClinVar object given the vcf line
        /// </summary>
        public List<ClinVarItem> ExtractClinVarItems(string line)
        {
            //taking care of the HP: in clinvar vcf info
            line = line.Replace("HP:", "HPO");

            var cols = line.Split('\t');
            if (cols.Length < 8) return null;

            var chromosome = cols[VcfCommon.ChromIndex];
            var position = int.Parse(cols[VcfCommon.PosIndex]);
            var refAllele = cols[VcfCommon.RefIndex];
            var vcfAltAlleles = cols[VcfCommon.AltIndex];
            var vcfInfo = cols[VcfCommon.InfoIndex];

            // Unfortunately, for clinVar, the alternate allele field does not tell us which alt alleles are being reported in the clinvar database. For that we need to look into CLNALLE field: 
            // ##INFO=<ID=CLNALLE,Number=.,Type=Integer,Description="Variant alleles from REF or ALT columns.  0 is REF, 1 is the first ALT allele, etc.  This is used to match alleles with other corresponding clinical (CLN) INFO tags.  A value of -1 indicates that no allele was found to match a corresponding HGVS allele name.">
            var altAlleles = GetClinVarAltAlleles(refAllele + ',' + vcfAltAlleles, vcfInfo);

            var clinVarItems = new List<ClinVarItem>();

            if (altAlleles == null)
            {
                // if there are not alt alleles, we create a blank clinvar item
                clinVarItems.Add(new ClinVarItem(chromosome, position, refAllele, null, 0, vcfInfo));
            }
            else
            {
                AddInitialItems(altAlleles, clinVarItems, chromosome, position, refAllele, vcfInfo);
            }

            // each initial clinvar item may produce multiple items
            var noClinVarItems = clinVarItems.Count;
            for (int i = 0; i < noClinVarItems; i++)
            {
                // in some cases, for a particular allele, there might be multiple studies and associated clinvar accession numbers. These are seperated by pipes
                clinVarItems.AddRange(CreateAtomicItems(clinVarItems[i]));
            }

            // removing the initial items
            clinVarItems.RemoveRange(0, noClinVarItems);
            return clinVarItems;
        }

        private void AddInitialItems(string altAlleles, List<ClinVarItem> clinVarItems, string chromosome, int position, string refAllele,
            string vcfInfo)
        {
            int alleleIndex = 0;
            foreach (var altAllele in altAlleles.Split(','))
            {
                clinVarItems.Add(new ClinVarItem(chromosome, position, refAllele, altAllele, alleleIndex, vcfInfo));
                alleleIndex++;
            }
        }

        private IEnumerable<ClinVarItem> CreateAtomicItems(ClinVarItem clinVarItem)
        {
            var clinVarList = new List<ClinVarItem>();
            if (!clinVarItem.ID.Contains("|"))
            {
                clinVarItem.SetDiseaseDbIds(clinVarItem.DiseaseDbIds, clinVarItem.DiseaseDbNames);

                var additionalInfo = GetAdditionalInfo(clinVarItem.ID);
                clinVarList.Add(new ClinVarItem(
                    clinVarItem.Chromosome,
                    clinVarItem.Start,
                    clinVarItem.AlleleOrigin,
                    clinVarItem.AltAllele,
                    clinVarItem.GeneReviewsID,
                    clinVarItem.ID,
                    clinVarItem.ReviewStatusString,
                    clinVarItem.MedGenID,
                    clinVarItem.OmimID,
                    clinVarItem.OrphanetID,
                    clinVarItem.Phenotype,
                    clinVarItem.ReferenceAllele,
                    clinVarItem.Significance,
                    clinVarItem.SnoMedCtID,
                    additionalInfo?.PubmedIds,
                    additionalInfo?.LastEvaluatedDate ?? long.MinValue
                    ));
                return clinVarList;
            }

            var ids = clinVarItem.ID.Split('|');
            var reviewStatuses = clinVarItem.ReviewStatusString.Split('|');
            var significances = clinVarItem.Significance?.Split('|');
            var diseaseDbNames = clinVarItem.DiseaseDbNames?.Split('|');
            var diseaseDbIds = clinVarItem.DiseaseDbIds?.Split('|');
            var phenotypes = clinVarItem.Phenotype?.Split('|');


            for (int i = 0; i < ids.Length; i++)
            {
                var additionalInfo = GetAdditionalInfo(ids[i]);
                clinVarList.Add(new ClinVarItem(
                    clinVarItem.Chromosome,
                    clinVarItem.Start,
                    clinVarItem.AlleleOrigin,
                    clinVarItem.AltAllele,
                    null,
                    ids[i],
                    reviewStatuses[i],
                    null,
                    null,
                    null,
                    phenotypes != null && phenotypes.Length > i ? phenotypes[i] : null,
                    clinVarItem.ReferenceAllele,
                    significances != null && significances.Length > i ? significances[i] : null,
                    null,
                    additionalInfo?.PubmedIds,
                    additionalInfo?.LastEvaluatedDate ?? long.MinValue
                    ));

                if (diseaseDbIds != null && diseaseDbNames != null)
                {
                    if (diseaseDbIds.Length > i && diseaseDbNames.Length > i)
                        clinVarList[i].SetDiseaseDbIds(diseaseDbIds[i], diseaseDbNames[i]);
                }

            }

            return clinVarList;

        }

        private static string GetClinVarAltAlleles(string altAlleles, string infoField)
        {
            var alleles = altAlleles.Split(',');
            var sb = new StringBuilder();

            // extract CLNALLE field from info
            string[] stringSeparators = { "CLNALLE=" };

            var clnAlleleField = infoField.Split(stringSeparators, StringSplitOptions.None);
            if (clnAlleleField.Length < 2) return null;

            var clnAlleles = clnAlleleField[1].Split(new[] { ';' }, 2)[0].Split(',');

            foreach (var clnAllele in clnAlleles)
            {
                var clnAlleleIndex = Convert.ToInt16(clnAllele);
                if (clnAlleleIndex < 0)//unknown alleles are representated by -1 
                    sb.Append("N,");//we use a '' to denote an unknown value. We use it so that its not output in json  as a valid alt allele
                else
                    sb.Append(alleles[clnAlleleIndex] + ',');
            }

            if (sb.Length == 0) return null;
            return sb.ToString(0, sb.Length - 1);//discarding the last comma
        }

        /// <summary>
        /// Parses a ClinVar file and return an enumeration object containing all the ClinVar objects
        /// that have been extracted
        /// </summary>
        private IEnumerable<ClinVarItem> GetClinVarItems()
        {
            using (var reader = GZipUtilities.GetAppropriateStreamReader(_clinVarFileInfo.FullName))
            {
                while (true)
                {
                    // grab the next line
                    string line = reader.ReadLine();
                    if (line == null) break;

                    // skip comments and empty lines
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;


                    var clinVarItems = ExtractClinVarItems(line);
                    if (clinVarItems == null) continue;

                    foreach (var clinVarItem in clinVarItems)
                    {
                        yield return clinVarItem;
                    }

                }
            }
        }
    }
}
