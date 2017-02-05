using System;
using System.Collections.Generic;
using System.IO;
using CacheUtils.UpdateMiniCacheFiles;
using CacheUtils.UpdateMiniCacheFiles.DataStructures;
using CacheUtils.UpdateMiniCacheFiles.Updaters;
using CacheUtils.UpdateMiniCacheFiles.Utilities;
using ErrorHandling.Exceptions;
using NDesk.Options;
using VariantAnnotation.CommandLine;
using VariantAnnotation.DataStructures;
using VariantAnnotation.DataStructures.CompressedSequence;
using VariantAnnotation.FileHandling;
using VariantAnnotation.FileHandling.TranscriptCache;
using VariantAnnotation.Utilities;

namespace CacheUtils.ExtractRegulatoryElements
{
    sealed class ExtractRegulatoryElementsMain : AbstractCommandLineHandler
    {
        public static int Run(string command, string[] args)
        {
            var ops = new OptionSet
            {
                {
                    "in|i=",
                    "input Nirvana cache {file}",
                    v => ConfigurationSettings.InputPrefix = v
                },
                {
                    "out|o=",
                    "output {directory}",
                    v => ConfigurationSettings.OutputDirectory = v
                },
                {
                    "ref|r=",
                    "input reference {filename}",
                    v => ConfigurationSettings.InputReferencePath = v
                },
                {
                    "regulatory=",
                    "regulatory element {ID}",
                    v => ConfigurationSettings.RegulatoryElementId = v
                }
            };

            var commandLineExample = $"{command} --in <cache prefix> --out <cache dir> --regulatory <regulatory feature ID>";

            var extractor = new ExtractRegulatoryElementsMain("Extracts regulatory features from Nirvana cache files.", ops, commandLineExample, Constants.Authors);
            extractor.Execute(args);
            return extractor.ExitCode;
        }

        // constructor
        private ExtractRegulatoryElementsMain(string programDescription, OptionSet ops, string commandLineExample, string programAuthors)
            : base(programDescription, ops, commandLineExample, programAuthors)
        { }

        /// <summary>
        /// validates the command line
        /// </summary>
        protected override void ValidateCommandLine()
        {
            HasRequiredParameter(ConfigurationSettings.InputPrefix, "Nirvana cache", "--in");
            CheckInputFilenameExists(ConfigurationSettings.InputReferencePath, "compressed reference", "--ref");
            CheckDirectoryExists(ConfigurationSettings.OutputDirectory, "output cache", "--out");
            HasRequiredParameter(ConfigurationSettings.RegulatoryElementId, "regulatory ID", "--regulatory");
        }

        /// <summary>
        /// executes the program
        /// </summary>
        protected override void ProgramExecution()
        {
            var bundle = GetDataBundle(ConfigurationSettings.InputReferencePath, ConfigurationSettings.InputPrefix);
            var header = bundle.Cache.Header.Custom as GlobalCustomHeader;
            if (header == null) throw new InvalidCastException("Unable to cast the custom header as a GlobalCustomHeader");

            var regulatoryElement = MiniCacheUtilities.GetDesiredRegulatoryElement(bundle, ConfigurationSettings.RegulatoryElementId);
            if (regulatoryElement == null) throw new UserErrorException($"Unable to find the desired regulatory element: {ConfigurationSettings.RegulatoryElementId}");

            var updater = new RegulatoryUpdater(ConfigurationSettings.RegulatoryElementId,
                regulatoryElement.ReferenceIndex, bundle.Cache.Header.TranscriptSource.ToString());

            var outputFiles = new List<string>();
            var status = updater.Update(bundle, ConfigurationSettings.OutputDirectory, header.VepVersion, outputFiles);

            UnitTestResourceCrawler.CleanupFiles(status, new List<string>(), outputFiles);

            if (status != UpdateStatus.Current)
            {
                throw new UserErrorException($"Unable to create the mini-cache file. Status: {status}");
            }

            Console.WriteLine();
            Console.WriteLine("- created the following files:");
            foreach (var path in outputFiles) Console.WriteLine(Path.GetFileNameWithoutExtension(path));
        }

        private static DataBundle GetDataBundle(string compressedSequencePath, string cachePrefix)
        {
            Console.Write("- loading global cache and reference sequence... ");
            var sequence = new CompressedSequence();

            var bundle = new DataBundle
            {
                Sequence       = sequence,
                SequenceReader = new CompressedSequenceReader(FileUtilities.GetReadStream(compressedSequencePath), sequence),
                Cache          = CacheUtilities.LoadCache(cachePrefix)
            };

            Console.WriteLine("finished.");

            return bundle.Cache == null ? null : bundle;
        }
    }
}
