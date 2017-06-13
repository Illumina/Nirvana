using System;
using System.IO;
using CacheUtils.DataDumperImport.FileHandling;
using CacheUtils.ParseVepCacheDirectory.PredictionConversion;
using CommandLine.Handlers;
using CommandLine.NDesk.Options;
using VariantAnnotation.DataStructures;
using VariantAnnotation.DataStructures.Transcript;
using VariantAnnotation.FileHandling.Compression;
using VariantAnnotation.FileHandling.TranscriptCache;
using VariantAnnotation.Interface;

namespace CacheUtils.ParseVepCacheDirectory
{
    public sealed class ParseVepCacheDirectoryMain : AbstractCommandLineHandler
    {
        public static int Run(string command, string[] args)
        {
            var ops = new OptionSet
            {
                {
                    "date=",
                    "VEP release {date}",
                    v => ConfigurationSettings.VepReleaseDate = v
                },
                {
                    "ensembl",
                    "import Ensembl transcripts",
                    v => ConfigurationSettings.ImportEnsemblTranscripts = v != null
                },
                {
                    "ga=",
                    "genome assembly {version}",
                    v => ConfigurationSettings.GenomeAssembly = v
                },
                {
                    "in|i=",
                    "input VEP {directory}",
                    v => ConfigurationSettings.InputVepDirectory = v
                },
                {
                    "out|o=",
                    "output filename {stub}",
                    v => ConfigurationSettings.OutputStub = v
                },
                {
                    "ref|r=",
                    "input reference {filename}",
                    v => ConfigurationSettings.InputReferencePath = v
                },
                {
                    "refseq",
                    "import RefSeq transcripts",
                    v => ConfigurationSettings.ImportRefSeqTranscripts = v != null
                },
                {
                    "vep=",
                    "VEP {version}",
                    (ushort v) => ConfigurationSettings.VepVersion = v
                }
            };

            var commandLineExample = $"{command} --in <VEP directory> --out <Nirvana pre-cache file> --vep <VEP version>";

            var converter = new ParseVepCacheDirectoryMain("Converts *deserialized* VEP cache files to a Nirvana pre-cache file",
                ops, commandLineExample, Constants.Authors);
            converter.Execute(args);
            return converter.ExitCode;
        }

        // constructor
        private ParseVepCacheDirectoryMain(string programDescription, OptionSet ops, string commandLineExample,
            string programAuthors)
            : base(programDescription, ops, commandLineExample, programAuthors)
        {
        }

        /// <summary>
        /// validates the command line
        /// </summary>
        protected override void ValidateCommandLine()
        {
            CheckDirectoryExists(ConfigurationSettings.InputVepDirectory, "VEP", "--in");
            CheckInputFilenameExists(ConfigurationSettings.InputReferencePath, "compressed reference", "--ref");
            HasRequiredParameter(ConfigurationSettings.OutputStub, "output stub", "--out");
            HasRequiredParameter(ConfigurationSettings.VepVersion, "VEP version", "--vep");
            HasRequiredParameter(ConfigurationSettings.GenomeAssembly, "genome assembly", "--ga");
            HasRequiredDate(ConfigurationSettings.VepReleaseDate, "VEP release date", "--date");

            HasOneOptionSelected(ConfigurationSettings.ImportRefSeqTranscripts, "--refseq",
                ConfigurationSettings.ImportEnsemblTranscripts, "--ensembl");
        }

