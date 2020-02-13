using System.Collections.Generic;
using System.Linq;
using CacheUtils.TranscriptCache;
using Genome;
using SAUtils.InputFileParsers.ClinVar;
using SAUtils.MitoMap;
using UnitTests.TestDataStructures;
using UnitTests.TestUtilities;
using Xunit;

namespace UnitTests.SAUtils.MitoMap
{
    public sealed class MitoMapVariantReaderTests
    {
        private static readonly ISequence  Sequence    = new NSequence();
        private static readonly SimpleSequenceProvider SequenceProvider = new SimpleSequenceProvider(GenomeAssembly.GRCh37, Sequence, 
            ChromosomeUtilities.RefNameToChromosome);
        private static readonly VariantAligner VariantAligner = new VariantAligner(SequenceProvider?.Sequence);
        private static readonly MitoMapInputDb MitoMapInputDb = new MitoMapInputDb(
            new Dictionary<string, string> {{"7616", "17616"},{"3510", "13510"},{"90282","190282"},{"99016","199016"}});

        [Fact]
        public void GetAltAllelesTests()
        {
            const string altAlleleString1 = "ACT";
            const string altAlleleString2 = "ACT;AGT";
            const string altAlleleString3 = "AKY";
            const string altAlleleString4 = "ACT;AKY";
            const string altAlleleString5 = "CNT;AKY";
            Assert.Equal(new[] { "ACT" }, MitoMapVariantReader.GetAltAlleles(altAlleleString1));
            Assert.Equal(new[] { "ACT", "AGT" }, MitoMapVariantReader.GetAltAlleles(altAlleleString2));
            Assert.Equal(new[] { "AGC", "AGT", "ATC", "ATT" }, MitoMapVariantReader.GetAltAlleles(altAlleleString3));
            Assert.Equal(new[] { "ACT", "AGC", "AGT", "ATC", "ATT" }, MitoMapVariantReader.GetAltAlleles(altAlleleString4));
            Assert.Equal(new[] { "CNT", "AGC", "AGT", "ATC", "ATT" }, MitoMapVariantReader.GetAltAlleles(altAlleleString5));
        }

        [Theory]
        [InlineData("0 (0)", MitoMapDataTypes.MitoMapMutationsRNA, 0)]
        [InlineData("<a href='/cgi-bin/index_mitomap.cgi?title=RNA+Mutation+A+at+750&pos=750&ref=A&alt=A' target=_blank>858 (0)</a>\"", MitoMapDataTypes.MitoMapMutationsRNA, 858)]
        [InlineData("<a href='/cgi-bin/index_mitomap.cgi?title=Coding+Control+Mutation+T-C+at+16217&pos=16217&ref=T&alt=C' target=_blank>3657 (4688)</a>", MitoMapDataTypes.MitoMapMutationsCodingControl, 3657)]
        [InlineData("<a href='/cgi-bin/index_mitomap.cgi?title=Coding+Polymorphism+T-C+at+rCRS+position+650&pos=650&ref=T&alt=C&purge_type=' target='_blank'>36</a>", MitoMapDataTypes.MitoMapPolymorphismsCoding, 36)]
        [InlineData("0", MitoMapDataTypes.MitoMapPolymorphismsCoding, 0)]
        [InlineData("0", MitoMapDataTypes.MitoMapPolymorphismsControl, 0)]
        [InlineData("<a href='/cgi-bin/index_mitomap.cgi?title=Control+Polymorphism+T-C+at+rCRS+position+14&pos=14&ref=T&alt=C' target='_blank'>5 (3/2)</a>", MitoMapDataTypes.MitoMapPolymorphismsControl, 3)]
        [InlineData("<a href='/cgi-bin/index_mitomap.cgi?title=Control+Polymorphism+T-A+at+rCRS+position+14&pos=14&ref=T&alt=A' target='_blank'>38 (0/38)</a>", MitoMapDataTypes.MitoMapPolymorphismsControl, 0)]
        public void GetNumFullLengthSequences_AsExpected(string field, string dataType, int numFullLengthSequences)
        {
            Assert.Equal(numFullLengthSequences, MitoMapVariantReader.GetNumFullLengthSequences(field, dataType));
        }

