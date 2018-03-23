using System.Collections.Generic;
using CommandLine.Builders;
using CommandLine.NDesk.Options;
using ErrorHandling;

namespace SAUtils.MergeInterimTsvs
{
    public static class MergeIntermediateTsvs
    {
        #region members

        // filenames
        private static string _outputDirectory;
        private static readonly List<string> IntermediateFiles = new List<string>();
        private static readonly List<string> IntervalFiles = new List<string>();
        private static readonly List<string> GeneTsvFiles = new List<string>();
        private static string _miscFile;
        private static string _tsvFilesDirectory;
        private static string _compressedReference;

        #endregion

        private static ExitCodes ProgramExecution()
        {
            var intermediateSaMerger = new InterimTsvsMerger(IntermediateFiles,
                IntervalFiles, _miscFile,
                GeneTsvFiles,
                _compressedReference,
                _outputDirectory);

            intermediateSaMerger.Merge();
            
            return ExitCodes.Success;

        }

        public static ExitCodes Run(string command, string[] commandArgs)
        {
            var ops = new OptionSet
            {
                {
                     "dir|d=",
                     " directoried for TSV supplementary annotation file in intermediate format",
                     v =>_tsvFilesDirectory = v
                 },
                {
                     "tsv|t=",
                     "TSV supplementary annotation file in intermediate format",
                     v => IntermediateFiles.Add(v)
                 },
                {
                     "int|i=",
                     "TSV supplementary interval file in intermediate format",
                     v => IntervalFiles.Add(v)
                 },
                {
                     "misc|m=",
                     "refminor and global major allele in tsv format",
                     v => _miscFile = v
                },
                {
                    "gene|g=",
                    "gene annotation files in intermediate format",
                    v => GeneTsvFiles.Add(v)
                },
                 {
                     "out|o=",
                     "output Nirvana Supplementary directory",
                     v => _outputDirectory = v
                 },
                 {
                     "ref|r=",
                     "reference sequence",
                     v => _compressedReference= v
                 }
            };

            var commandLineExample = $"{command} [options]";
            var exitCode = new ConsoleAppBuilder(commandArgs, ops)
                .Parse()
                .GetTsvAndIntervalFiles(_tsvFilesDirectory, IntermediateFiles,
                    IntervalFiles, GeneTsvFiles, ref _miscFile)
                .CheckInputFilenameExists(_compressedReference, "Input compressed reference file", "--ref")
                .HasRequiredParameter(_outputDirectory, "Output Supplementary directory", "--out")
                .CheckNonZero(_miscFile == null
                    ? IntermediateFiles.Count + IntervalFiles.Count + GeneTsvFiles.Count
                    : IntermediateFiles.Count + IntervalFiles.Count +
                    GeneTsvFiles.Count + 1, "No intermediate files were provided, use --dir, --tsv, --int or --gene")
                .CheckEachFilenameExists(IntermediateFiles, "Intermediate Annotation file name", "--tsv", false)
                .CheckEachFilenameExists(IntervalFiles, "Intermediate interval file name", "--int", false)
                .CheckInputFilenameExists(_miscFile, "Intermediate misc file name", "--misc", false)
                .CheckEachFilenameExists(GeneTsvFiles, "Intermediate gene file name", "--gene", false)
                .SkipBanner()
                .ShowHelpMenu("Reads provided intermediate TSV files and creates supplementary database", commandLineExample)
                .ShowErrors()
                .Execute(ProgramExecution);

            return exitCode;

        }
    }
}
