using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using CommandLine.Builders;
using CommandLine.NDesk.Options;
using ErrorHandling;
using Genome;
using Intervals;
using IO;
using ReferenceSequence.Compression;
using ReferenceSequence.Creation;
using ReferenceSequence.IO;

namespace ReferenceSequence.Commands
{
    public static class CreateReferenceMain
    {
        private static string _fastaPrefix;
        private static string _genomeAssemblyReportPath;
        private static string _cytogeneticBandPath;
        private static string _referenceNamesPath;
        private static string _genomeAssembly;
        private static string _outputCompressedPath;
        private static byte _patchLevel;

        private static ExitCodes ProgramExecution()
        {
            var genomeAssembly = GenomeAssemblyHelper.Convert(_genomeAssembly);

            Console.Write("- loading previous reference names... ");
            List<Chromosome> oldChromosomes = ReferenceNamesReader.GetReferenceNames(FileUtilities.GetReadStream(_referenceNamesPath));
            Console.WriteLine("finished.");

            Dictionary<string, Chromosome> oldRefNameToChromosome = ReferenceDictionaryUtils.GetRefNameToChromosome(oldChromosomes);

            Console.Write("- reading the genome assembly report... ");
            List<Chromosome> chromosomes = AssemblyReader.GetChromosomes(FileUtilities.GetReadStream(_genomeAssemblyReportPath), oldRefNameToChromosome, oldChromosomes.Count);
            int numRefSeqs  = chromosomes.Count;
            Console.WriteLine($"{numRefSeqs} references found.");

            Console.Write("- checking reference index contiguity... ");
            CheckReferenceIndexContiguity(chromosomes, oldChromosomes);
            Console.WriteLine("contiguous.");

            Dictionary<string, Chromosome> refNameToChromosome = ReferenceDictionaryUtils.GetRefNameToChromosome(chromosomes);

            Console.Write("- reading cytogenetic bands... ");
            List<Band>[] cytogeneticBandsByRef = CytogeneticBandsReader.GetCytogeneticBands(FileUtilities.GetReadStream(_cytogeneticBandPath),
                    numRefSeqs, refNameToChromosome);
            Console.WriteLine("finished.");

            Console.WriteLine("- reading FASTA files:");
            List<FastaSequence> fastaSequences = GetFastaSequences(_fastaPrefix, refNameToChromosome);
            long genomeLength  = GetGenomeLength(fastaSequences);
            Console.WriteLine($"- genome length: {genomeLength:N0}");

            Console.Write("- check if chrY has PAR masking... ");
            CheckChrYPadding(fastaSequences);
            Console.WriteLine("unmasked.");

            Console.Write("- applying 2-bit compression... ");
            List<Creation.ReferenceSequence> referenceSequences = CreateReferenceSequences(fastaSequences, cytogeneticBandsByRef);
            Console.WriteLine("finished.");

            Console.Write("- creating reference sequence file... ");
            CreateReferenceSequenceFile(genomeAssembly, _patchLevel, chromosomes, referenceSequences);
            long fileSize = new FileInfo(_outputCompressedPath).Length;
            Console.WriteLine($"{fileSize:N0} bytes");

            return ExitCodes.Success;
        }

        private static long GetGenomeLength(IEnumerable<FastaSequence> fastaSequences) =>
            fastaSequences.Aggregate<FastaSequence, long>(0, (current, fastaSequence) => current + fastaSequence.Bases.Length);

        private static List<Creation.ReferenceSequence> CreateReferenceSequences(IEnumerable<FastaSequence> fastaSequences, IReadOnlyList<List<Band>> cytogeneticBandsByRef)
        {
            var referenceSequences = new List<Creation.ReferenceSequence>();

            foreach (var fastaSequence in fastaSequences)
            {
                Band[] cytogeneticBands = cytogeneticBandsByRef[fastaSequence.Chromosome.Index].ToArray();
                (byte[] buffer, Interval[] maskedEntries) = TwoBitCompressor.Compress(fastaSequence.Bases);
                var referenceSequence = new Creation.ReferenceSequence(buffer, maskedEntries,
                    cytogeneticBands, 0, fastaSequence.Bases.Length);
                referenceSequences.Add(referenceSequence);
            }

            return referenceSequences;
        }

        private static void CheckChrYPadding(IEnumerable<FastaSequence> fastaSequences)
        {
            FastaSequence chrY = fastaSequences.FirstOrDefault(s => s.Chromosome.UcscName == "chrY");

            if (chrY == null) return;

            int numN = CountNs(chrY.Bases);

            if (numN > 33720001)
            {
                throw new InvalidDataException($"Found a large number of Ns ({numN}) in the Y chromosome. Are you sure the PAR region is unmasked?");
            }
        }