        [Theory]
        [InlineData("[\"618\",\"<a href='/MITOMAP/GenomeLoci#MTTF'>MT-TF</a>\",\"Ptosis CPEO MM & EXIT\",\"T618G\",\"tRNA Phe\",\"-\",\"+\",\"Reported\",\"<span style='display:inline-block;white-space:nowrap;'><a href='/cgi-bin/mitotip?pos=618&alt=G&quart=1'><u>77.50%</u></a> <i class='fa fa-arrow-up' style='color:red' aria-hidden='true'></i><i class='fa fa-arrow-up' style='color:red' aria-hidden='true'></i></span>\",\"0.0%<br>(0.0%)\",\"0 (0)\",\"<a href='/cgi-bin/print_ref_list?refs=7616&title=RNA+Mutation+T618G' target='_blank'>1</a>\"],", 
            "MutationsRNA", "\"refAllele\":\"T\",\"altAllele\":\"G\",\"diseases\":[\"Ptosis CPEO MM & EXIT\"],\"hasHomoplasmy\":false,\"hasHeteroplasmy\":true,\"status\":\"Reported\",\"clinicalSignificance\":\"likely pathogenic\",\"scorePercentile\":77.50,\"numGenBankFullLengthSeqs\":0,\"pubMedIds\":[\"17616\"]")]
        [InlineData("[\"3308\",\"<a href='/MITOMAP/GenomeLoci#MTND1'>MT-ND1</a>\",\"Sudden Infant Death\",\"T3308G\",\"T-G\",\"M-Term\",\"+\",\"+\",\"Reported\",\"0.0%<br>(0.0%)\",\"<a href='/cgi-bin/index_mitomap.cgi?title=Coding+Control+Mutation+T-G+at+3308&pos=3308&ref=T&alt=G' target=_blank>6 (0)</a>\",\"<a href='/cgi-bin/print_ref_list?refs=3510&title=Mutation+T-G+at+3308' target='_blank'>1</a>\"],", 
             "MutationsCodingControl", "\"refAllele\":\"T\",\"altAllele\":\"G\",\"diseases\":[\"Sudden Infant Death\"],\"hasHomoplasmy\":true,\"hasHeteroplasmy\":true,\"status\":\"Reported\",\"numGenBankFullLengthSeqs\":6,\"pubMedIds\":[\"13510\"]")]
        [InlineData("[\"606\",\"<a href='/MITOMAP/GenomeLoci#MTTF'>MT-TF</a>\",\"A-G\",\"-\",\"-\",\"tRNA\",\"0.0%\",\"<a href='/cgi-bin/index_mitomap.cgi?title=Coding+Polymorphism+A-G+at+rCRS+position+606&pos=606&ref=A&alt=G&purge_type=' target='_blank'>15</a>\",\"<a href='/cgi-bin/print_ref_list?refs=90282,99016&title=Coding+Polymorphism+A-G+at+606' target='_blank'>2</a>\"],", 
             "PolymorphismsCoding", "\"refAllele\":\"A\",\"altAllele\":\"G\",\"numGenBankFullLengthSeqs\":15,\"pubMedIds\":[\"190282\",\"199016\"]")]
        public void ParseLine_AsExpected(string line, string fileName, string expectedJsonString)
        {
            string jsonString = MitoMapVariantReader.ParseLine(line, fileName, SequenceProvider, VariantAligner, ChromosomeUtilities.ChrM, MitoMapInputDb)
                                                    .FirstOrDefault()
                                                    ?.GetJsonString();
            Assert.Equal(expectedJsonString, jsonString);
        }
    }
}