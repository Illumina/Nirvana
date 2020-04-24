using System.Linq;
using Newtonsoft.Json;
using SAUtils.DataStructures;
using SAUtils.Omim;
using SAUtils.Omim.EntryApiResponse;
using Xunit;

namespace UnitTests.SAUtils.Omim
{
    public sealed class OmimUtilitiesTests
    {
        [Theory]
        [InlineData("In unstressed cells, p53 (not removed) is {kept} inactive essentially through the actions of the ubiquitin ligase MDM2 ({164785}) and a 28-kD beta subunits (ETFB; {130410}), which inhibits p53 transcriptional activity and ubiquitinates p53 to promote its degradation. Activity of p53 is ubiquitously lost in human cancer either by mutation of the p53 gene itself or by loss of cell signaling upstream or downstream of p53 ({305:Toledo and Wahl, 2006}; {30:Bourdon, 2007}; {324:Vousden and Lane, 2007}).", "In unstressed cells, p53 (not removed) is kept inactive essentially through the actions of the ubiquitin ligase MDM2 and a 28-kD beta subunits (ETFB), which inhibits p53 transcriptional activity and ubiquitinates p53 to promote its degradation. Activity of p53 is ubiquitously lost in human cancer either by mutation of the p53 gene itself or by loss of cell signaling upstream or downstream of p53 (Toledo and Wahl, 2006; Bourdon, 2007; Vousden and Lane, 2007).")]
        [InlineData("macules (summary by {2:Baas et al., 2013}).", "macules (summary by Baas et al., 2013).")]
        [InlineData("(MMR) ({18,17:Fishel et al., 1993, 1994}).", "(MMR) (Fishel et al., 1993, 1994).")]
        [InlineData("({516030}, {516040}, and {516050})", "")]
        [InlineData("(e.g., D1, {168461}; D2, {123833}; D3, {123834})", "(e.g., D1; D2; D3)")]
        [InlineData("(desmocollins; see DSC2, {125645})", "(desmocollins; see DSC2)")]
        [InlineData("(e.g., see {102700}, {300755})", "")]
        [InlineData("(ADH, see {103700}). See also liver mitochondrial ALDH2 ({100650})", "(ADH). See also liver mitochondrial ALDH2")]
        [InlineData("(see, e.g., CACNA1A; {601011})", "(see, e.g., CACNA1A)")]
        [InlineData("(e.g., GSTA1; {138359}), mu (e.g., {138350})", "(e.g., GSTA1), mu")]
        [InlineData("(NFKB; see {164011})", "(NFKB)")]
        [InlineData("(see ISGF3G, {147574})", "(see ISGF3G)")]
        [InlineData("(DCK; {EC 2.7.1.74}; {125450})", "(DCK; EC 2.7.1.74)")]
        [InlineData("chromosome 13q21 (see {603680.0001} and {613289.0001}).", "chromosome 13q21.")]
        [InlineData("common genetic haptoglobin types, Hp1 ({140100.0001}), Hp2 ({140100.0002}), and the heterozygous phenotype Hp2-1.", "common genetic haptoglobin types, Hp1, Hp2, and the heterozygous phenotype Hp2-1.")]
        [InlineData("and RBBP7/4 ({300825}/{602923}).", "and RBBP7/4.")]
        [InlineData("ultimately to formation of fibrin ({134570}/{134580}).", "ultimately to formation of fibrin.")]
        public void RemoveLinks_AsExpected(string input, string output)
        {
            Assert.Equal(output, input.RemoveLinks());
        }
        
        [Theory]
        [InlineData("<Subhead> UGT1A Gene Complex", " UGT1A Gene Complex")]
        public void RemoveFormatControl_AsExpected(string input, string output)
        {
            Assert.Equal(output, input.RemoveFormatControl());
        }

        [Theory]
        [InlineData("[Beta-glycopyranoside tasting], (3) {Alcohol dependence, susceptibility to}", "[Beta-glycopyranoside tasting], {Alcohol dependence, susceptibility to}", "2,3")]
        [InlineData("?Proteasome-associated autoinflammatory syndrome 3, digenic", "?Proteasome-associated autoinflammatory syndrome 3, digenic", "1")]
        [InlineData("{?Thyroid cancer, nonmedullary, 5}", "{?Thyroid cancer, nonmedullary, 5}", "3,1")]
        [InlineData("Methylmalonic aciduria, mut(0) type", "Methylmalonic aciduria, mut(0) type", "0")]
        [InlineData("?{Diabetes, susceptibility to},", "?{Diabetes, susceptibility to}", "1,3")]
        public void ExtractPhenotypeAndComments_AsExpected(string input, string expectedPhenotype, string commentsEnumString)
        {
            (string phenotype, var comments) = OmimUtilities.ExtractPhenotypeAndComments(input);

            var expectedComments = commentsEnumString.Split(',').Select(x => (OmimItem.Comment) byte.Parse(x)).Where(x => x != OmimItem.Comment.unknown).ToArray();
            
            Assert.Equal(expectedPhenotype, phenotype);
            Assert.Equal(expectedComments, comments);
        }


        [Fact]
        public void ExtractAndProcessItemDescription_AsExpected()
        {
            const string textSectionJson = "{\"textSection\":{\"textSectionName\": \"description\",\"textSectionTitle\": \"Description\",\"textSectionContent\": \"Constitutional mismatch repair deficiency is a rare childhood cancer predisposition syndrome with 4 main tumor types: hematologic malignancies, brain/central nervous system tumors, colorectal tumors and multiple intestinal polyps, and other malignancies including embryonic tumors and rhabdomyosarcoma. Many patients show signs reminiscent of neurofibromatosis type I (NF1; {162200}), particularly multiple cafe-au-lait macules (summary by {2:Baas et al., 2013}).\n\n'Turcot syndrome' classically refers to the combination of colorectal polyposis and primary tumors of the central nervous system ({13:Hamilton et al., 1995}).\"}}";
            var textSection = JsonConvert.DeserializeObject<TextSection>(textSectionJson);
            var entryItem = new EntryItem{textSectionList = new []{textSection}};
            var description = OmimUtilities.ExtractAndProcessItemDescription(entryItem);

            const string expected = "Constitutional mismatch repair deficiency is a rare childhood cancer predisposition syndrome with 4 main tumor types: hematologic malignancies, brain/central nervous system tumors, colorectal tumors and multiple intestinal polyps, and other malignancies including embryonic tumors and rhabdomyosarcoma. Many patients show signs reminiscent of neurofibromatosis type I (NF1), particularly multiple cafe-au-lait macules (summary by Baas et al., 2013).\n\n'Turcot syndrome' classically refers to the combination of colorectal polyposis and primary tumors of the central nervous system (Hamilton et al., 1995).";

            Assert.Equal(expected, description);
        }
    }
}