using Cache.Data;
using Genome;
using Moq;
using UnitTests.TestDataStructures;
using UnitTests.TestUtilities;
using VariantAnnotation.AnnotatedPositions;
using Variants;
using Xunit;

namespace UnitTests.VariantAnnotation.AnnotatedPositions
{
    public sealed class HgvsCodingNomenclatureTests
    {
        private readonly Transcript _forwardTranscript;
        private readonly Transcript _reverseTranscript;
        private readonly Transcript _gapTranscript;

        public HgvsCodingNomenclatureTests()
        {
            _forwardTranscript = GetForwardTranscript();
            _reverseTranscript = GetReverseTranscript();
            _gapTranscript     = GetGapTranscript();
        }

        internal static Transcript GetForwardTranscript()
        {
            // get info from ENST00000343938.4 
            var regions = new TranscriptRegion[]
            {
                new(1260147, 1260482, 1, 336, TranscriptRegionType.Exon, 1, null),
                new(1260483, 1262215, 336, 337, TranscriptRegionType.Intron, 1, null),
                new(1262216, 1262412, 337, 533, TranscriptRegionType.Exon, 2, null),
                new(1262413, 1262620, 533, 534, TranscriptRegionType.Intron, 2, null),
                new(1262621, 1264277, 534, 2190, TranscriptRegionType.Exon, 3, null)
            };

            var codingRegion = new CodingRegion(1262291, 1263143, 412, 1056, "ENSP00000343890.4",
                "MDDSETGFNLKVVLVSFKQCLDEKEEVLLDPYIASWKGLVRFLNSLGTIFSFISKDVVSKLRIMERLRGGPQSEHYRSLQAMVAHELSNRLVDLERRSHHPESGCRTVLRLHRALHWLQLFLEGLRTSPEDARTSALCADSYNASLAAYHPWVVRRAVTVAFCTLPTREVFLEAMNVGPPEQAVQMLGEALPFIQRVYNVSQKLYAEHSLLDLP",
                0, 0, 0, null, null);

            var gene = new Gene("80772", "ENSG00000224051", false, null) {Symbol = "CPTP"};

            return new Transcript(ChromosomeUtilities.Chr1, 1260147, 1264277, "ENST00000343938.4", BioType.mRNA, false,
                Source.Ensembl, gene, regions,
                "GAGGGCGGGGCGAGGGCGGGGCGGTGGGCGGGGACGGGGCCCGCACGGCGGCTACGGCCTAGGTGAGCGGCTCGGACTCGGCGGCCGCACCTGCCCAACCCAACCCGCACGGTCCGGAAGTCGCCGAGGGGCCGGGAGCGGGAGGGGACGTCGTCCTAGAGGGCCGGAGCGGGCGGGCGGCCGAGGACCCGGCTCCCGCGCAGGACGGAGCCGTGGCTCAGGTCGGCCCCTCCCCAACACCACCCCGGGCCTCCGCCCCTTCCTGGGCCTCTCGGTGGAGCAGGGACCCGAACCGGTGCCCATCCAGTCCGGTGCCATCTGAAGCCCCCTTCCCAGAAAATGAGCCACAGAGCAAGCTGACCCCAGCGACACAGCCCCCCAGCCCTACTGTATTTCCGTTCCTATCAAAAAATGGATGACTCGGAGACAGGTTTCAATCTGAAAGTCGTCCTGGTCAGTTTCAAGCAGTGTCTCGATGAGAAGGAAGAGGTCTTGCTGGACCCCTACATTGCCAGCTGGAAGGGCCTGGTCAGGTTTCTGAACAGCCTGGGCACCATCTTCTCATTCATCTCCAAGGACGTGGTCTCCAAGCTGCGGATCATGGAGCGCCTCAGGGGCGGCCCGCAGAGCGAGCACTACCGCAGCCTGCAGGCCATGGTGGCCCACGAGCTGAGCAACCGGCTGGTGGACCTGGAGCGCCGCTCCCACCACCCGGAGTCTGGCTGCCGGACGGTGCTGCGCCTGCACCGCGCCCTGCACTGGCTGCAGCTGTTCCTGGAGGGCCTGCGTACCAGCCCCGAGGACGCACGCACCTCCGCGCTCTGCGCCGACTCCTACAACGCCTCGCTGGCCGCCTACCACCCCTGGGTCGTGCGCCGCGCCGTCACCGTGGCCTTCTGCACGCTGCCCACACGCGAGGTCTTCCTGGAGGCCATGAACGTGGGGCCCCCGGAGCAGGCCGTGCAGATGCTAGGCGAGGCCCTCCCCTTCATCCAGCGTGTCTACAACGTCTCCCAGAAGCTCTACGCCGAGCACTCCCTGCTGGACCTGCCCTAGGGGCGGGAAGCCAGGGCCGCACCGGCTTTCCTGCTGCAGATCTGGGCTGCGGTGGCCAGGGCCGTGAGTCCCGTGGCAGAGCCTTCTGGGCGCTGCGGGAACAGGAGATCCTCTGTCGCCCCTGTGAGCTGAGCTGGTTAGGAACCACAGACTGTGACAGAGAAGGTGGCGACCAGCCCAGAAGAGGCCCACCCTCTCGGTCCGGAACAAGACGCCTCGGCCACGGCTCCCCCTCGGCCTATTACACGCGTGCGCAGCCAGGCCTCGCCAGGGTGCGGTGCAGAGCAGAGCAGGCAGGGGTGGGGGCCGGGCCTGCAAGAGCCCGAAAGGTCGCCACCCCCTAGCCTGTGGGGTGCATCTGCGAACCAGGGTGAAGTCACAGGTCCCGGGGTGTGGAGGCTCCATCCTTTCTCCTTTCTGCCAGCCGATGTGTCCTCATCTCAGGCCCGTGCCTGGGACCCCGTGTCTGCCCAGGTGGGCAGCCTTGAGCCCAGGGGACTCAGTGCCCTCCATGCCCTGGCTGGCAGAAACCCTCAACAGCAGTCTGGGCACTGTGGGGCTCTCCCCGCCTCTCCTGCCTTGTTTGCCCCTCAGCGTGCCAGGCAGACTGGGGGCAGGACAGCCGGAAGCTGAGACCAAGGCTCCTCACAGAAGGGCCCAGGAAGTCCCCGCCCTTGGGACAGCCTCCTCCGTAGCCCCTGCACGGCACCAGTTCCCCGAGGGACGCAGCAGGCCGCCTCCCGCAGCGGCCGTGGGTCTGCACAGCCCAGCCCAGCCCAAGGCCCCCAGGAGCTGGGACTCTGCTACACCCAGTGAAATGCTGTGTCCCTTCTCCCCCGTGCCCCTTGATGCCCCCTCCCCACAGTGCTCAGGAGACCCGTGGGGCACGGAACAGGAGGGTCTGGACCCTGTGGCCCAGCCAAAGGCTACCAGACAGCCACAACCAGCCCAGCCACCATCCAGTGCCTGGGGCCTGGCCACTGGCTCTTCACAGTGGACCCCAGCACCTCGGGGTGGCAGAGGGACGGCCCCCACGGCCCAGCAGACATGCGAGCTTCCAGAGTGCAATCTATGTGATGTCTTCCAACGTTAATAAATCACACAGCCTCCCAGGAGGGAGACGCTGGGGTGCAC",
                codingRegion);
        }

