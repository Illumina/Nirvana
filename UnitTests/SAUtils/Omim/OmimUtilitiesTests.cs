using SAUtils.Omim;
using Xunit;

namespace UnitTests.SAUtils.Omim
{
    public sealed class OmimUtilitiesTests
    {
        [Theory]
        [InlineData("In unstressed cells, p53 (not removed) is {kept} inactive essentially through the actions of the ubiquitin ligase MDM2 ({164785}) and a 28-kD beta subunits (ETFB; {130410}), which inhibits p53 transcriptional activity and ubiquitinates p53 to promote its degradation. Activity of p53 is ubiquitously lost in human cancer either by mutation of the p53 gene itself or by loss of cell signaling upstream or downstream of p53 ({305:Toledo and Wahl, 2006}; {30:Bourdon, 2007}; {324:Vousden and Lane, 2007}).", "In unstressed cells, p53 (not removed) is kept inactive essentially through the actions of the ubiquitin ligase MDM2 and a 28-kD beta subunits (ETFB), which inhibits p53 transcriptional activity and ubiquitinates p53 to promote its degradation. Activity of p53 is ubiquitously lost in human cancer either by mutation of the p53 gene itself or by loss of cell signaling upstream or downstream of p53 (Toledo and Wahl, 2006; Bourdon, 2007; Vousden and Lane, 2007).")]
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
        public void RemoveLinksInText_AsExpected(string input, string output)
        {
            Assert.Equal(output, OmimUtilities.RemoveLinksInText(input));
        }

        [Theory]
        [InlineData("[Beta-glycopyranoside tasting], (3) {Alcohol dependence, susceptibility to}", "Beta-glycopyranoside tasting, Alcohol dependence, susceptibility to")]
        [InlineData("?Proteasome-associated autoinflammatory syndrome 3, digenic", "Proteasome-associated autoinflammatory syndrome 3, digenic")]
        [InlineData("{?Thyroid cancer, nonmedullary, 5}", "Thyroid cancer, nonmedullary, 5")]
        [InlineData("Methylmalonic aciduria, mut(0) type", "Methylmalonic aciduria, mut(0) type")]
        [InlineData("{Diabetes, susceptibility to},", "Diabetes, susceptibility to")]
        public void ExtractPhenotypeAndComments_AsExpected(string input, string phenotype)
        {
            Assert.Equal(phenotype, OmimUtilities.ExtractPhenotypeAndComments(input).Phenotype);
        }
    }
}