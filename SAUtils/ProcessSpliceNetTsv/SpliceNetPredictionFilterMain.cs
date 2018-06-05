using CommandLine.Builders;
using CommandLine.NDesk.Options;
using ErrorHandling;

namespace SAUtils.ProcessSpliceNetTsv
{
    public static class SpliceNetPredictionFilterMain
    {
        private static string _spliceNetResultsFile;
        private static string _filteredResultsFile;
        private static string _gffFile1;
        private static string _gffFile2;

        public static ExitCodes Run(string command, string[] commandArgs)
        {
            var ops = new OptionSet
            {
                {
                    "in|i=",
                    "SpliceNet prediction results",
                    v => _spliceNetResultsFile = v
                },
                {
                    "gff1|g1=",
                    "Gene structure file 1",
                    v => _gffFile1 = v
                },
                {
                    "gff2|g2=",
                    "Gene structures file 2",
                    v => _gffFile2 = v
                },
                {
                    "out|o=",
                    "Filtered SpliceNet results",
                    v => _filteredResultsFile = v
                }
            };

            var commandLineExample = $"{command} [options]";

            var exitCode = new ConsoleAppBuilder(commandArgs, ops)
                .Parse()
                .CheckInputFilenameExists(_spliceNetResultsFile, "SpliceNet prediction results", "--in")
                .CheckInputFilenameExists(_gffFile1, "Gene structures file 1", "--gff1")
                .CheckInputFilenameExists(_gffFile2, "Gene structures file 2", "--gff2")
                .SkipBanner()
                .ShowHelpMenu("Filter SpliceNet results based on predicted scores and variant location",
                    commandLineExample)
                .ShowErrors()
                .Execute(ProgramExecution);

            return exitCode;
        }

        private static ExitCodes ProgramExecution()
        {
            PredictionFilter.Filter(_spliceNetResultsFile, _gffFile1, _gffFile2, _filteredResultsFile);
            return ExitCodes.Success;
        }
    }
}