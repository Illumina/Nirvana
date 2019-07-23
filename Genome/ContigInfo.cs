using System.Collections.Generic;
using ErrorHandling.Exceptions;

namespace Genome
{
    public static class ContigInfo
    {
        private static readonly (string, int)[] ChromLengthsGrch37 = {
            ("1", 249250621),
            ("2", 243199373),
            ("3", 198022430),
            ("4", 191154276),
            ("5", 180915260),
            ("6", 171115067),
            ("7", 159138663),
            ("8", 146364022),
            ("9", 141213431),
            ("10", 135534747),
            ("11", 135006516),
            ("12", 133851895),
            ("13", 115169878),
            ("14", 107349540),
            ("15", 102531392),
            ("16", 90354753),
            ("17", 81195210),
            ("18", 78077248),
            ("19", 59128983),
            ("20", 63025520),
            ("21", 48129895),
            ("22", 51304566),
            ("X", 155270560),
            ("Y", 59373566)
        };

        private static readonly (string, int)[] ChromLengthsGrch38 =
        {
            ("1", 248956422),
            ("2", 242193529),
            ("3", 198295559),
            ("4", 190214555),
            ("5", 181538259),
            ("6", 170805979),
            ("7", 159345973),
            ("8", 145138636),
            ("9", 138394717),
            ("10", 133797422),
            ("11", 135086622),
            ("12", 133275309),
            ("13", 114364328),
            ("14", 107043718),
            ("15", 101991189),
            ("16", 90338345),
            ("17", 83257441),
            ("18", 80373285),
            ("19", 58617616),
            ("20", 64444167),
            ("21", 46709983),
            ("22", 50818468),
            ("X", 156040895),
            ("Y", 57227415)
        };

        private static readonly Dictionary<string, Dictionary<int, GenomeAssembly>> ChromLengthToAssembly = GetChromLengthToAssembly();

        private static Dictionary<string, Dictionary<int, GenomeAssembly>> GetChromLengthToAssembly()
        {
            var chromLengthToAssembly = new Dictionary<string, Dictionary<int, GenomeAssembly>>();
            foreach ((string chrom, int length) in ChromLengthsGrch37)
            {
                chromLengthToAssembly[chrom] = new Dictionary<int, GenomeAssembly> { { length, GenomeAssembly.GRCh37 } };
            }
            foreach ((string contig, int length) in ChromLengthsGrch38)
            {
                chromLengthToAssembly[contig][length] = GenomeAssembly.GRCh38;
            }
            chromLengthToAssembly["MT"] = new Dictionary<int, GenomeAssembly> { { 16569, GenomeAssembly.rCRS } };

            return chromLengthToAssembly;
        }

        public static GenomeAssembly GetGenomeAssembly(IChromosome chromosome, int length)
        {
            if (!ChromLengthToAssembly.TryGetValue(chromosome.EnsemblName, out var lengthToAssembly))
                return GenomeAssembly.Unknown;

            if (lengthToAssembly.TryGetValue(length, out GenomeAssembly assembly)) return assembly;

            if (chromosome.EnsemblName == "MT") return GenomeAssembly.Unknown;

            throw new UserErrorException($"Invalid length provided in VCF header: chromosome {chromosome.EnsemblName}, length {length}");

        }
    }
}
