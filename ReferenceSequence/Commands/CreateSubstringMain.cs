using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using CommandLine.Builders;
using CommandLine.NDesk.Options;
using ErrorHandling;
using Genome;
using IO;
using ReferenceSequence.Common;
using ReferenceSequence.Compression;
using ReferenceSequence.Creation;
using ReferenceSequence.IO;

namespace ReferenceSequence.Commands
{
    public static class CreateSubstringMain
    {
        private static string _fastaPath;
        private static string _genomeAssemblyReportPath;
        private static string _cytogeneticBandPath;
        
        private static string _genomeAssembly;
        private static string _outputCompressedPath;

        private static int _beginPosition;
        private static int _endPosition;

        private static ExitCodes ProgramExecution()
        {
            var genomeAssembly = GenomeAssemblyHelper.Convert(_genomeAssembly);

            Console.Write("- reading the genome assembly report... ");
            var dummyRefNameToChromosome = new Dictionary<string, Chromosome>();
            List<Chromosome> chromosomes = AssemblyReader.GetChromosomes(FileUtilities.GetReadStream(_genomeAssemblyReportPath), dummyRefNameToChromosome, 0);
            int numRefSeqs  = chromosomes.Count;
            Console.WriteLine($"{numRefSeqs} references found.");

            Dictionary<string, Chromosome> refNameToChromosome = ReferenceDictionaryUtils.GetRefNameToChromosome(chromosomes);

            Console.Write("- reading FASTA file... ");
            var fastaSequence = GetFastaSequence(_fastaPath, refNameToChromosome);
            Console.WriteLine($"- sequence length: {fastaSequence.Bases.Length:N0}");

            Console.Write("- reading cytogenetic bands... ");
            List<Band> cytogeneticBands = GetCytogeneticBands(fastaSequence.Chromosome.Index, numRefSeqs, refNameToChromosome);
            Console.WriteLine("finished.");

            Console.Write("- applying 2-bit compression... ");
            var referenceSequence = CreateReferenceSequence(fastaSequence, cytogeneticBands);
            Console.WriteLine("finished.");

            Console.Write("- creating reference sequence file... ");
            var minimalChromosomes = new List<Chromosome> { fastaSequence.Chromosome };
            CreateReferenceSequenceFile(genomeAssembly, minimalChromosomes, referenceSequence);
            long fileSize = new FileInfo(_outputCompressedPath).Length;
            Console.WriteLine($"{fileSize:N0} bytes");

            return ExitCodes.Success;
        }

        private static List<Band> GetCytogeneticBands(ushort refIndex, int numRefSeqs, Dictionary<string, Chromosome> refNameToChromosome)
        {
            List<Band> chrBands = CytogeneticBandsReader.GetCytogeneticBands(FileUtilities.GetReadStream(_cytogeneticBandPath), numRefSeqs, refNameToChromosome)[refIndex];

            int substringBegin = _beginPosition;
            int substringEnd   = _beginPosition + _endPosition - 1;

            return chrBands.Where(band => Intervals.Utilities.Overlaps(substringBegin, substringEnd, band.Begin, band.End))
                .ToList();
        }

        private static void CreateReferenceSequenceFile(GenomeAssembly genomeAssembly, IReadOnlyCollection<Chromosome> chromosomes, Creation.ReferenceSequence referenceSequence)
        {
            using (var writer = new ReferenceSequenceWriter(FileUtilities.GetCreateStream(_outputCompressedPath),
                chromosomes, genomeAssembly, 0))
            {
                writer.Write(new List<Creation.ReferenceSequence> {referenceSequence});
            }
        }

        private static Creation.ReferenceSequence CreateReferenceSequence(FastaSequence fastaSequence, List<Band> cytogeneticBands)
        {
            Band[] bands = cytogeneticBands.ToArray();
            (byte[] buffer, MaskedEntry[] maskedEntries) = TwoBitCompressor.Compress(fastaSequence.Bases);
            return new Creation.ReferenceSequence(buffer, maskedEntries, bands, _beginPosition - 1, fastaSequence.Bases.Length);
        }

        private static FastaSequence GetFastaSequence(string fastaPath, Dictionary<string, Chromosome> refNameToChromosome)
        {
            var references = new List<FastaSequence>();
            FastaReader.AddReferenceSequences(new GZipStream(FileUtilities.GetReadStream(fastaPath), CompressionMode.Decompress), refNameToChromosome, references);

            if (references.Count != 1)
            {
                throw new InvalidDataException($"Expected 1 reference, but found {references.Count} references.");
            }

            var reference    = references[0];
            int length       = _endPosition - _beginPosition + 1;
            string substring = reference.Bases.Substring(_beginPosition - 1, length);

            return new FastaSequence(reference.Chromosome, substring);
        }

        public static ExitCodes Run(string command, string[] args)
        {
            var ops = new OptionSet
            {
                {
                    "begin=",
                    "begin {position}",
                    (int v) => _beginPosition = v
                },
                {
                    "cb|c=",
                    "cytogenetic band {filename}",
                    v => _cytogeneticBandPath = v
                },
                {
                    "end=",
                    "end {position}",
                    (int v) => _endPosition = v
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
                    "FASTA {filename}",
                    v => _fastaPath = v
                },
                {
                    "out|o=",
                    "output compressed reference {filename}",
                    v => _outputCompressedPath = v
                }
            };

            string commandLineExample = $"{command} --in <path> --gar <path> --cb <path> --rn <path> --ga <genome assembly> --out <path>";

            return new ConsoleAppBuilder(args, ops)
                .Parse()
                .CheckInputFilenameExists(_genomeAssemblyReportPath, "genome assembly report", "--gar")
                .CheckInputFilenameExists(_cytogeneticBandPath, "cytogenetic band", "--cb")
                .HasRequiredParameter(_fastaPath, "FASTA prefix", "--in")
                .HasRequiredParameter(_genomeAssembly, "genome assembly", "--ga")
                .HasRequiredParameter(_outputCompressedPath, "output reference", "--out")
                .HasRequiredParameter(_beginPosition, "offset", "--begin")
                .HasRequiredParameter(_endPosition, "length", "--end")
                .SkipBanner()
                .ShowHelpMenu("Converts a FASTA file to the Nirvana reference format.", commandLineExample)
                .ShowErrors()
                .Execute(ProgramExecution);
        }
    }
}
