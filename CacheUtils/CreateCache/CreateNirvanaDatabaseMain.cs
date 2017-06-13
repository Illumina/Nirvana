using CacheUtils.CombineAndUpdateGenes.FileHandling;
using CacheUtils.CreateCache.FileHandling;
using CacheUtils.DataDumperImport.FileHandling;
using CommandLine.Handlers;
using CommandLine.NDesk.Options;
using VariantAnnotation.DataStructures;
using VariantAnnotation.FileHandling.TranscriptCache;
using VariantAnnotation.Utilities;

namespace CacheUtils.CreateCache
{
    internal sealed class CreateNirvanaDatabaseMain : AbstractCommandLineHandler
    {
        public static int Run(string command, string[] args)
        {
            var ops = new OptionSet
            {
                {
                    "genes|g=",
                    "input merged genes {filename}",
                    v => ConfigurationSettings.InputMergedGenesPath = v
                },
                {
                    "in|i=",
                    "input filename {prefix}",
                    v => ConfigurationSettings.InputPrefix = v
                },
                {
                    "lrg|l=",
                    "input LRG {filename}",
                    v => ConfigurationSettings.InputLrgPath = v
                },
                {
                    "out|o=",
                    "output cache file {prefix}",
                    v => ConfigurationSettings.OutputCacheFilePrefix = v
                },
                {
                    "ref|r=",
                    "input reference {filename}",
                    v => ConfigurationSettings.InputReferencePath = v
                },
                {
                    "truth|t=",
                    "input GFF truth {filename}",
                    v => ConfigurationSettings.InputGffTruthPath = v
                }
            };

            var commandLineExample = $"{command} --in <VEP directory> --out <cache prefix> --vep <VEP version>";

            var converter = new CreateNirvanaDatabaseMain("Converts *deserialized* VEP cache files to Nirvana cache format", ops, commandLineExample, Constants.Authors);
            converter.Execute(args);
            return converter.ExitCode;
        }

        /// <summary>
        /// constructor
        /// </summary>
        private CreateNirvanaDatabaseMain(string programDescription, OptionSet ops, string commandLineExample, string programAuthors)
            : base(programDescription, ops, commandLineExample, programAuthors)
        { }

        /// <summary>
        /// validates the command line
        /// </summary>
        protected override void ValidateCommandLine()
        {
            HasRequiredParameter(ConfigurationSettings.InputPrefix, "VEP", "--in");
            CheckInputFilenameExists(ConfigurationSettings.InputReferencePath, "compressed reference", "--ref");
            CheckInputFilenameExists(ConfigurationSettings.InputMergedGenesPath, "merged genes", "--genes");
            HasRequiredParameter(ConfigurationSettings.OutputCacheFilePrefix, "Nirvana", "--out");
            CheckInputFilenameExists(ConfigurationSettings.InputLrgPath, "LRG", "--lrg");
            CheckInputFilenameExists(ConfigurationSettings.InputGffTruthPath, "GFF truth", "--truth");
        }

        /// <summary>
        /// executes the program
        /// </summary>
        protected override void ProgramExecution()
        {
            var transcriptPath = ConfigurationSettings.InputPrefix + ".transcripts.gz";
            var regulatoryPath = ConfigurationSettings.InputPrefix + ".regulatory.gz";
            var genePath       = ConfigurationSettings.InputPrefix + ".genes.gz";
            var intronPath     = ConfigurationSettings.InputPrefix + ".introns.gz";
            var mirnaPath      = ConfigurationSettings.InputPrefix + ".mirnas.gz";
            var siftPath       = ConfigurationSettings.InputPrefix + ".sift.dat";
            var polyphenPath   = ConfigurationSettings.InputPrefix + ".polyphen.dat";
            var peptidePath    = ConfigurationSettings.InputPrefix + ".peptides.gz";

            var renamer = ChromosomeRenamer.GetChromosomeRenamer(FileUtilities.GetReadStream(ConfigurationSettings.InputReferencePath));

            using (var transcriptReader = new VepTranscriptReader(transcriptPath))
            using (var regulatoryReader = new VepRegulatoryReader(regulatoryPath))
            using (var geneReader       = new VepGeneReader(genePath))
            using (var mergedGeneReader = new VepCombinedGeneReader(ConfigurationSettings.InputMergedGenesPath))
            using (var intronReader     = new VepSimpleIntervalReader(intronPath, "intron", GlobalImportCommon.FileType.Intron))
            using (var mirnaReader      = new VepSimpleIntervalReader(mirnaPath, "miRNA", GlobalImportCommon.FileType.MicroRna))
            using (var peptideReader    = new VepSequenceReader(peptidePath, "peptide", GlobalImportCommon.FileType.Peptide))
            {
                var converter = new NirvanaDatabaseCreator(transcriptReader, regulatoryReader, geneReader,
                    mergedGeneReader, intronReader, mirnaReader, peptideReader, renamer);

                converter.LoadData();
                converter.MarkCanonicalTranscripts(ConfigurationSettings.InputLrgPath);
                converter.CreateTranscriptCacheFile(ConfigurationSettings.OutputCacheFilePrefix);
                converter.CopyPredictionCacheFile("SIFT", siftPath, CacheConstants.SiftPath(ConfigurationSettings.OutputCacheFilePrefix));
                converter.CopyPredictionCacheFile("PolyPhen", polyphenPath, CacheConstants.PolyPhenPath(ConfigurationSettings.OutputCacheFilePrefix));
            }
        }
    }
}