        private static Transcript GetForwardTranscriptWithoutUtr()
        {
            // ENST00000579622.1  chrX:70361035-70361156, non-coding, forward strand, no utr (GRCh37)
            var transcriptRegions = new TranscriptRegion[]
            {
                new(70361035, 70361156, 1, 122, TranscriptRegionType.Exon, 1, null)
            };

            var gene = new Gene(string.Empty, "ENSG00000265597", false, null) {Symbol = "AL590764.1"};

            return new Transcript(ChromosomeUtilities.ChrX, 70361035, 70361156, "ENST00000579622.1", BioType.miRNA,
                true, Source.Ensembl, gene, transcriptRegions,
                "CTTCCTCCCTCTGCTCCTTCTGAAGTATCTTTTGTGTTCTTATAGCAGCAGCAGCAACAGCAACAGCAGCAGCAGCAGCAGCAGCAACAGCAACAGCAGCAGCAGCAACAGCAACAACAGCA",
                null);
        }

        internal static Transcript GetReverseTranscript()
        {
            // get info from ENST00000423372.3 (GRCh37)
            var regions = new TranscriptRegion[]
            {
                new(134901, 135802, 1760, 2661, TranscriptRegionType.Exon, 2, null),
                new(135803, 137620, 1759, 1760, TranscriptRegionType.Intron, 1, null),
                new(137621, 139379, 1, 1759, TranscriptRegionType.Exon, 1, null)
            };

            var codingRegion = new CodingRegion(138530, 139309, 71, 850, "ENSP00000473460.1",
                "TSLWTPQAKLPTFQQLLHTQLLPPSGLFRPSSCFTRAFPGPTFVSWQPSLARFLPVSQQPRQAQVLPHTGLSTSSLCLTVASPRPTPVPGHHLRAQNLLKSDSLVPTAASWWPMKAQNLLKLTCPGPAPASCQRLQAQPLPHGGFSRPTSSSWLGLQAQLLPHNSLFWPSSCPANGGQCRPKTSSSQTLQAHLLLPGGINRPSFDLRTASAGPALASQGLFPGPALASWQLPQAKFLPACQQPQQAQLLPHSGPFRPNL*",
                0, 0, 0, null, null);

            var gene = new Gene(string.Empty, "ENSG00000237683", true, null) {Symbol = "AL627309.1"};

            return new Transcript(ChromosomeUtilities.Chr1, 134901, 139379, "ENST00000423372.3", BioType.mRNA, true,
                Source.Ensembl, gene, regions,
                "GCCTCTGCCTCCCGTCAGCCTCTACAGTCCCAACGTCTGCCTCACAGCAGATTCTTCACGCCCAGCTTCTACCTCACTGTGGACCCCCCAAGCCAAGCTCCCAACCTTTCAGCAGCTTCTACACACCCAGCTCCTGCCACCCAGTGGCCTCTTTAGGCCAAGCTCATGCTTCACAAGGGCCTTTCCAGGCCCAACTTTTGTCTCATGGCAACCTTCCCTGGCCAGATTCCTGCCTGTCTCCCAGCAGCCTAGACAGGCCCAGGTCTTGCCTCACACTGGCCTCTCTACATCCAGCTTATGCCTCACGGTGGCCTCTCCACGGCCAACTCCTGTCCCAGGACATCATCTCCGGGCCCAAAACTTACTCAAGTCAGACTCTCTAGTCCCAACTGCTGCCTCCTGGTGGCCTATGAAGGCCCAAAATCTCCTCAAGTTGACCTGTCCAGGCCCAGCTCCTGCCTCCTGTCAGCGTCTACAGGCCCAACCTCTGCCTCATGGGGGCTTCTCCAGGCCCACCTCTTCCTCTTGGCTGGGTCTACAGGCACAACTGCTGCCTCACAACAGCCTTTTTTGGCCCAGTTCCTGTCCAGCTAATGGCGGCCAATGTAGGCCCAAAACTTCCTCAAGTCAAACTCTCCAGGCCCACCTTCTGCTTCCCGGTGGCATCAACAGGCCCAGCTTTGACTTGAGAACAGCCTCTGCAGGCCCTGCTCTTGCCTCCCAGGGGCTTTTTCCAGGCCCAGCTCTTGCCTCATGGCAGCTGCCCCAGGCCAAATTTCTGCCTGCCTGCCAGCAGCCTCAACAGGCACAGCTCCTCCCTCACAGTGGCCCATTTAGGCCCAACTTATGACTGTGAGGCCATTTCCAGGCCTAGTGCCTGCCTCGTGGCTGACTCTTGAAGCCCAAAACTTCCTCAAATCAGGCTTTTGCCCAACTTCTGTCTACTGTCGGACTCTACAGGTCAGCCTCTGCCTCACAGTGGACCCTCCAGACCCAGATGGTGTCTCACTGTGGCATCCTCAGGTGAAGCTCCTGCCTTTCGGCAGCCTCTCCAGGCCCAGCTCCTCCTGCCTCCCAGTGGCCTCTTTCGGCCCAGCCCAGCTCATGCCTCCCGGCGGCCTTCCCAAGCCCCGCTTTTGACTTTCGGTGGCCTCTGCAGGCCTCGACAAGGCCCAGCCTCCTGCCTCCCGAAGGCCTGCACAGGCCCAGCCTCTGCCTCACAGCGGACTCTCCACGCCCAGCTAGCTGTTGCTTCACTGCGGCCTCCCGAGTCCAAAGCTCCTGCCTCTCGGCCGCTTCGGCAGGCCCAGCTCCCGCCTGCCAGTGGCCTCTTCAGGCCCATGGGGCTCATTCCTGACAACGGCCTTTCCAGGCCCAGTTTTTCCCTTCCAGCGGCCTCTCCGGGCCCAGAACCTCCTCAAGTCGGCCTCTCCAGACCCACTTGCACCCTCCGGGCGTTCTCTCCGGGCCCAGCTCTTCTTCCTGGTTGGGTCTCCAGGCCCGATTCCTGCCTCTCAACAACCTCTTTGGACTCAGTGCCTACCCATCTCCTGGCGGCCTTGGTCGGTCCACAGCTTCCTCAAGCCAAGCTCCCCAGGCCCAGGTCAGGCCTCACGGTGGCCTCTCCAGGATGAGCTCCTGCCCTCCGATGGCATCTCCAGGCCCCAAATGGTCTCCGGTCGGTGGGCTCCTCCACGCCAAGGTTGGGCCTCCCGGCGACCGCCGCAGGCCCAAGTTGTCCTGAAGTCGGGCTCTCCCGGCTGCATCTCCAGGCCGGACTCTGGCCCGACTCCAGGTCCCAACAACGTCTTTGGACTCAGCTCCTGCCCAGCTCCCAGCGGCCCTGGTAGGCCCACAACTTCCCTAAGCCAAGCTCCCCAGGCCCAGCTCAGGCCTCGCGGTGGCCTCTCCAGGCTCAGCTCCTGGCCCTCCGATGACATCTGCAGGCCCCAAATGGCCTCCGGTCGGTGGGCTCCTCTAGGCCCAGCTTGGGCCTCCCGGCGGCCTCCGCAGGCCCAAATCGTCCCGAAGTCAGTCTCTCCAGGCTTAGCTCCAGCCTCCCGGCGGCCTCTGCAGGCCCAAGTCGTCCTCAAGTCGGCCTGGAAGTGGGCCTGGAAGAGCAGCAAGTCGGCCTCCCTGGGCCCAGCTCCGTCCTCTCGACGGCCTCTCCAGGTGCAAAACTTCCTCGAGTCAGCCTCTCCAGGCCCAGCTCCTCCTGCCTCCCAGTGGCCTCTTTCGGCCCAGCCCAGCTCATGGCTCTCGGCGGCCTTCCCAGGCCCCGCTTTTGACTTTTGGCAGCCTCTTCAGGCGCAGAACTTGATCTCCAGTCGGCCTTTGCAGGCCCGGCCTCCTGCGTCTCGAAGGCCTGCACGGGCCCAGCCTCGGCCTCGGCCTCACAGCGGACTCTCCACGCCCAGCTAGCTCTCGCCTCACTGCGGCCTCCCCAGTCCAAAGCTCCTGCCTTTCGGCCACTTCGGCAGGTCCAGCTCCTGCCTGCCAGTGGCCTCTTTAGGCCCAGCTCATTCCTCACGTCGGCCATTCCAGGCCCTGTTTTTCCCTTCCGGCAGCCTCTTGGCCTCTAATTTGTTTATCTTTTGTGTATAAATCCCAAAATATTGAATTTTGGAATATTTCCACCATTATGTAAATGTTTTGGTAGGT",
                codingRegion);
        }

