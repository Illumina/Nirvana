using CommandLine.Builders;
using CommandLine.NDesk.Options;
using ErrorHandling;
using IO;
using SAUtils.InputFileParsers;
using VariantAnnotation.NSA;

namespace SAUtils.NsaIndexUpdater
{
    public static class UpdateIndex
    {
        private static string _inputIndexFile;
        private static string _outputIndexFile;
        private static string _versionFile;
        public static ExitCodes Run(string command, string[] commandArgs)
        {

            var ops = new OptionSet
            {
                {
                    "ind|i=",
                    "input NSA index file path",
                    v => _inputIndexFile = v
                },
                {
                    "ver|r=",
                    "version file path",
                    v => _versionFile = v
                },
                {
                    "out|o=",
                    "output index file path",
                    v => _outputIndexFile= v
                }
            };

            var commandLineExample = $"{command} --ind <input NSA index file path> --out <output index file path> --ver <version file path>";

            var exitCode = new ConsoleAppBuilder(commandArgs, ops)
                .Parse()
                .CheckInputFilenameExists(_inputIndexFile, "input NSA index file path", "--ind")
                .HasRequiredParameter(_outputIndexFile, "output index file path", "--out")
                .CheckInputFilenameExists(_versionFile, "version file path", "--ver")
                .SkipBanner()
                .ShowHelpMenu("Extracts mini supplementary annotations for the given range from Nirvana Supplementary Annotations files.", commandLineExample)
                .ShowErrors()
                .Execute(ProgramExecution);

            return exitCode;
        }

        private static ExitCodes ProgramExecution()
        {
            using (var indexStream = FileUtilities.GetReadStream(_inputIndexFile))
            using (var outStream = FileUtilities.GetCreateStream(_outputIndexFile))
            using (var extWriter = new ExtendedBinaryWriter(outStream))
            {
                var version = DataSourceVersionReader.GetSourceVersion(_versionFile);
                var oldIndex = new NsaIndex(indexStream);
                var newIndex = new NsaIndex(extWriter, oldIndex.Assembly, version, oldIndex.JsonKey, oldIndex.MatchByAllele, oldIndex.IsArray, oldIndex.SchemaVersion, oldIndex.IsPositional);

                newIndex.Write(oldIndex.GetBlocks());
            }

            return ExitCodes.Success;
        }
    }
}
