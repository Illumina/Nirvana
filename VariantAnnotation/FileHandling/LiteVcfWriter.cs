using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using VariantAnnotation.DataStructures;
using VariantAnnotation.DataStructures.JsonAnnotations;
using VariantAnnotation.Interface;
using VariantAnnotation.Utilities;

namespace VariantAnnotation.FileHandling
{
    public sealed class LiteVcfWriter : IDisposable
    {
        #region members

        private readonly StreamWriter _writer;

        private const string AnnotatorTag                   = "##annotator=";
        private const string AnnotationServiceUriTag        = "##annotationserviceuri=";
        private const string AnnotationCollectionVersionTag = "##annotationcollectionversion=";
        private const string CsqInfoTag                     = "##INFO=<ID=CSQ,";
        private const string CsqRInfoTag                    = "##INFO=<ID=CSQR,";
        private const string CsqTInfoTag                    = "##INFO=<ID=CSQT,";
        private const string InfoTag                        = "##INFO=";
        private readonly string _nirvanaAnnotatorTag;

        private readonly StringBuilder _sb;
        private const string CsqtHeaderLine = "##INFO=<ID=CSQT,Number=.,Type=String,Description=\"Consequence type as predicted by IAE. Format: GenotypeIndex|HGNC|Transcript ID|Consequence\">";
        private const string CsqrHeaderLine = "##INFO=<ID=CSQR,Number=.,Type=String,Description=\"Predicted regulatory consequence type. Format: GenotypeIndex|RegulatoryID|Consequence\">";

        private const string InfoHeaderLines =
            "##INFO=<ID=AF1000G,Number=A,Type=Float,Description=\"The allele frequency from all populations of 1000 genomes data\">\n" +
            "##INFO=<ID=AA,Number=A,Type=String,Description=\"The inferred allele ancestral (if determined) to the chimpanzee/human lineage.\">\n" +
            "##INFO=<ID=GMAF,Number=A,Type=String,Description=\"Global minor allele frequency (GMAF); technically, the frequency of the second most frequent allele.  Format: GlobalMinorAllele|AlleleFreqGlobalMinor\">\n" +
            "##INFO=<ID=cosmic,Number=.,Type=String,Description=\"The numeric identifier for the variant in the Catalogue of Somatic Mutations in Cancer (COSMIC) database. Format: GenotypeIndex|Significance\">\n" +
            "##INFO=<ID=clinvar,Number=.,Type=String,Description=\"Clinical significance. Format: GenotypeIndex|Significance\">\n" +
            "##INFO=<ID=EVS,Number=A,Type=String,Description=\"Allele frequency, coverage and sample count taken from the Exome Variant Server (EVS). Format: AlleleFreqEVS|EVSCoverage|EVSSamples.\">\n" +
            "##INFO=<ID=RefMinor,Number=0,Type=Flag,Description=\"Denotes positions where the reference base is a minor allele and is annotated as though it were a variant\">\n" +
            "##INFO=<ID=phyloP,Number=A,Type=Float,Description=\"PhyloP conservation score. Denotes how conserved the reference sequence is between species throughout evolution\">";

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
                    _writer.Dispose();
                }