        private static Transcript GetGapTranscript()
        {
            var cigarOps = new CigarOp[]
            {
                new(CigarType.Match, 666),
                new(CigarType.Deletion, 1),
                new(CigarType.Match, 444)
            };
            
            //NM_000314.4
            var regions = new TranscriptRegion[]
            {
                new(89623195, 89624305, 1, 1110, TranscriptRegionType.Exon, 1, cigarOps),
                new(89624306, 89653781, 1110, 1111, TranscriptRegionType.Intron, 1, null),
                new(89653782, 89653866, 1111, 1195, TranscriptRegionType.Exon, 2, null),
                new(89653867, 89685269, 1195, 1196, TranscriptRegionType.Intron, 2, null),
                new(89685270, 89685314, 1196, 1240, TranscriptRegionType.Exon, 3, null),
                new(89685315, 89690802, 1240, 1241, TranscriptRegionType.Intron, 3, null),
                new(89690803, 89690846, 1241, 1284, TranscriptRegionType.Exon, 4, null),
                new(89690847, 89692769, 1284, 1285, TranscriptRegionType.Intron, 4, null),
                new(89692770, 89693008, 1285, 1523, TranscriptRegionType.Exon, 5, null),
                new(89693009, 89711874, 1523, 1524, TranscriptRegionType.Intron, 5, null),
                new(89711875, 89712016, 1524, 1665, TranscriptRegionType.Exon, 6, null),
                new(89712017, 89717609, 1665, 1666, TranscriptRegionType.Intron, 6, null),
                new(89717610, 89717776, 1666, 1832, TranscriptRegionType.Exon, 7, null),
                new(89717777, 89720650, 1832, 1833, TranscriptRegionType.Intron, 7, null),
                new(89720651, 89720875, 1833, 2057, TranscriptRegionType.Exon, 8, null),
                new(89720876, 89725043, 2057, 2058, TranscriptRegionType.Intron, 8, null),
                new(89725044, 89728532, 2058, 5546, TranscriptRegionType.Exon, 9, null)
            };

            var codingRegion = new CodingRegion(89624227, 89725229, 1032, 2243, "NP_000305.3",
                "MTAIIKEIVSRNKRRYQEDGFDLDLTYIYPNIIAMGFPAERLEGVYRNNIDDVVRFLDSKHKNHYKIYNLCAERHYDTAKFNCRVAQYPFEDHNPPQLELIKPFCEDLDQWLSEDDNHVAAIHCKAGKGRTGVMICAYLLHRGKFLKAQEALDFYGEVRTRDKKGVTIPSQRRYVYYYSYLLKNHLDYRPVALLFHKMMFETIPMFSGGTCNPQFVVCQLKVKIYSSNSGPTRREDKFMYFEFPQPLPVCGDIKVEFFHKQNKMLKKDKMFHFWVNTFFIPGPEETSEKVENGSLCDQEIDSICSIERADNDKEYLVLTLTKNDLDKANKDKANRYFSPNFKVKLYFTKTVEEPSNPEASSSTSVTPDVSDNEPDHYRYSDTTDSDPENEPFDEDQHTQITKV",
                0, 0, 0, null, null);

            var gene = new Gene("5728", "ENSG00000171862", false, 9588) {Symbol = "PTEN"};

            return new Transcript(ChromosomeUtilities.Chr10, 89623195, 89728532, "NM_000314.4", BioType.mRNA, false,
                Source.RefSeq, gene, regions,
                "CCTCCCCTCGCCCGGCGCGGTCCCGTCCGCCTCTCGCTCGCCTCCCGCCTCCCCTCGGTCTTCCGAGGCGCCCGGGCTCCCGGCGCGGCGGCGGAGGGGGCGGGCAGGCCGGCGGGCGGTGATGTGGCGGGACTCTTTATGCGCTGCGGCAGGATACGCGCTCGGCGCTGGGACGCGACTGCGCTCAGTTCTCTCCTCTCGGAAGCTGCAGCCATGATGGAAGTTTGAGAGTTGAGCCGCTGTGAGGCGAGGCCGGGCTCAGGCGAGGGAGATGAGAGACGGCGGCGGCCGCGGCCCGGAGCCCCTCTCAGCGCCTGTGAGCAGCCGCGGGGGCAGCGCCCTCGGGGAGCCGGCCGGCCTGCGGCGGCGGCAGCGGCGGCGTTTCTCGCCTCCTCTTCGTCTTTTCTAACCGTGCAGCCTCTTCCTCGGCTTCTCCTGAAAGGGAAGGTGGAAGCCGTGGGCTCGGGCGGGAGCCGGCTGAGGCGCGGCGGCGGCGGCGGCACCTCCCGCTCCTGGAGCGGGGGGGAGAAGCGGCGGCGGCGGCGGCCGCGGCGGCTGCAGCTCCAGGGAGGGGGTCTGAGTCGCCTGTCACCATTTCCAGGGCTGGGAACGCCGGAGAGTTGGTCTCTCCCCTTCTACTGCCTCCAACACGGCGGCGGCGGCGGCGGCACATCCAGGGACCCGGGCCGGTTTTAAACCTCCCGTCCGCCGCCGCCGCACCCCCCGTGGCCCGGGCTCCGGAGGCCGCCGGCGGAGGCAGCCGTTCGGAGGATTATTCGTCTTCTCCCCATTCCGCTGCCGCCGCTGCCAGGCCTCTGGCTGCTGAGGAGAAGCAGGCCCAGTCGCTGCAACCATCCAGCAGCCGCCGCAGCAGCCATTACCCGGCTGCGGTCCAGAGCCAAGCGGCGGCAGAGCGAGGGGCATCAGCTACCGCCAAGTCCAGAGCCATTTCCATCCTGCAGAAGAAGCCCCGCCACCAGCAGCTTCTGCCATCTCTCTCCTCCTTTTTCTTCAGCCACAGGCTCCCAGACATGACAGCCATCATCAAAGAGATCGTTAGCAGAAACAAAAGGAGATATCAAGAGGATGGATTCGACTTAGACTTGACCTATATTTATCCAAACATTATTGCTATGGGATTTCCTGCAGAAAGACTTGAAGGCGTATACAGGAACAATATTGATGATGTAGTAAGGTTTTTGGATTCAAAGCATAAAAACCATTACAAGATATACAATCTTTGTGCTGAAAGACATTATGACACCGCCAAATTTAATTGCAGAGTTGCACAATATCCTTTTGAAGACCATAACCCACCACAGCTAGAACTTATCAAACCCTTTTGTGAAGATCTTGACCAATGGCTAAGTGAAGATGACAATCATGTTGCAGCAATTCACTGTAAAGCTGGAAAGGGACGAACTGGTGTAATGATATGTGCATATTTATTACATCGGGGCAAATTTTTAAAGGCACAAGAGGCCCTAGATTTCTATGGGGAAGTAAGGACCAGAGACAAAAAGGGAGTAACTATTCCCAGTCAGAGGCGCTATGTGTATTATTATAGCTACCTGTTAAAGAATCATCTGGATTATAGACCAGTGGCACTGTTGTTTCACAAGATGATGTTTGAAACTATTCCAATGTTCAGTGGCGGAACTTGCAATCCTCAGTTTGTGGTCTGCCAGCTAAAGGTGAAGATATATTCCTCCAATTCAGGACCCACACGACGGGAAGACAAGTTCATGTACTTTGAGTTCCCTCAGCCGTTACCTGTGTGTGGTGATATCAAAGTAGAGTTCTTCCACAAACAGAACAAGATGCTAAAAAAGGACAAAATGTTTCACTTTTGGGTAAATACATTCTTCATACCAGGACCAGAGGAAACCTCAGAAAAAGTAGAAAATGGAAGTCTATGTGATCAAGAAATCGATAGCATTTGCAGTATAGAGCGTGCAGATAATGACAAGGAATATCTAGTACTTACTTTAACAAAAAATGATCTTGACAAAGCAAATAAAGACAAAGCCAACCGATACTTTTCTCCAAATTTTAAGGTGAAGCTGTACTTCACAAAAACAGTAGAGGAGCCGTCAAATCCAGAGGCTAGCAGTTCAACTTCTGTAACACCAGATGTTAGTGACAATGAACCTGATCATTATAGATATTCTGACACCACTGACTCTGATCCAGAGAATGAACCTTTTGATGAAGATCAGCATACACAAATTACAAAAGTCTGAATTTTTTTTTATCAAGAGGGATAAAACACCATGAAAATAAACTTGAATAAACTGAAAATGGACCTTTTTTTTTTTAATGGCAATAGGACATTGTGTCAGATTACCAGTTATAGGAACAATTCTCTTTTCCTGACCAATCTTGTTTTACCCTATACATCCACAGGGTTTTGACACTTGTTGTCCAGTTGAAAAAAGGTTGTGTAGCTGTGTCATGTATATACCTTTTTGTGTCAAAAGGACATTTAAAATTCAATTAGGATTAATAAAGATGGCACTTTCCCGTTTTATTCCAGTTTTATAAAAAGTGGAGACAGACTGATGTGTATACGTAGGAATTTTTTCCTTTTGTGTTCTGTCACCAACTGAAGTGGCTAAAGAGCTTTGTGATATACTGGTTCACATCCTACCCCTTTGCACTTGTGGCAACAGATAAGTTTGCAGTTGGCTAAGAGAGGTTTCCGAAGGGTTTTGCTACATTCTAATGCATGTATTCGGGTTAGGGGAATGGAGGGAATGCTCAGAAAGGAAATAATTTTATGCTGGACTCTGGACCATATACCATCTCCAGCTATTTACACACACCTTTCTTTAGCATGCTACAGTTATTAATCTGGACATTCGAGGAATTGGCCGCTGTCACTGCTTGTTGTTTGCGCATTTTTTTTTAAAGCATATTGGTGCTAGAAAAGGCAGCTAAAGGAAGTGAATCTGTATTGGGGTACAGGAATGAACCTTCTGCAACATCTTAAGATCCACAAATGAAGGGATATAAAAATAATGTCATAGGTAAGAAACACAGCAACAATGACTTAACCATATAAATGTGGAGGCTATCAACAAAGAATGGGCTTGAAACATTATAAAAATTGACAATGATTTATTAAATATGTTTTCTCAATTGTAACGACTTCTCCATCTCCTGTGTAATCAAGGCCAGTGCTAAAATTCAGATGCTGTTAGTACCTACATCAGTCAACAACTTACACTTATTTTACTAGTTTTCAATCATAATACCTGCTGTGGATGCTTCATGTGCTGCCTGCAAGCTTCTTTTTTCTCATTAAATATAAAATATTTTGTAATGCTGCACAGAAATTTTCAATTTGAGATTCTACAGTAAGCGTTTTTTTTCTTTGAAGATTTATGATGCACTTATTCAATAGCTGTCAGCCGTTCCACCCTTTTGACCTTACACATTCTATTACAATGAATTTTGCAGTTTTGCACATTTTTTAAATGTCATTAACTGTTAGGGAATTTTACTTGAATACTGAATACATATAATGTTTATATTAAAAAGGACATTTGTGTTAAAAAGGAAATTAGAGTTGCAGTAAACTTTCAATGCTGCACACAAAAAAAAGACATTTGATTTTTCAGTAGAAATTGTCCTACATGTGCTTTATTGATTTGCTATTGAAAGAATAGGGTTTTTTTTTTTTTTTTTTTTTTTTTTTTTAAATGTGCAGTGTTGAATCATTTCTTCATAGTGCTCCCCCGAGTTGGGACTAGGGCTTCAATTTCACTTCTTAAAAAAAATCATCATATATTTGATATGCCCAGACTGCATACGATTTTAAGCGGAGTACAACTACTATTGTAAAGCTAATGTGAAGATATTATTAAAAAGGTTTTTTTTTCCAGAAATTTGGTGTCTTCAAATTATACCTTCACCTTGACATTTGAATATCCAGCCATTTTGTTTCTTAATGGTATAAAATTCCATTTTCAATAACTTATTGGTGCTGAAATTGTTCACTAGCTGTGGTCTGACCTAGTTAATTTACAAATACAGATTGAATAGGACCTACTAGAGCAGCATTTATAGAGTTTGATGGCAAATAGATTAGGCAGAACTTCATCTAAAATATTCTTAGTAAATAATGTTGACACGTTTTCCATACCTTGTCAGTTTCATTCAACAATTTTTAAATTTTTAACAAAGCTCTTAGGATTTACACATTTATATTTAAACATTGATATATAGAGTATTGATTGATTGCTCATAAGTTAAATTGGTAAAGTTAGAGACAACTATTCTAACACCTCACCATTGAAATTTATATGCCACCTTGTCTTTCATAAAAGCTGAAAATTGTTACCTAAAATGAAAATCAACTTCATGTTTTGAAGATAGTTATAAATATTGTTCTTTGTTACAATTTCGGGCACCGCATATTAAAACGTAACTTTATTGTTCCAATATGTAACATGGAGGGCCAGGTCATAAATAATGACATTATAATGGGCTTTTGCACTGTTATTATTTTTCCTTTGGAATGTGAAGGTCTGAATGAGGGTTTTGATTTTGAATGTTTCAATGTTTTTGAGAAGCCTTGCTTACATTTTATGGTGTAGTCATTGGAAATGGAAAAATGGCATTATATATATTATATATATAAATATATATTATACATACTCTCCTTACTTTATTTCAGTTACCATCCCCATAGAATTTGACAAGAATTGCTATGACTGAAAGGTTTTCGAGTCCTAATTAAAACTTTATTTATGGCAGTATTCATAATTAGCCTGAAATGCATTCTGTAGGTAATCTCTGAGTTTCTGGAATATTTTCTTAGACTTTTTGGATGTGCAGCAGCTTACATGTCTGAAGTTACTTGAAGGCATCACTTTTAAGAAAGCTTACAGTTGGGCCCTGTACCATCCCAAGTCCTTTGTAGCTCCTCTTGAACATGTTTGCCATACTTTTAAAAGGGTAGTTGAATAAATAGCATCACCATTCTTTGCTGTGGCACAGGTTATAAACTTAAGTGGAGTTTACCGGCAGCATCAAATGTTTCAGCTTTAAAAAATAAAAGTAGGGTACAAGTTTAATGTTTAGTTCTAGAAATTTTGTGCAATATGTTCATAACGATGGCTGTGGTTGCCACAAAGTGCCTCGTTTACCTTTAAATACTGTTAATGTGTCATGCATGCAGATGGAAGGGGTGGAACTGTGCACTAAAGTGGGGGCTTTAACTGTAGTATTTGGCAGAGTTGCCTTCTACCTGCCAGTTCAAAAGTTCAACCTGTTTTCATATAGAATATATATACTAAAAAATTTCAGTCTGTTAAACAGCCTTACTCTGATTCAGCCTCTTCAGATACTCTTGTGCTGTGCAGCAGTGGCTCTGTGTGTAAATGCTATGCACTGAGGATACACAAAAATACCAATATGATGTGTACAGGATAATGCCTCATCCCAATCAGATGTCCATTTGTTATTGTGTTTGTTAACAACCCTTTATCTCTTAGTGTTATAAACTCCACTTAAAACTGATTAAAGTCTCATTCTTGTCAAAAAAAAAAAAAAAAAAAAAAAAAAA",
                codingRegion);
        }

