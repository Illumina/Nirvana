using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.IO.VcfWriter;
using VariantAnnotation.Providers;
using Xunit;

namespace UnitTests.VariantAnnotation.IO.VcfWriter
{
    public sealed class LiteVcfWriterTests
    {
        private const string CsqtHeaderLine =
                "##INFO=<ID=CSQT,Number=.,Type=String,Description=\"Consequence type as predicted by Nirvana. Format: GenotypeIndex|HGNC|TranscriptID|Consequence\">"
            ;

        private const string CsqrHeaderLine =
                "##INFO=<ID=CSQR,Number=.,Type=String,Description=\"Predicted regulatory consequence type. Format: GenotypeIndex|RegulatoryID|Consequence\">"
            ;

        private readonly List<string> _infoHeaderLines = new List<string>
        {
            "##INFO=<ID=AF1000G,Number=A,Type=Float,Description=\"The allele frequency from all populations of 1000 genomes data\">",
            "##INFO=<ID=AA,Number=1,Type=String,Description=\"The inferred allele ancestral (if determined) to the chimpanzee/human lineage.\">",
            "##INFO=<ID=GMAF,Number=A,Type=String,Description=\"Global minor allele frequency (GMAF); technically, the frequency of the second most frequent allele.  Format: GlobalMinorAllele|AlleleFreqGlobalMinor\">",
            "##INFO=<ID=cosmic,Number=.,Type=String,Description=\"The numeric identifier for the variant in the Catalogue of Somatic Mutations in Cancer (COSMIC) database. Format: GenotypeIndex|Significance\">",
            "##INFO=<ID=clinvar,Number=.,Type=String,Description=\"Clinical significance. Format: GenotypeIndex|Significance\">",
            "##INFO=<ID=EVS,Number=A,Type=String,Description=\"Allele frequency, coverage and sample count taken from the Exome Variant Server (EVS). Format: AlleleFreqEVS|EVSCoverage|EVSSamples.\">",
            "##INFO=<ID=RefMinor,Number=0,Type=Flag,Description=\"Denotes positions where the reference base is a minor allele and is annotated as though it were a variant\">",
            "##INFO=<ID=phyloP,Number=A,Type=Float,Description=\"PhyloP conservation score. Denotes how conserved the reference sequence is between species throughout evolution\">"
        };

        [Fact]
        public void Vcf_header_write_as_expected()
        {
            var ms     = new MemoryStream();
            var writer = new StreamWriter(ms, Encoding.Default, 1024, true);

            var currentHeaderLines = new List<string>
            {
                "##fileformat=VCFv4.1",
                "##FORMAT=<ID=GT,Number=1,Type=String,Description=\"Genotype\">",
                "##source=IsaacVariantCaller",
                "#CHROM  POS     ID      REF     ALT     QUAL    FILTER  INFO    FORMAT  Mother"
            };

            var dataSourceVersions = new IDataSourceVersion[]
            {
                new DataSourceVersion("VEP", "84", DateTime.Parse("2017/7/21").Ticks, "RefSeq"),
                new DataSourceVersion("1000 Genomes Project", "v5", DateTime.Parse("2017/7/21").Ticks),
                new DataSourceVersion("dbSNP", "72", DateTime.Parse("2017/8/15").Ticks),
                new DataSourceVersion("dummy", "2", DateTime.Parse("2017/9/15").Ticks) //should not showing in output
            };

            const string vcfLine = "1       10167   .       C       A       4       LowGQXHetSNP    SNVSB=0.0;SNVHPOL=3;CSQT=1|DDX11L1|ENST00000456328.2|upstream_gene_variant,1|WASH7P|ENST00000438504.2|downstream_gene_variant,1|DDX11L1|NR_046018.2|upstream_gene_variant,1|WASH7P|NR_024540.1|downstream_gene_variant;CSQR=1|ENSR00001576074|regulatory_region_variant,1|ENSR00001576074|regulatory_region_variant     GT:GQ:GQX:DP:DPF:AD     0/1:34:8:3:0:2,1";
            using (var vcfWriter = new LiteVcfWriter(writer,currentHeaderLines, "Illumina Annotation Engine 2.0.4", "84.21.41",dataSourceVersions))
            {
                vcfWriter.Write(vcfLine);
            }

            var expectedLines = new List<string>
            {
                "##fileformat=VCFv4.1",
                "##FORMAT=<ID=GT,Number=1,Type=String,Description=\"Genotype\">",
                "##source=IsaacVariantCaller",
                "##annotator=Illumina Annotation Engine 2.0.4",
                "##annotatorDataVersion=84.21.41",
                "##annotatorTranscriptSource=RefSeq",
                "##dataSource=1000 Genomes Project,version:v5,release date:2017-07-21",
                "##dataSource=dbSNP,version:72,release date:2017-08-15"
            };

            expectedLines.AddRange(_infoHeaderLines);
            expectedLines.Add(CsqtHeaderLine);
            expectedLines.Add(CsqrHeaderLine);
            expectedLines.Add("#CHROM  POS     ID      REF     ALT     QUAL    FILTER  INFO    FORMAT  Mother");
            expectedLines.Add(vcfLine);

            ms.Position = 0;
            using (var reader = new StreamReader(ms))
            {
                string line;
                int i = 0;
                while ((line = reader.ReadLine()) != null)
                {
                    Assert.Equal(expectedLines[i], line);
                    i++;
                }
                Assert.Equal(20, i);
            }
        }
    }
}