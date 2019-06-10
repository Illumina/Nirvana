using System;
using System.Collections.Generic;
using System.IO;
using CommandLine.Builders;
using CommandLine.NDesk.Options;
using Compression.Utilities;
using ErrorHandling;
using ErrorHandling.Exceptions;
using IO;
using Jasix.DataStructures;
using VariantAnnotation.Interface;

namespace Jasix
{
    public static class JasixMain 
    {
        private static string _inputJson;
        private static string _outputFile;
        private static readonly List<string> Queries = new List<string>();
        private static bool _printHeader;
        private static bool _printHeaderOnly;
        private static bool _listChromosomeNames;
        private static bool _createIndex;

        public static int Main(string[] args)
        {
            var ops = new OptionSet
            {
                {
                    "header|t",
                    "print also the header lines",
                    v => _printHeader = v != null
                },
                {
                    "only-header|H",
                    "print only the header lines",
                    v => _printHeaderOnly = v != null
                },
                {
                    "chromosomes|l",
                    "list chromosome names",
                    v => _listChromosomeNames = v != null
                },
                {
                    "index|c",
                    "create index",
                    v => _createIndex = v != null
                },
                {
                    "in|i=",
                    "input",
                    v => _inputJson = v
                },
                {
                    "out|o=",
                    "compressed output file name (default:console)",
                    v => _outputFile = v
                },
                {
                    "query|q=",
                    "query range",
                    v => Queries.Add(v)
                }
            };

            var exitCode = new ConsoleAppBuilder(args, ops)
                .Parse()
                .CheckInputFilenameExists(_inputJson, "input Json file", "[in.json.gz]")
                .DisableOutput(!_createIndex && _outputFile == null)
                .ShowBanner(Constants.Authors)
                .ShowHelpMenu("Indexes a Nirvana annotated JSON file", "-i in.json.gz [options]")
                .ShowErrors()
                .Execute(ProgramExecution);

            return (int)exitCode;
        }

        private static ExitCodes ProgramExecution()
        {
            if (_createIndex)
            {
                using (var indexCreator = new IndexCreator(_inputJson))
                {
                    indexCreator.CreateIndex();
                }

                return ExitCodes.Success;
            }

            string indexFileName = _inputJson + JasixCommons.FileExt;

            ValidateIndexFile(indexFileName);
            var writer = string.IsNullOrEmpty(_outputFile)
                ? null : GZipUtilities.GetStreamWriter(_outputFile);

            using (var queryProcessor = new QueryProcessor(GZipUtilities.GetAppropriateStreamReader(_inputJson),
                    FileUtilities.GetReadStream(indexFileName), writer))
            {
                if (_listChromosomeNames)
                {
                    queryProcessor.PrintChromosomeList();
                    return ExitCodes.Success;
                }

                if (_printHeaderOnly)
                {
                    queryProcessor.PrintHeaderOnly();
                    return ExitCodes.Success;
                }

                if (Queries == null)
                {
                    Console.WriteLine("Please specify query region(s)");
                    return ExitCodes.BadArguments;
                }
                
                queryProcessor.ProcessQuery(Queries, _printHeader);
                
            }
            return ExitCodes.Success;
        }

        private static void ValidateIndexFile(string indexFileName)
        {
            if (!File.Exists(indexFileName))
                throw new UserErrorException("No index file found,please generate index file first.");
            //var indexFileCreateTime = File.GetCreationTime(indexFileName).Ticks;
            //var fileCreateTime = File.GetCreationTime(_inputJson).Ticks;
            //if (fileCreateTime > indexFileCreateTime - 1000) // adding a 100ms buffer
            //    throw new UserErrorException("Index file is older than the input file, please re-generate the index.");
        }
        
    }
}