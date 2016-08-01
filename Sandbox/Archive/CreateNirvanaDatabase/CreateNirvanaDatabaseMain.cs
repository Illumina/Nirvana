using System;
using System.IO;
using System.Linq;
using Illumina.VariantAnnotation.DataStructures;
using Illumina.VariantAnnotation.FileHandling;
using Illumina.VariantAnnotation.Utilities;
using NDesk.Options;

namespace CreateNirvanaDatabase
{
    class CreateNirvanaDatabaseMain : AbstractCommandLineHandler
    {
        // constructor
        private CreateNirvanaDatabaseMain(string programDescription, OptionSet ops, string commandLineExample, string programAuthors)
            : base(programDescription, ops, commandLineExample, programAuthors)
        { }

        /// <summary>
        /// validates the command line
        /// </summary>
        protected override void ValidateCommandLine()
        {
            CheckDirectoryExists(ConfigurationSettings.InputVepDirectory, "VEP", "--in");
            CheckInputFilenameExists(ConfigurationSettings.InputReferencePath, "compressed reference", "--ref");
            CheckDirectoryExists(ConfigurationSettings.OutputNirvanaDirectory, "Nirvana", "--out");
            HasRequiredParameter(ConfigurationSettings.VepVersion, "VEP version", "--vep");
			HasRequiredParameter(ConfigurationSettings.InputGeneSymbolsPath, "gene symbols", "--genesymbols");
            HasRequiredParameter(ConfigurationSettings.GenomeAssembly, "genome assembly", "--ga");
            HasRequiredDate(ConfigurationSettings.VepReleaseDate, "VEP release date", "--date");

            HasOneOptionSelected(ConfigurationSettings.ImportRefSeqTranscripts, "--refseq",
                ConfigurationSettings.ImportEnsemblTranscripts, "--ensembl");

            if (ConfigurationSettings.ImportRefSeqTranscripts)
            {
                CheckInputFilenameExists(ConfigurationSettings.InputLrgPath, "LRG", "--lrg");
                CheckInputFilenameExists(ConfigurationSettings.InputHgncIdsPath, "HGNC ids", "--hgncids");
            }
        }

