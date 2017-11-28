using System.Collections.Generic;
using System.IO;
using System.Linq;
using CacheUtils.Commands.UniversalGeneArchive;
using CacheUtils.Genbank;
using CacheUtils.IntermediateIO;
using CacheUtils.Utilities;
using CommandLine.Builders;
using CommandLine.NDesk.Options;
using Compression.Utilities;
using ErrorHandling;
using VariantAnnotation.Interface;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.Logger;
using VariantAnnotation.Providers;
using VariantAnnotation.Utilities;

namespace CacheUtils.Commands.ParseGenbank
{
    public static class ParseGenbankMain
    {
        private static string _outputGenbankPath;

        private static ExitCodes ProgramExecution()
        {
            var logger = new ConsoleLogger();
            if (!_outputGenbankPath.EndsWith(".gz")) _outputGenbankPath += ".gz";

            var genbankFiles = GetGenbankFiles(logger);
            genbankFiles.Execute(logger, "downloads",    file => file.Download());
            genbankFiles.Execute(logger, "file parsing", file => file.Parse());

            var genbankEntries = GetIdToGenbankEntryDict(genbankFiles);
            WriteDictionary(logger, genbankEntries);

            return ExitCodes.Success;
        }

        private static List<GenbankFile> GetGenbankFiles(ILogger logger)
        {
            int numGenbankFiles = GetNumGenbankFiles(logger);
            var genbankFiles    = new List<GenbankFile>(numGenbankFiles);

            for (int i = 0; i < numGenbankFiles; i++) genbankFiles.Add(new GenbankFile(logger, i + 1));
            return genbankFiles;
        }

        private static IEnumerable<GenbankEntry> GetIdToGenbankEntryDict(IEnumerable<GenbankFile> files) =>
            files.SelectMany(file => file.GenbankDict.Values).OrderBy(x => x.TranscriptId).ToList();

        private static int GetNumGenbankFiles(ILogger logger)
        {
            var fileList = new RemoteFile("RefSeq filelist", "ftp://ftp.ncbi.nlm.nih.gov/refseq/H_sapiens/mRNA_Prot/human.files.installed");
            fileList.Download(logger);

            int maxNum = 0;

            using (var reader = new StreamReader(FileUtilities.GetReadStream(fileList.FilePath)))
            {
                while (true)
                {
                    var line = reader.ReadLine();
                    if (line == null) break;

                    var filename = line.Split('\t')[1];
                    if (!filename.EndsWith(".rna.gbff.gz")) continue;

                    int num = int.Parse(filename.Substring(6, filename.Length - 18));
                    if (num > maxNum) maxNum = num;
                }
            }

            return maxNum;
        }

        private static void WriteDictionary(ILogger logger, IEnumerable<GenbankEntry> entries)
        {
            var header = new IntermediateIoHeader(0, 0, Source.None, GenomeAssembly.Unknown, 0);

            logger.Write($"- writing output file ({Path.GetFileName(_outputGenbankPath)})... ");
            using (var writer = new GenbankWriter(GZipUtilities.GetStreamWriter(_outputGenbankPath), header))
            {
                foreach (var entry in entries) writer.Write(entry);
            }
            logger.WriteLine("finished.");
        }

        public static ExitCodes Run(string command, string[] args)
        {
            var ops = new OptionSet
            {
                {
                    "out|o=",
                    "output intermediate Genbank {path}",
                    v => _outputGenbankPath = v
                }
            };

            var commandLineExample = $"{command} --out <intermediate Genbank path>";

            return new ConsoleAppBuilder(args, ops)
                .UseVersionProvider(new VersionProvider())
                .Parse()
                .HasRequiredParameter(_outputGenbankPath, "output Genbank path", "--out")
                .SkipBanner()
                .ShowHelpMenu("Parses Genbank data to allow other CacheUtils tools to access the data", commandLineExample)
                .ShowErrors()
                .Execute(ProgramExecution);
        }
    }
}
