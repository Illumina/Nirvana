using System;
using System.IO;
using System.Linq;
using CommandLine.NDesk.Options;
using CommandLine.VersionProvider;
using ErrorHandling.Exceptions;
using Jasix.DataStructures;
using VariantAnnotation.DataStructures;
using VariantAnnotation.FileHandling.Compression;
using VariantAnnotation.Utilities;

namespace Jasix
{
    public class Jasix : PositionalCommandLineHandler
    {
        public Jasix(string programDescription, OptionSet ops, string commandLineExample, string programAuthors,
            IVersionProvider versionProvider = null)
            : base(programDescription, ops, commandLineExample, programAuthors, versionProvider)
        {}

        protected override void ValidateCommandLine()
        {
            CheckInputFilenameExists(ConfigurationSettings.InputJson, "input Json file", "[in.json.gz]");
        }

        protected override void ProgramExecution()
        {
            var indexFileName = ConfigurationSettings.InputJson + JasixCommons.FileExt;
            if (!ConfigurationSettings.ListChromosomeName && !ConfigurationSettings.PrintHeader &&
                !ConfigurationSettings.PrintHeaderOnly && string.IsNullOrEmpty(ConfigurationSettings.Query))
            {
                if (File.Exists(indexFileName) && !ConfigurationSettings.OverWriteIndex)
                    throw new UserErrorException("Index File already exist, please run jasix.exe -f [in.json.gz] to generate new index file ");

                using (var indexCreator = new IndexCreator(ConfigurationSettings.InputJson))
                {
                    indexCreator.CreateIndex();
                }

                return;
            }

            ValidateIndexFile(indexFileName);

            using (
                var queryProcessor = new QueryProcessor(GZipUtilities.GetAppropriateStreamReader(ConfigurationSettings.InputJson),
                    FileUtilities.GetReadStream(indexFileName)))
            {
                if (ConfigurationSettings.ListChromosomeName)
                {
                    queryProcessor.PrintChromosomeList();
                    return;
                }

                if (ConfigurationSettings.PrintHeaderOnly)
                {
                    queryProcessor.PrintHeader();
                    return;
                }

                if (ConfigurationSettings.Query == null)
                {
                    Console.WriteLine("Plese specify query region");
                    return;
                }

                queryProcessor.ProcessQuery(ConfigurationSettings.Query, ConfigurationSettings.PrintHeader);
            }
        }

        private static void ValidateIndexFile(string indexFileName)
        {
            if (!File.Exists(indexFileName))
                throw new UserErrorException("No index file found,please run jasix.exe  [in.json.gz] to generate index file ");
            var indexFileCreateTime = File.GetCreationTime(indexFileName);
            var fileCreateTime = File.GetCreationTime(ConfigurationSettings.InputJson);
            if (fileCreateTime > indexFileCreateTime)
                throw new UserErrorException("Index file is created before json file, please run jasix.exe -f [in.json.gz] to generate new index file");
        }

        public static int Main(string[] args)
        {
            var ops = new OptionSet
            {
                {
                    "h",
                    "print also the header lines",
                    v => ConfigurationSettings.PrintHeader = v != null
                },
                {
                    "H",
                    "print only the header lines",
                    v => ConfigurationSettings.PrintHeaderOnly = v != null
                },
                {
                    "l",
                    "list chromosome names",
                    v => ConfigurationSettings.ListChromosomeName = v != null
                },
                {
                    "f",
                    "force to overwrite the index",
                    v => ConfigurationSettings.OverWriteIndex = v != null
                }
            };

            var commandLineExample = "<in.json.gz> [region1 [region2 [...]]]";

            string[] updatedArgs = args;
            var jasix = new Jasix("Json file indexer", ops, commandLineExample, Constants.Authors, new JasixVersionProvider());
            jasix.Execute(updatedArgs);

            return 0;
        }

        protected override void ParseArguments(string[] args, out string[] updatedArgs, out int positionalArgsCount)
        {
            positionalArgsCount = 0;

            foreach (var arg in args)
            {
                if (arg.StartsWith("-")) continue;
                if (ConfigurationSettings.InputJson == null)
                {
                    ConfigurationSettings.InputJson = arg;
                    positionalArgsCount++;
                    continue;
                }
                if (ConfigurationSettings.Query == null)
                {
                    positionalArgsCount++;
                    ConfigurationSettings.Query = arg;
                }
            }

            var argslist = args.ToList();
            argslist.RemoveAll(x => x == ConfigurationSettings.InputJson || x == ConfigurationSettings.Query);
            updatedArgs = argslist.ToArray();
        }
    }
}