        /// <summary>
        /// executes the program
        /// </summary>
        protected override void ProgramExecution()
        {
            var transcriptDataSource = ConfigurationSettings.ImportRefSeqTranscripts ? TranscriptDataSource.RefSeq : TranscriptDataSource.Ensembl;
            var converter = new VepDatabaseConverter(ConfigurationSettings.VepVersion, transcriptDataSource, ConfigurationSettings.DoNotFilterTranscripts, ConfigurationSettings.InputReferencePath, ConfigurationSettings.VepReleaseDate);
            converter.LoadGeneSymbols(ConfigurationSettings.InputGeneSymbolsPath);

            if (ConfigurationSettings.ImportRefSeqTranscripts)
            {
                converter.LoadLrgData(ConfigurationSettings.InputLrgPath);
				converter.LoadHgncIds(ConfigurationSettings.InputHgncIdsPath);
            }

            converter.AddReferenceSequences();

            string[] ndbFiles = Directory.GetFiles(ConfigurationSettings.OutputNirvanaDirectory, "*.ndb");
            string[] vepDirectories = Directory.GetDirectories(ConfigurationSettings.InputVepDirectory);

            // ==========================
            // handle file deletion logic
            // ==========================

            var chromosomeRenamer = AnnotationLoader.Instance.ChromosomeRenamer;

            bool processSpecificRefSeq = !string.IsNullOrEmpty(ConfigurationSettings.OnlyProcessReferenceSequenceName);

            if (processSpecificRefSeq)
            {
                // delete the Nirvana data file for the specific reference sequence
                string refSeqName = chromosomeRenamer.GetUcscReferenceName(ConfigurationSettings.OnlyProcessReferenceSequenceName, false);
                string refSeqFileName = $"{refSeqName}.ndb";
                string refSeqPath = Path.Combine(ConfigurationSettings.OutputNirvanaDirectory, refSeqFileName);

                if (File.Exists(refSeqPath))
                {
                    Console.Write("Deleting existing Nirvana database file ({0})... ", refSeqFileName);
                    File.Delete(refSeqPath);
                    Console.WriteLine("finished.\n");
                }
            }
            else if (ConfigurationSettings.SkipExistingNirvanaFiles)
            {
                // delete files with a zero file size
                foreach (
                    var ndbFile in from ndbFile in ndbFiles
                                   let fi = new FileInfo(ndbFile)
                                   where fi.Length == 0
                                   select ndbFile)
                {
                    File.Delete(ndbFile);
                }
            }
            else
            {
                // delete all files
                foreach (var ndbFile in ndbFiles) File.Delete(ndbFile);
            }

            // ========================
            // create the desired files
            // ========================

            // process each VEP directory
            int numDirectoriesProcessed = 0;

            foreach (string refSeqDirectory in vepDirectories)
            {
                string refSeq        = Path.GetFileName(refSeqDirectory);
                string currentRefSeq = chromosomeRenamer.GetUcscReferenceName(refSeq, false);

                // skip chromosomes that are not common to the reference and VEP
                if (!chromosomeRenamer.InReferenceAndVep(refSeq))
                {
                    Console.WriteLine("- skipping reference {0} (not present in both the reference sequences and VEP)", refSeq);
                    continue;
                }

                if (currentRefSeq == null) continue;

                // skip unwanted reference sequences
                if (processSpecificRefSeq && (currentRefSeq != ConfigurationSettings.OnlyProcessReferenceSequenceName))
                    continue;

                // continue if we should skip existing reference sequences
                if (ConfigurationSettings.SkipExistingNirvanaFiles &&
                    File.Exists(VepDatabaseConverter.GetDatabasePath(currentRefSeq,
                        ConfigurationSettings.OutputNirvanaDirectory)))
                {
                    continue;
                }

                Console.WriteLine("Parsing reference sequence [{0}]:", currentRefSeq);
                numDirectoriesProcessed++;

                var genomeAssembly = GenomeAssemblyUtilities.Convert(ConfigurationSettings.GenomeAssembly);
                converter.ParseDumpDirectory(refSeqDirectory, currentRefSeq, ConfigurationSettings.OutputNirvanaDirectory, genomeAssembly);
            }

            Console.WriteLine("\n{0} directories processed.", numDirectoriesProcessed);
        }

        static int Main(string[] args)
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
                    "genesymbols|g=",
                    "input gene symbols {filename} (RefSeq)",
                    v => ConfigurationSettings.InputGeneSymbolsPath = v
                },
                {
                    "hgncids=",
                    "input HGNC ids {filename} (RefSeq)",
                    v => ConfigurationSettings.InputHgncIdsPath = v
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
                    "no-filter",
                    "do not filter transcripts",
                    v => ConfigurationSettings.DoNotFilterTranscripts = v != null
                },
                {
                    "lrg|l=",
                    "input LRG {filename}",
                    v => ConfigurationSettings.InputLrgPath = v
                },
                {
                    "only|n=",
                    "only process reference sequence {name}",
                    v => ConfigurationSettings.OnlyProcessReferenceSequenceName = v
                },
                {
                    "out|o=",
                    "output Nirvana {directory}",
                    v => ConfigurationSettings.OutputNirvanaDirectory = v
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
                    "skip|s",
                    "skip existing Nirvana files",
                    v => ConfigurationSettings.SkipExistingNirvanaFiles = v != null
                },
                {
                    "vep=",
                    "VEP {version}",
                    (ushort v) => ConfigurationSettings.VepVersion = v
                }
            };

            var commandLineExample = "--in <VEP directory> --out <Nirvana directory> --vep <VEP version>";

            var converter = new CreateNirvanaDatabaseMain("Converts *deserialized* VEP cache files to Nirvana format", ops, commandLineExample, Constants.Authors);
            converter.Execute(args);
            return converter.ExitCode;
        }
    }
}