        private static List<FastaSequence> GetFastaSequences(string fastaPrefix, Dictionary<string, Chromosome> refNameToChromosome)
        {
            string directory = Path.GetDirectoryName(fastaPrefix);
            string prefix    = Path.GetFileName(fastaPrefix);
            string[] fastaFiles   = Directory.GetFiles(directory, $"{prefix}*.fa.gz");

            var references = new List<FastaSequence>();

            foreach (string filePath in fastaFiles)
            {
                Console.Write($"  - parsing {Path.GetFileName(filePath)}... ");
                FastaReader.AddReferenceSequences(new GZipStream(FileUtilities.GetReadStream(filePath), CompressionMode.Decompress), refNameToChromosome, references);
                Console.WriteLine($"total: {references.Count} sequences");
            }

            return references.OrderBy(x => x.Chromosome.Index).ToList();
        }

        private static void CheckReferenceIndexContiguity(IEnumerable<Chromosome> chromosomes, IReadOnlyList<Chromosome> oldChromosomes)
        {
            ushort testRefIndex = 0;

            foreach (var chromosome in chromosomes)
            {
                if (chromosome.Index != testRefIndex)
                {
                    Console.WriteLine($"Found a non-contiguous entry at test refIndex: {testRefIndex} vs chromosome.Index: {chromosome.Index}");
                    Console.WriteLine($"NEW: RefIndex: {chromosome.Index}, Ensembl: {chromosome.EnsemblName}, UCSC: {chromosome.UcscName}, GenBank: {chromosome.GenBankAccession}, RefSeq: {chromosome.RefSeqAccession}");
                    Console.WriteLine($"OLD: RefIndex: {oldChromosomes[testRefIndex].Index}, Ensembl: {oldChromosomes[testRefIndex].EnsemblName}, UCSC: {oldChromosomes[testRefIndex].UcscName}, GenBank: {oldChromosomes[testRefIndex].GenBankAccession}, RefSeq: {oldChromosomes[testRefIndex].RefSeqAccession}");
                    Environment.Exit(1);
                }

                testRefIndex++;
            }
        }

        private static void CreateReferenceSequenceFile(GenomeAssembly genomeAssembly, byte patchLevel,
            IReadOnlyCollection<Chromosome> chromosomes, List<Creation.ReferenceSequence> referenceSequences)
        {
            using (var writer = new ReferenceSequenceWriter(FileUtilities.GetCreateStream(_outputCompressedPath),
                chromosomes, genomeAssembly, patchLevel))
            {
                writer.Write(referenceSequences);
            }
        }

        private static int CountNs(string s)
        {
            var numN = 0;
            foreach (char c in s) if (c == 'N') numN++;
            return numN;
        }

        public static ExitCodes Run(string command, string[] args)
        {
            var ops = new OptionSet
            {
                {
                    "cb|c=",
                    "cytogenetic band {filename}",
                    v => _cytogeneticBandPath = v
                },
                {
                    "ga=",
                    "genome assembly {version}",
                    v => _genomeAssembly = v
                },
                {
                    "gar|g=",
                    "genome assembly report {filename}",
                    v => _genomeAssemblyReportPath = v
                },
                {
                    "in|i=",
                    "FASTA {prefix}",
                    v => _fastaPrefix = v
                },
                {
                    "patch=",
                    "patch {level}",
                    (byte v) => _patchLevel = v
                },
                {
                    "rn=",
                    "reference names {filename}",
                    v => _referenceNamesPath = v
                },
                {
                    "out|o=",
                    "output compressed reference {filename}",
                    v => _outputCompressedPath = v
                }
            };

            string commandLineExample = $"{command} --in <prefix> --gar <path> --cb <path> --rn <path> --ga <genome assembly> --out <path>";

            return new ConsoleAppBuilder(args, ops)
                .Parse()
                .CheckInputFilenameExists(_genomeAssemblyReportPath, "genome assembly report", "--gar")
                .CheckInputFilenameExists(_cytogeneticBandPath, "cytogenetic band", "--cb")
                .CheckInputFilenameExists(_referenceNamesPath, "reference names", "--rn")
                .HasRequiredParameter(_fastaPrefix, "FASTA prefix", "--in")
                .HasRequiredParameter(_genomeAssembly, "genome assembly", "--ga")
                .HasRequiredParameter(_patchLevel, "patch level", "--patch")
                .HasRequiredParameter(_outputCompressedPath, "output reference", "--out")
                .SkipBanner()
                .ShowHelpMenu("Converts a FASTA file to the Nirvana reference format.", commandLineExample)
                .ShowErrors()
                .Execute(ProgramExecution);
        }
    }
}
