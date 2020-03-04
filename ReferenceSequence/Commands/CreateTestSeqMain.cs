using System;
using System.Collections.Generic;
using System.IO;
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
    public static class CreateTestSeqMain
    {
        private static string _outputCompressedPath;

        private static ExitCodes ProgramExecution()
        {
            var testSeqChromosome = new Chromosome("chrTestSeq", "TestSeq", null, null, 1, 0);
            var chromosomes       = new List<IChromosome> {testSeqChromosome};
            
            Console.Write("- creating FASTA sequence... ");
            var fastaSequence = new FastaSequence(testSeqChromosome, "NNATGTTTCCACTTTCTCCTCATTAGANNNTAACGAATGGGTGATTTCCCTAN");
            Console.WriteLine($"- sequence length: {fastaSequence.Bases.Length:N0}");

            Console.Write("- applying 2-bit compression... ");
            var referenceSequence = CreateReferenceSequence(fastaSequence);
            Console.WriteLine("finished.");

            Console.Write("- creating reference sequence file... ");
            CreateReferenceSequenceFile(GenomeAssembly.GRCh37, chromosomes, referenceSequence);
            long fileSize = new FileInfo(_outputCompressedPath).Length;
            Console.WriteLine($"{fileSize:N0} bytes");

            return ExitCodes.Success;
        }

        private static void CreateReferenceSequenceFile(GenomeAssembly genomeAssembly, IReadOnlyCollection<IChromosome> chromosomes, Creation.ReferenceSequence referenceSequence)
        {
            using (var writer = new ReferenceSequenceWriter(FileUtilities.GetCreateStream(_outputCompressedPath),
                chromosomes, genomeAssembly, 0))
            {
                writer.Write(new List<Creation.ReferenceSequence> { referenceSequence });
            }
        }

        private static Creation.ReferenceSequence CreateReferenceSequence(FastaSequence fastaSequence)
        {
            (byte[] buffer, MaskedEntry[] maskedEntries) = TwoBitCompressor.Compress(fastaSequence.Bases);
            return new Creation.ReferenceSequence(buffer, maskedEntries, new Band[0], 0, fastaSequence.Bases.Length);
        }

        public static ExitCodes Run(string command, string[] args)
        {
            var ops = new OptionSet
            {
                {
                    "out|o=",
                    "output compressed reference {filename}",
                    v => _outputCompressedPath = v
                }
            };

            string commandLineExample = $"{command} --out <prefix>";

            return new ConsoleAppBuilder(args, ops)
                .Parse()
                .HasRequiredParameter(_outputCompressedPath, "output reference", "--out")
                .SkipBanner()
                .ShowHelpMenu("Creates a TestSeq_reference.dat file.", commandLineExample)
                .ShowErrors()
                .Execute(ProgramExecution);
        }
    }
}