        [Theory]
        [InlineData(89_623_859, 89_623_859, "G", "C", "G", VariantType.SNV, "NM_000314.4:c.-367G>C")]
        [InlineData(89_623_860, 89_623_860, "C", "T", "C", VariantType.SNV, "NM_000314.4:c.-366C>T")]
        // [InlineData(89_623_861, 89_623_861, "T", "A", "", VariantType.SNV, "NM_000314.4:c.-366_-365insA")] // TODO: NIR-915
        // [InlineData(89_623_861, 89_623_861, "T", "C", "", VariantType.SNV, "NM_000314.4:c.-366dup")]       // TODO: NIR-915
        [InlineData(89_623_862, 89_623_862, "G", "C", "G", VariantType.SNV, "NM_000314.4:c.-365G>C")]
        [InlineData(89_623_863, 89_623_863, "G", "C", "G", VariantType.SNV, "NM_000314.4:c.-364G>C")]
        [InlineData(89_623_860, 89_623_862, "CTG", "", "CG", VariantType.deletion, "NM_000314.4:c.-366_-365del")]
        [InlineData(89_623_860, 89_623_861, "CT", "", "C", VariantType.deletion, "NM_000314.4:c.-366del")]
        // [InlineData(89_623_861, 89_623_862, "TG", "", "G", VariantType.deletion, "NM_000314.4:c.-365del")] // TODO: NIR-916
        [InlineData(89_623_861, 89_623_860, "", "C", "G", VariantType.insertion, "NM_000314.4:c.-366dup")]
        // [InlineData(89_623_862, 89_623_861, "", "C", "G", VariantType.insertion, "NM_000314.4:c.-366dup")] // TODO: NIR-917
        public void GetHgvscAnnotation_PTEN_AroundDeletionRnaEdit(int variantStart, int variantEnd, string reference,
            string alt, string transcriptRef, VariantType variantType, string expected)
        {
            (int startIndex, _) = MappedPositionUtilities.FindRegion(_gapTranscript.TranscriptRegions, variantStart);
            (int endIndex, _)   = MappedPositionUtilities.FindRegion(_gapTranscript.TranscriptRegions, variantEnd);

            var variant = new SimpleVariant(ChromosomeUtilities.Chr10, variantStart, variantEnd, reference, alt,
                variantType);

            var simpleSequence = new SimpleSequence("GCTGG", 89_623_858);

            string actual = HgvsCodingNomenclature.GetHgvscAnnotation(_gapTranscript, variant, simpleSequence, startIndex,
                endIndex, transcriptRef, null);

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(89624304, 89624308, "CTGTA", "", "CT", VariantType.deletion, "NM_000314.4:c.78_79+3del")]
        [InlineData(89624308, 89624310, "ATC", "", "ATC", VariantType.deletion, "NM_000314.4:c.79+3_79+5del")]
        public void GetHgvscAnnotation_PTEN_SET362(int variantStart, int variantEnd, string reference,
            string alt, string transcriptRef, VariantType variantType, string expected)
        {
            (int startIndex, _) = MappedPositionUtilities.FindRegion(_gapTranscript.TranscriptRegions, variantStart);
            (int endIndex, _)   = MappedPositionUtilities.FindRegion(_gapTranscript.TranscriptRegions, variantEnd);

            var variant = new SimpleVariant(ChromosomeUtilities.Chr10, variantStart, variantEnd, reference, alt,
                variantType);

            string actual = HgvsCodingNomenclature.GetHgvscAnnotation(_gapTranscript, variant, null, startIndex,
                endIndex, transcriptRef, null);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetHgvscAnnotation_substitution_in_3UTR()
        {
            var variant = new SimpleVariant(ChromosomeUtilities.Chr1, 1260247, 1260247, "A", "G", VariantType.SNV);
            var observedHgvsc =
                HgvsCodingNomenclature.GetHgvscAnnotation(_forwardTranscript, variant, null, 0, 0, null, null);

            Assert.Equal("ENST00000343938.4:c.-311A>G", observedHgvsc);
        }

        [Fact]
        public void GetHgvscAnnotation_substitution_in_intron_before_TSS()
        {
            var variant = new SimpleVariant(ChromosomeUtilities.Chr1, 1262210, 1262210, "C", "G", VariantType.SNV);
            var observedHgvsc =
                HgvsCodingNomenclature.GetHgvscAnnotation(_forwardTranscript, variant, null, 1, 1, null, null);

            Assert.Equal("ENST00000343938.4:c.-75-6C>G", observedHgvsc);
        }

        [Fact]
        public void GetHgvscAnnotation_insertion_in_coding_region()
        {
            var sequence = new Mock<ISequence>();
            sequence.Setup(x => x.Substring(1262627, 1)).Returns("A");

            var variant = new SimpleVariant(ChromosomeUtilities.Chr1, 1262629, 1262628, "", "G", VariantType.insertion);
            var observedHgvsc =
                HgvsCodingNomenclature.GetHgvscAnnotation(_forwardTranscript, variant, sequence.Object, 4, 4, null,
                    null);

            Assert.Equal("ENST00000343938.4:c.130_131insG", observedHgvsc);
        }

        [Fact]
        public void GetHgvscAnnotation_insertion_after_coding_region()
        {
            var sequence = new Mock<ISequence>();
            sequence.Setup(x => x.Substring(1262627, 1)).Returns("A");

            var variant = new SimpleVariant(ChromosomeUtilities.Chr1, 1263159, 1263158, "", "G", VariantType.insertion);
            var observedHgvsc =
                HgvsCodingNomenclature.GetHgvscAnnotation(_forwardTranscript, variant, sequence.Object, 4, 4, null,
                    null);

            Assert.Equal("ENST00000343938.4:c.*15_*16insG", observedHgvsc);
        }

        [Fact]
        public void GetHgvscAnnotation_duplication_in_coding_region()
        {
            var sequence = new Mock<ISequence>();
            sequence.Setup(x => x.Substring(1262626, 2)).Returns("TA");

            var variant = new SimpleVariant(ChromosomeUtilities.Chr1, 1262629, 1262628, "", "TA",
                VariantType.insertion);
            var observedHgvsc =
                HgvsCodingNomenclature.GetHgvscAnnotation(_forwardTranscript, variant, sequence.Object, 4, 4, null,
                    null);

            Assert.Equal("ENST00000343938.4:c.129_130dup", observedHgvsc);
        }
        
        [Fact]
        public void GetHgvscAnnotation_PartialCdsBug_NeedCdsOffset()
        {
            var variant = new SimpleVariant(ChromosomeUtilities.Chr3, 72427537, 72427538, "CA", "",
                VariantType.deletion);

            string actual = HgvsCodingNomenclature.GetHgvscAnnotation(MockedData.Transcripts.NM_012234_6, variant, null,
                0, 0, "GA", "");

            Assert.Equal("NM_012234.6:c.685_686del", actual);
        }

        [Fact]
        public void ApplyDuplicationAdjustments_NonCoding_Reverse()
        {
            var regions = new TranscriptRegion[]
            {
                new(20976856, 20977050, 154, 348, TranscriptRegionType.Exon, 2, null),
                new(20977051, 20977054, 153, 154, TranscriptRegionType.Intron, 1, null),
                new(20977055, 20977207, 1, 153, TranscriptRegionType.Exon, 1, null)
            };


            var observedResults = regions.ShiftDuplication(20977006, "AACT", true);

            Assert.Equal("AACT",   observedResults.RefAllele);
            Assert.Equal(20977009, observedResults.Start);
            Assert.Equal(20977006, observedResults.End);
        }

        [Fact]
        public void ApplyDuplicationAdjustments_Coding_Forward()
        {
            var regions = new TranscriptRegion[41];
            for (int i = 0; i < 22; i++)
                regions[i] = new TranscriptRegion(107000000, 107334926, 1, 1564, TranscriptRegionType.Exon, 0, null);
            for (int i = 23; i < regions.Length; i++)
                regions[i] = new TranscriptRegion(107335162, 108000000, 1662, 1700, TranscriptRegionType.Exon, 0, null);
            regions[21] = new TranscriptRegion(107334926, 107335065, 1565, 1566, TranscriptRegionType.Intron, 11, null);
            regions[22] = new TranscriptRegion(107335066, 107335161, 1566, 1661, TranscriptRegionType.Exon, 12, null);

            var observedResults = regions.ShiftDuplication(107335068, "AGTC", false);

            Assert.Equal("AGTC",    observedResults.RefAllele);
            Assert.Equal(107335064, observedResults.Start);
            Assert.Equal(107335067, observedResults.End);
        }

        [Fact]
        public void GetHgvscAnnotation_Deletion_StartBeforeTranscript_ReturnNull()
        {
            var variant = new SimpleVariant(ChromosomeUtilities.Chr1, 1260144, 1260148, "ATGTC", "",
                VariantType.deletion);
            string observedHgvsc =
                HgvsCodingNomenclature.GetHgvscAnnotation(_forwardTranscript, variant, null, -1, 0, null, null);

            Assert.Null(observedHgvsc);
        }
        
        [Fact]
        public void GetHgvscAnnotation_Deletion_EndAfterTranscript_ReturnNull()
        {
            var variant = new SimpleVariant(ChromosomeUtilities.Chr1, 1260143, 1260148, "ATGTC", "",
                VariantType.deletion);
            string observedHgvsc =
                HgvsCodingNomenclature.GetHgvscAnnotation(_forwardTranscript, variant, null, -1, 0, null, null);

            Assert.Null(observedHgvsc);
        }

        [Fact]
        public void GetHgvscAnnotation_Delins_start_from_Exon_end_in_intron()
        {
            var variant = new SimpleVariant(ChromosomeUtilities.Chr1, 1262410, 1262414, "ATGTC", "TG",
                VariantType.indel);
            var observedHgvsc =
                HgvsCodingNomenclature.GetHgvscAnnotation(_forwardTranscript, variant, null, 2, 3, null, null);

            Assert.Equal("ENST00000343938.4:c.120_122+2delinsTG", observedHgvsc);
        }

        [Fact]
        public void GetHgvscAnnotation_inversion_start_from_Exon_end_in_intron()
        {
            var variant = new SimpleVariant(ChromosomeUtilities.Chr1, 1262410, 1262414, "ATGTC", "GACAT",
                VariantType.MNV);
            var observedHgvsc =
                HgvsCodingNomenclature.GetHgvscAnnotation(_forwardTranscript, variant, null, 2, 3, null, null);

            Assert.Equal("ENST00000343938.4:c.120_122+2inv", observedHgvsc);
        }

        [Fact]
        public void GetHgvscAnnotation_Deletion_end_after_transcript()
        {
            var variant = new SimpleVariant(ChromosomeUtilities.Chr1, 1260143, 1260148, "ATGTC", "",
                VariantType.deletion);
            var observedHgvsc =
                HgvsCodingNomenclature.GetHgvscAnnotation(_forwardTranscript, variant, null, -1, 0, null, null);

            Assert.Null(observedHgvsc);
        }

        [Fact]
        public void GetHgvscAnnotation_Reference_no_hgvsc()
        {
            var variant = new SimpleVariant(ChromosomeUtilities.Chr1, 1260138, 1260138, "A", "A",
                VariantType.reference);
            var observedHgvsc =
                HgvsCodingNomenclature.GetHgvscAnnotation(_forwardTranscript, variant, null, -1, -1, null, null);

            Assert.Null(observedHgvsc);
        }

        [Fact]
        public void GetHgvscAnnotation_substitution_in_intron_of_reverse_gene()
        {
            var variant = new SimpleVariant(ChromosomeUtilities.Chr1, 136000, 136000, "A", "G", VariantType.SNV);
            var observedHgvsc =
                HgvsCodingNomenclature.GetHgvscAnnotation(_reverseTranscript, variant, null, 1, 1, null, null);

            Assert.Equal("ENST00000423372.3:c.*910-198T>C", observedHgvsc);
        }

        [Fact]
        public void GetHgvscAnnotation_substitution_after_stopCodon_of_reverse_gene()
        {
            var variant = new SimpleVariant(ChromosomeUtilities.Chr1, 138529, 138529, "A", "G", VariantType.SNV);
            var observedHgvsc =
                HgvsCodingNomenclature.GetHgvscAnnotation(_reverseTranscript, variant, null, 2, -1, null, null);

            Assert.Equal("ENST00000423372.3:c.*1T>C", observedHgvsc);
        }

        [Fact]
        public void GetHgvscAnnotation_deletion_of_reverse_gene()
        {
            var variant = new SimpleVariant(ChromosomeUtilities.Chr1, 135802, 137619, "ATCGTGGGTTGT", "",
                VariantType.deletion);
            var observedHgvsc =
                HgvsCodingNomenclature.GetHgvscAnnotation(_reverseTranscript, variant, null, 0, 1, "ACAACCCACGAT",
                    null);

            Assert.Equal("ENST00000423372.3:c.*909+2_*910del", observedHgvsc);
        }

        [Fact]
        public void GetHgvscAnnotation_insertion_at_last_position()
        {
            var sequence = new Mock<ISequence>();
            sequence.Setup(x => x.Substring(70361157 - 12, 12)).Returns("TATATATATATA");

            var variant = new SimpleVariant(ChromosomeUtilities.ChrX, 70361157, 70361156, "", "ACACCAGCAGCA",
                VariantType.insertion); //right shifted variant
            var observedHgvsc =
                HgvsCodingNomenclature.GetHgvscAnnotation(GetForwardTranscriptWithoutUtr(), variant, sequence.Object, 0,
                    0, null, null);

            Assert.Equal("ENST00000579622.1:n.122_123insACACCAGCAGCA", observedHgvsc);
        }

        [Fact]
        public void GetHgvscAnnotation_duplication_at_last_position()
        {
            var sequence = new Mock<ISequence>();
            sequence.Setup(x => x.Substring(70361156 - 4, 4)).Returns("ACAC");

            var variant = new SimpleVariant(ChromosomeUtilities.ChrX, 70361157, 70361156, "", "ACAC",
                VariantType.insertion); //right shifted variant
            var observedHgvsc =
                HgvsCodingNomenclature.GetHgvscAnnotation(GetForwardTranscriptWithoutUtr(), variant, sequence.Object, 0,
                    0, null, null);

            Assert.Equal("ENST00000579622.1:n.119_122dup", observedHgvsc);
        }
    }
}