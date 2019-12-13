using System.IO;
using System.IO.Compression;
using System.Linq;
using CommandLine.Builders;
using CommandLine.NDesk.Options;
using Compression.Utilities;
using ErrorHandling;
using IO;
using OptimizedCore;
using VariantAnnotation.Interface.IO;

namespace SAUtils.OneKGenSvDb
{
    public static class VcfToBed
    {
        private static string _inputFileName;
        private static string _outputDirectory;

        public static ExitCodes Run(string command, string[] commandArgs)
        {
            var ops = new OptionSet
            {
                {
                    "in|i=",
                    "OneKGenSv VCF file",
                    v => _inputFileName = v
                },
                {
                    "out|o=",
                    "output directory",
                    v => _outputDirectory = v
                }
            };

            string commandLineExample = $"{command} [options]";

            var exitCode = new ConsoleAppBuilder(commandArgs, ops)
                .Parse()
                .CheckInputFilenameExists(_inputFileName, "OneKGenSv VCF file", "--in")
                .CheckDirectoryExists(_outputDirectory, "output directory", "--out")
                .SkipBanner()
                .ShowHelpMenu("Convert the VCF file into BED-like format", commandLineExample)
                .ShowErrors()
                .Execute(ProgramExecution);

            return exitCode;
        }

        private static ExitCodes ProgramExecution()
        {
            string outFileName = Path.GetFileName(_inputFileName).Replace("vcf", "bed");
            using (var reader = GZipUtilities.GetAppropriateStreamReader(_inputFileName))
            using (var outputStream = FileUtilities.GetCreateStream(Path.Combine(_outputDirectory, outFileName)))
            using (var outputGzipStream = new GZipStream(outputStream, CompressionMode.Compress))
            using (var writer = new StreamWriter(outputGzipStream))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var fields = line.OptimizedSplit('\t', VcfCommon.InfoIndex + 2);
                    if (fields.Length <= VcfCommon.InfoIndex) continue;

                    string infoFields = fields[VcfCommon.InfoIndex];
                    string svEnd = GetSvEndString(infoFields);
                    if (svEnd == null) continue;

                    // Because 1K Genome SV has a padding base, the POS should add one to get the 1-based start position of the interval
                    // However, the start position need to minus one to become the 0-based start position in a BED file
                    // So the POS value can be used directly in the BED file.
                    writer.WriteLine(string.Join('\t', fields[VcfCommon.ChromIndex], fields[VcfCommon.PosIndex], svEnd, fields[VcfCommon.IdIndex], fields[VcfCommon.AltIndex], infoFields));
                }
            }

            return ExitCodes.Success;
        }

        private static string GetSvEndString(string infoFields)
        {
            if (infoFields == "" || infoFields == ".") return null;

            string endInfo = infoFields.OptimizedSplit(';').FirstOrDefault(x => x.StartsWith("END="));

            return string.IsNullOrEmpty(endInfo) ? null : endInfo.Substring(4);
        }
    }
}
