using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommonUtilities;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Providers;

namespace VariantAnnotation.IO.VcfWriter
{
    public sealed class LiteVcfWriter : IDisposable
    {
        private readonly StreamWriter _writer;

        private const string AnnotatorTag                   = "##annotator=";
        private const string AnnotationServiceUriTag        = "##annotationserviceuri=";
        private const string AnnotationCollectionVersionTag = "##annotationcollectionversion=";
        private const string CsqInfoTag                     = "##INFO=<ID=CSQ,";
        private const string CsqRInfoTag                    = "##INFO=<ID=CSQR,";
        private const string CsqTInfoTag                    = "##INFO=<ID=CSQT,";
        private const string InfoTag                        = "##INFO=";

        private const string CsqtHeaderLine = "##INFO=<ID=CSQT,Number=.,Type=String,Description=\"Consequence type as predicted by Nirvana. Format: GenotypeIndex|HGNC|TranscriptID|Consequence\">";
        private const string CsqrHeaderLine = "##INFO=<ID=CSQR,Number=.,Type=String,Description=\"Predicted regulatory consequence type. Format: GenotypeIndex|RegulatoryID|Consequence\">";

        private const string InfoHeaderLines =
            "##INFO=<ID=AF1000G,Number=A,Type=Float,Description=\"The allele frequency from all populations of 1000 genomes data\">\n" +
            "##INFO=<ID=AA,Number=1,Type=String,Description=\"The inferred allele ancestral (if determined) to the chimpanzee/human lineage.\">\n" +
            "##INFO=<ID=GMAF,Number=A,Type=String,Description=\"Global minor allele frequency (GMAF); technically, the frequency of the second most frequent allele.  Format: GlobalMinorAllele|AlleleFreqGlobalMinor\">\n" +
            "##INFO=<ID=cosmic,Number=.,Type=String,Description=\"The numeric identifier for the variant in the Catalogue of Somatic Mutations in Cancer (COSMIC) database. Format: GenotypeIndex|Significance\">\n" +
            "##INFO=<ID=clinvar,Number=.,Type=String,Description=\"Clinical significance. Format: GenotypeIndex|Significance\">\n" +
            "##INFO=<ID=EVS,Number=A,Type=String,Description=\"Allele frequency, coverage and sample count taken from the Exome Variant Server (EVS). Format: AlleleFreqEVS|EVSCoverage|EVSSamples.\">\n" +
            "##INFO=<ID=RefMinor,Number=0,Type=Flag,Description=\"Denotes positions where the reference base is a minor allele and is annotated as though it were a variant\">\n" +
            "##INFO=<ID=phyloP,Number=A,Type=Float,Description=\"PhyloP conservation score. Denotes how conserved the reference sequence is between species throughout evolution\">";

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
     
        public LiteVcfWriter(StreamWriter vcfWriter, IEnumerable<string> headerLines,string nirvanaVersion, string nirvanaDataVersion, IEnumerable<IDataSourceVersion> dataSourceVersions)
        {
            // open the vcf file
            _writer = vcfWriter;
            _writer.NewLine = "\n";

            // write out the header lines                
            WriteHeader(headerLines, BuildVcfHeaderLines(nirvanaVersion, nirvanaDataVersion, dataSourceVersions));
        }

        private static string BuildVcfHeaderLines(string nirvanaVersion,string nirvanaDataVersion, IEnumerable<IDataSourceVersion> dataSourceVersions)
        {
            var sb = StringBuilderCache.Acquire();

            var nirvanaAnnotatorTag = "##annotator="+nirvanaVersion +'\n';

            sb.Append(nirvanaAnnotatorTag);
            // add the data version
            sb.Append("##annotatorDataVersion=" + nirvanaDataVersion + '\n');

            // only certain data sources are output to vcf. We will maintain a white list of those
            var dataSourceWhiteList = new HashSet<string> { "dbSNP", "COSMIC", "1000 Genomes Project", "EVS", "ExAC", "ClinVar", "phyloP" };

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

            return StringBuilderCache.GetStringAndRelease(sb);
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
            var lastIndex = currentHeaderLines.FindLastIndex(x => x.StartsWith(InfoTag));
            if (lastIndex == -1)
            {
                var lastChromIndex = currentHeaderLines.FindLastIndex(x => x.StartsWith(VcfCommon.ChromosomeHeader));
                lastIndex = lastChromIndex == -1 ? currentHeaderLines.Count - 1 : lastChromIndex - 1;
            }

            // write the modified header lines
            for (var currentIndex = 0; currentIndex < currentHeaderLines.Count; currentIndex++)
            {
                var line = currentHeaderLines[currentIndex];
                _writer.WriteLine(line);

                if (currentIndex != lastIndex) continue;

                _writer.WriteLine(csqInfoTag);
            }
        }

        public void Write(string s) => _writer.WriteLine(s);
    }
}