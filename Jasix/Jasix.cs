using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine.Builders;
using CommandLine.NDesk.Options;
using Compression.Utilities;
using ErrorHandling;
using ErrorHandling.Exceptions;
using Jasix.DataStructures;
using VariantAnnotation.Interface;
using VariantAnnotation.Utilities;

namespace Jasix
{
    public sealed class Jasix 
    {
        private ExitCodes ProgramExecution()
        {
            if (ConfigurationSettings.CreateIndex)
            {
                using (var indexCreator = new IndexCreator(ConfigurationSettings.InputJson))
                {
                    indexCreator.CreateIndex();
                }

                return ExitCodes.Success;
            }

            var indexFileName = ConfigurationSettings.InputJson + JasixCommons.FileExt;

            ValidateIndexFile(indexFileName);

            using (var queryProcessor = new QueryProcessor(GZipUtilities.GetAppropriateStreamReader(ConfigurationSettings.InputJson),
                    FileUtilities.GetReadStream(indexFileName)))
            {
                if (ConfigurationSettings.ListChromosomeName)
                {
                    queryProcessor.PrintChromosomeList();
                    return ExitCodes.Success;
                }

                if (ConfigurationSettings.PrintHeaderOnly)
                {
                    queryProcessor.PrintHeader();
                    return ExitCodes.Success;
                }

                if (ConfigurationSettings.Queries == null)
                {
                    Console.WriteLine("Plese specify query region");
                    return ExitCodes.BadArguments;
                }
                foreach (var query in ConfigurationSettings.Queries)
                {
                    queryProcessor.ProcessQuery(query, ConfigurationSettings.PrintHeader);
                }
                
            }
            return ExitCodes.Success;
        }

        private static void ValidateIndexFile(string indexFileName)
        {
            if (!File.Exists(indexFileName))
                throw new UserErrorException("No index file found,please generate index file first.");
            var indexFileCreateTime = File.GetCreationTime(indexFileName);
            var fileCreateTime = File.GetCreationTime(ConfigurationSettings.InputJson);
            if (fileCreateTime > indexFileCreateTime)
                throw new UserErrorException("Index file is older than the input file, please re-generate the index.");
        }

        public static int Main(string[] args)
        {
            var ops = new OptionSet
            {
                {
                    "header|t",
                    "print also the header lines",
                    v => ConfigurationSettings.PrintHeader = v != null
                },
                {
                    "only-header|H",
                    "print only the header lines",
                    v => ConfigurationSettings.PrintHeaderOnly = v != null
                },
                {
                    "chromosomes|l",
                    "list chromosome names",
                    v => ConfigurationSettings.ListChromosomeName = v != null
                },
                {
                    "index|c",
                    "create index",
                    v => ConfigurationSettings.CreateIndex = v != null
                },
                {
                    "in|i=",
                    "input",
                    v => ConfigurationSettings.InputJson = v
                },
                {
                    "out|o=",
                    "output(default:console)",
                    v => ConfigurationSettings.OutputFile = v
                },
                {
                    "query|q=",
                    "query range",
                    v => ConfigurationSettings.Queries.Add(v)
                }

            };

            var jasix = new Jasix();
            var exitCode = new ConsoleAppBuilder(args, ops)
                .UseVersionProvider(new JasixVersionProvider())
                .Parse()
                .CheckInputFilenameExists(ConfigurationSettings.InputJson, "input Json file", "[in.json.gz]")
                .DisableOutput(!ConfigurationSettings.CreateIndex && ConfigurationSettings.OutputFile == null)
                .ShowBanner(Constants.Authors)
                .ShowHelpMenu("Indexes a Nirvana annotated JSON file", "-i in.json.gz [options]")
                .ShowErrors()
                .Execute(jasix.ProgramExecution);

            return (int)exitCode;
        }
        
    }
}