        /// <summary>
        /// executes the program
        /// </summary>
        protected override void ProgramExecution()
        {
            var transcriptSource = ConfigurationSettings.ImportRefSeqTranscripts
                ? TranscriptDataSource.RefSeq
                : TranscriptDataSource.Ensembl;

            var referenceIndex = new ReferenceIndex(ConfigurationSettings.InputReferencePath);
            var vepDirectories = referenceIndex.GetUcscKaryotypeOrder(ConfigurationSettings.InputVepDirectory);
            var converter = new VepCacheParser(transcriptSource);

            var genomeAssembly = GenomeAssemblyUtilities.Convert(ConfigurationSettings.GenomeAssembly);

            // =========================
            // create the pre-cache file
            // =========================

            // process each VEP directory
            int numDirectoriesProcessed = 0;

            var transcriptPath = ConfigurationSettings.OutputStub + ".transcripts.gz";
            var regulatoryPath = ConfigurationSettings.OutputStub + ".regulatory.gz";
            var genePath       = ConfigurationSettings.OutputStub + ".genes.gz";
            var intronPath     = ConfigurationSettings.OutputStub + ".introns.gz";
            var exonPath       = ConfigurationSettings.OutputStub + ".exons.gz";
            var mirnaPath      = ConfigurationSettings.OutputStub + ".mirnas.gz";
            var siftPath       = ConfigurationSettings.OutputStub + ".sift.dat";
            var polyphenPath   = ConfigurationSettings.OutputStub + ".polyphen.dat";
            var cdnaPath       = ConfigurationSettings.OutputStub + ".cdnas.gz";
            var peptidePath    = ConfigurationSettings.OutputStub + ".peptides.gz";

            using (var transcriptWriter = GZipUtilities.GetStreamWriter(transcriptPath))
            using (var regulatoryWriter = GZipUtilities.GetStreamWriter(regulatoryPath))
            using (var geneWriter       = GZipUtilities.GetStreamWriter(genePath))
            using (var intronWriter     = GZipUtilities.GetStreamWriter(intronPath))
            using (var exonWriter       = GZipUtilities.GetStreamWriter(exonPath))
            using (var mirnaWriter      = GZipUtilities.GetStreamWriter(mirnaPath))
            using (var siftWriter       = GZipUtilities.GetBinaryWriter(siftPath + ".tmp"))
            using (var polyphenWriter   = GZipUtilities.GetBinaryWriter(polyphenPath + ".tmp"))
            using (var cdnaWriter       = GZipUtilities.GetStreamWriter(cdnaPath))
            using (var peptideWriter    = GZipUtilities.GetStreamWriter(peptidePath))
            {
                transcriptWriter.NewLine = "\n";
                regulatoryWriter.NewLine = "\n";
                geneWriter.NewLine       = "\n";
                intronWriter.NewLine     = "\n";
                exonWriter.NewLine       = "\n";
                mirnaWriter.NewLine      = "\n";
                cdnaWriter.NewLine       = "\n";
                peptideWriter.NewLine    = "\n";

                WriteHeader(transcriptWriter, GlobalImportCommon.FileType.Transcript, transcriptSource, genomeAssembly);
                WriteHeader(regulatoryWriter, GlobalImportCommon.FileType.Regulatory, transcriptSource, genomeAssembly);
                WriteHeader(geneWriter,       GlobalImportCommon.FileType.Gene,       transcriptSource, genomeAssembly);
                WriteHeader(intronWriter,     GlobalImportCommon.FileType.Intron,     transcriptSource, genomeAssembly);
                WriteHeader(exonWriter,       GlobalImportCommon.FileType.Exon,       transcriptSource, genomeAssembly);
                WriteHeader(mirnaWriter,      GlobalImportCommon.FileType.MicroRna,   transcriptSource, genomeAssembly);
                WriteHeader(siftWriter,       GlobalImportCommon.FileType.Sift,       transcriptSource, genomeAssembly);
                WriteHeader(polyphenWriter,   GlobalImportCommon.FileType.PolyPhen,   transcriptSource, genomeAssembly);
                WriteHeader(cdnaWriter,       GlobalImportCommon.FileType.CDna,       transcriptSource, genomeAssembly);
                WriteHeader(peptideWriter,    GlobalImportCommon.FileType.Peptide,    transcriptSource, genomeAssembly);

                foreach (var refTuple in vepDirectories)
                {
                    Console.WriteLine("Parsing reference sequence [{0}]:", refTuple.Item1);
                    numDirectoriesProcessed++;

                    var refIndex = referenceIndex.GetIndex(refTuple.Item1);

                    converter.ParseDumpDirectory(refIndex, refTuple.Item2, transcriptWriter, regulatoryWriter, geneWriter,
                        intronWriter, exonWriter, mirnaWriter, siftWriter, polyphenWriter, cdnaWriter, peptideWriter);
                }
            }

            Console.WriteLine("\n{0} directories processed.", numDirectoriesProcessed);

            converter.DumpStatistics();
            Console.WriteLine();

            // convert our protein function predictions
            var predictionConverter = new PredictionConverter(referenceIndex.NumReferenceSeqs);
            predictionConverter.Convert(siftPath, "SIFT", GlobalImportCommon.FileType.Sift);
            predictionConverter.Convert(polyphenPath, "PolyPhen", GlobalImportCommon.FileType.PolyPhen);
        }

        /// <summary>
        /// writes the header to our output file
        /// </summary>
        private static void WriteHeader(StreamWriter writer, GlobalImportCommon.FileType fileType,
            TranscriptDataSource transcriptSource, GenomeAssembly genomeAssembly)
        {
            var vepReleaseTicks = DateTime.Parse(ConfigurationSettings.VepReleaseDate).Ticks;

            writer.WriteLine("{0}\t{1}", GlobalImportCommon.Header, (byte)fileType);
            writer.WriteLine("{0}\t{1}\t{2}\t{3}", ConfigurationSettings.VepVersion, vepReleaseTicks, (byte)transcriptSource, (byte)genomeAssembly);
        }

        /// <summary>
        /// writes the header to our output file
        /// </summary>
        private static void WriteHeader(BinaryWriter writer, GlobalImportCommon.FileType fileType,
            TranscriptDataSource transcriptSource, GenomeAssembly genomeAssembly)
        {
            var vepReleaseTicks = DateTime.Parse(ConfigurationSettings.VepReleaseDate).Ticks;

            writer.Write(GlobalImportCommon.Header);
            writer.Write((byte)fileType);
            writer.Write(ConfigurationSettings.VepVersion);
            writer.Write(vepReleaseTicks);
            writer.Write((byte)transcriptSource);
            writer.Write((byte)genomeAssembly);
            writer.Write(CacheConstants.GuardInt);
        }
    }
}