                // Free any unmanaged objects here. 
                _isDisposed = true;
            }
        }

        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        public LiteVcfWriter(string vcfPath, IEnumerable<string> headerLines, string nirvanaDataVersion,
            IEnumerable<IDataSourceVersion> dataSourceVersions)
        {
            _nirvanaAnnotatorTag = "##annotator=Nirvana " + CommandLineUtilities.Version;
            _sb = new StringBuilder();

            // open the vcf file
            _writer = GZipUtilities.GetStreamWriter(vcfPath);
            _writer.NewLine = "\n";

            // write out the header lines                
            WriteHeader(headerLines, BuildVcfHeaderLines(nirvanaDataVersion, dataSourceVersions));
        }

        private static string BuildVcfHeaderLines(string nirvanaDataVersion, IEnumerable<IDataSourceVersion> dataSourceVersions)
        {
            var sb = new StringBuilder();

            // add the data version
            sb.Append("##annotatorDataVersion=" + nirvanaDataVersion + '\n');

			// only certain data sources are output to vcf. We will maintain a white list of those
			var dataSourceWhiteList = new HashSet<string>() { "dbSNP", "COSMIC", "1000 Genomes Project", "EVS", "ExAC", "ClinVar", "phyloP" };

            // add the data source versions
            if (dataSourceVersions != null)
            {
                foreach (var sourceVersion in dataSourceVersions)
                {
                    // add the transcript source
                    if (sourceVersion.Name == "VEP")
                    {
                        sb.Append("##annotatorTranscriptSource=" + sourceVersion.Description + '\n');
                        continue;
                    }
					
					if (dataSourceWhiteList.Contains(sourceVersion.Name)) sb.AppendFormat("##{0}\n", sourceVersion);
                }
            }

            // add the supplementary INFO tag descriptions
            sb.Append(InfoHeaderLines + '\n');

            // add the CSQT and CSQR header lines
            sb.Append(CsqtHeaderLine + '\n');
            sb.Append(CsqrHeaderLine);

            return sb.ToString();
        }

        /// <summary>
        /// writes the vcf header to the current output stream
        /// </summary>
        private void WriteHeader(IEnumerable<string> headerLines, string csqInfoTag)
        {
            // skip over some header lines that may already be present
            var currentHeaderLines =
                headerLines.Where(
                    line =>
                        !line.StartsWith(AnnotatorTag) &&
                        !line.StartsWith(AnnotationCollectionVersionTag) &&
                        !line.StartsWith(AnnotationServiceUriTag) &&
                        !line.StartsWith(CsqInfoTag) &&
                        !line.StartsWith(CsqRInfoTag) &&
                        !line.StartsWith(CsqTInfoTag)).ToList();

            // find where we should place our info field and annotator tags
            int lastIndex = currentHeaderLines.FindLastIndex(x => x.StartsWith(InfoTag));
            if (lastIndex == -1)
            {
                int lastChromIndex = currentHeaderLines.FindLastIndex(x => x.StartsWith(VcfCommon.ChromosomeHeader));
                lastIndex = lastChromIndex == -1 ? currentHeaderLines.Count - 1 : lastChromIndex - 1;
            }

            // write the modified header lines
            for (int currentIndex = 0; currentIndex < currentHeaderLines.Count; currentIndex++)
            {
                var line = currentHeaderLines[currentIndex];
                _writer.WriteLine(line);

                if (currentIndex != lastIndex) continue;

                _writer.WriteLine(_nirvanaAnnotatorTag);
                _writer.WriteLine(csqInfoTag);
            }
        }

        public void Write(IVariant vcfVariant, IAnnotatedVariant annotatedVariant)
        {
            _sb.Clear();

            var fields = vcfVariant.Fields;

            // add all of the fields before the info field
            for (int vcfIndex = 0; vcfIndex < VcfCommon.IdIndex; vcfIndex++)
            {
                _sb.Append(fields[vcfIndex]);
                _sb.Append('\t');
            }

            // ad dbSNP id
            var dbSnpId = ExtractDbId(annotatedVariant, fields[VcfCommon.IdIndex]);
            _sb.Append(dbSnpId);
            _sb.Append('\t');

            for (int vcfIndex = VcfCommon.IdIndex + 1; vcfIndex < VcfCommon.InfoIndex; vcfIndex++)
            {
                _sb.Append(fields[vcfIndex]);
                _sb.Append('\t');
            }

            string infoField = (annotatedVariant as UnifiedJson)?.InfoFromVcf;

            // write the information in annotatedvariant
            AddInfoField(infoField, annotatedVariant);

            // add all of the fields after the info field
            int numColumns = fields.Length;
            for (int vcfIndex = VcfCommon.InfoIndex + 1; vcfIndex < numColumns; vcfIndex++)
            {
                _sb.Append('\t');
                _sb.Append(fields[vcfIndex]);
            }

            _writer.WriteLine(_sb.ToString());
        }

        private static string ExtractDbId(IAnnotatedVariant annotatedVariant, string idField)
        {
            var dbSnp = new VcfField();

            var nonDbsnpIds = GetNonDbsnpIds(idField);
            if (nonDbsnpIds != null) foreach (var nonDbsnpId in nonDbsnpIds) dbSnp.Add(nonDbsnpId);

            foreach (var altAllele in annotatedVariant.AnnotatedAlternateAlleles)
            {
	            if (altAllele.DbSnpIds==null) continue;
	            foreach (var id in altAllele.DbSnpIds) dbSnp.Add(id);
            }

	        return dbSnp.GetString("");
        }

        private static IEnumerable<string> GetNonDbsnpIds(string idField)
        {
            if (idField == null || idField == ".") return null;
            var idList = idField.Split(';').Where(id => !id.StartsWith("rs")).ToList();

            return idList.Count == 0 ? null : idList;
        }

        private void AddInfoField(string infoField, IAnnotatedVariant annotatedVariant)
        {
            var infoEntries = new VcfField();

            if (!string.IsNullOrEmpty(infoField))
            {
                infoEntries.Add(infoField);
            }

            ExtractInfo(annotatedVariant as UnifiedJson, infoEntries);

            infoField = infoEntries.GetString("");

            // remove .
            if (infoField == ".") infoField = "";

            _sb.Append(infoField);

            var csqs = new List<CsqEntry>();

            ExtractCsqs(annotatedVariant as UnifiedJson, csqs);

            if (csqs.Count != 0)
                if (infoField.Length > 0) _sb.Append(";");

            // append CSQ tags using delegate from annotator
            if (csqs.Count == 0 && infoField.Length == 0)
            {
                _sb.Append(".");
            }
        }

        private static void ExtractCsqs(UnifiedJson unifiedJson, List<CsqEntry> csqs)
        {
            foreach (var jsonVariant in unifiedJson.JsonVariants)
            {
                csqs.AddRange(
                    jsonVariant.EnsemblTranscripts.Where(transcript => transcript.IsCanonical == "true")
                        .Select(transcript => new CsqEntry
                        {
                            Allele = jsonVariant.GenotypeIndex.ToString(),
                            Canonical = transcript.IsCanonical,
                            Feature = transcript.TranscriptID,
                            FeatureType = CsqCommon.TranscriptFeatureType,
                            Symbol = transcript.Hgnc,
                            Consequence = transcript.Consequence == null ? null : string.Join("&", transcript.Consequence)
                        }));

                csqs.AddRange(from transcript in jsonVariant.RefSeqTranscripts
                              where transcript.IsCanonical == "true"
                              select new CsqEntry
                              {
                                  Allele = jsonVariant.GenotypeIndex.ToString(),
                                  Canonical = transcript.IsCanonical,
                                  Feature = transcript.TranscriptID,
                                  FeatureType = CsqCommon.TranscriptFeatureType,
                                  Symbol = transcript.Hgnc,
                                  Consequence = transcript.Consequence == null ? null : string.Join("&", transcript.Consequence)
                              });

                csqs.AddRange(jsonVariant.RegulatoryRegions.Select(regulatoryRegion => new CsqEntry
                {
                    Allele = jsonVariant.GenotypeIndex.ToString(),
                    Consequence = string.Join("&", regulatoryRegion.Consequence),
                    Feature = regulatoryRegion.ID,
                    FeatureType = CsqCommon.RegulatoryFeatureType
                }));
            }
        }

        private static void ExtractInfo(UnifiedJson unifiedJson, VcfField infoEntries)
        {
            var alleleFreq1000G   = new VcfInfoKeyValue("AF1000G");
            var ancestralAllele   = new VcfInfoKeyValue("AA");
            var clinVar           = new VcfInfoKeyValue("clinvar");
            var cosmic            = new VcfInfoKeyValue("cosmic");
            var evs               = new VcfInfoKeyValue("EVS");
            var globalMinorAllele = new VcfInfoKeyValue("GMAF");
            var phyloP            = new VcfInfoKeyValue("phyloP");

            foreach (var jsonVariant in unifiedJson.JsonVariants)
            {
                if (jsonVariant.IsReferenceMinor) infoEntries.Add("RefMinor");

			    phyloP.Add(jsonVariant.PhylopScore);

                ancestralAllele.Add(jsonVariant.AncestralAllele);

                foreach (var cosmicEntry in jsonVariant.CosmicEntries)
                {
                    if (cosmicEntry.ID == null) continue;
                    if (cosmicEntry.SaAltAllele != jsonVariant.SaAltAllele) continue;

                    cosmic.Add(jsonVariant.GenotypeIndex.ToString(CultureInfo.InvariantCulture) + '|' + cosmicEntry.ID);
                }

                foreach (var clinVarEntry in jsonVariant.ClinVarEntries)
                {
                    if (clinVarEntry.Significance == null) continue;
                    if (clinVarEntry.SaAltAllele != jsonVariant.SaAltAllele) continue;

                    clinVar.Add(jsonVariant.GenotypeIndex.ToString(CultureInfo.InvariantCulture) + '|' + RemoveWhiteSpace(clinVarEntry.Significance));
                }

                if ((jsonVariant.GlobalMinorAllele != null) || (jsonVariant.GlobalMinorAlleleFrequency != null))
                {
                    globalMinorAllele.Add(jsonVariant.GlobalMinorAllele + '|' + jsonVariant.GlobalMinorAlleleFrequency);
                }
                else
                {
                    // for multi allelic variants, we need to add a . for the entries that do not have a Global minor allele.
                    globalMinorAllele.Add(null);
                }

                alleleFreq1000G.Add(jsonVariant.AlleleFrequencyAll);
                if (jsonVariant.EvsAlleleFrequencyAll != null || jsonVariant.EvsCoverage != null || jsonVariant.EvsSamples != null)
                {
                    evs.Add(jsonVariant.EvsAlleleFrequencyAll + '|' + jsonVariant.EvsCoverage + '|' + jsonVariant.EvsSamples);
                }
                else
                {
                    evs.Add(null);
                }
            }

            infoEntries.Add(ancestralAllele.GetString());
            infoEntries.Add(globalMinorAllele.GetString());
            infoEntries.Add(alleleFreq1000G.GetString());
            infoEntries.Add(evs.GetString());
            infoEntries.Add(phyloP.GetString());
            infoEntries.Add(cosmic.GetString());
            infoEntries.Add(clinVar.GetString());
        }

        private static string RemoveWhiteSpace(string s)
        {
            return s.Replace(' ', '_');
        }
    }
}
