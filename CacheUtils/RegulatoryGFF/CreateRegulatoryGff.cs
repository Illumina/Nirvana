using System;
using System.IO;
using VariantAnnotation.DataStructures;
using VariantAnnotation.FileHandling;
using VariantAnnotation.Utilities;
using NDesk.Options;
using VariantAnnotation.CommandLine;
using VariantAnnotation.DataStructures.CompressedSequence;
using VariantAnnotation.FileHandling.TranscriptCache;

namespace CacheUtils.RegulatoryGFF
{
    sealed class CreateRegulatoryGff : AbstractCommandLineHandler
    {
        public static int Run(string command, string[] args)
        {
            var ops = new OptionSet
            {
                {
                    "in|i=",
                    "input cache {prefix}",
                    v => ConfigurationSettings.CachePrefix = v
                },
                {
                    "out|o=",
                    "output {file name}",
                    v => ConfigurationSettings.OutputFileName = v
                },
                {
                    "ref|r=",
                    "reference {file}",
                    v => ConfigurationSettings.CompressedReferencePath = v
                }
            };

            var commandLineExample = $"{command} --in <cache prefix> --out <GFF path>";

            var extractor = new CreateRegulatoryGff("Outputs regulatory regions in a database.", ops, commandLineExample, Constants.Authors);
            extractor.Execute(args);
            return extractor.ExitCode;
        }

        private CreateRegulatoryGff(string programDescription, OptionSet ops, string commandLineExample, string programAuthors, IVersionProvider versionProvider = null) : base(programDescription, ops, commandLineExample, programAuthors, versionProvider)
        {
        }

        protected override void ValidateCommandLine()
        {
            CheckInputFilenameExists(ConfigurationSettings.CompressedReferencePath, "compressed reference sequence", "--ref");
            HasRequiredParameter(ConfigurationSettings.CachePrefix, "input cache", "--in");
        }

        /// <summary>
        /// returns a datastore specified by the filepath
        /// </summary>
        private static GlobalCache GetCache(string cachePath)
        {
            if (!File.Exists(cachePath)) throw new FileNotFoundException($"Could not find {cachePath}");

            GlobalCache cache;
            using (var reader = new GlobalCacheReader(cachePath)) cache = reader.Read();
            return cache;
        }

        protected override void ProgramExecution()
        {
            var referenceNames = GetUcscReferenceNames(ConfigurationSettings.CompressedReferencePath);

            using (var writer = GZipUtilities.GetStreamWriter(ConfigurationSettings.OutputFileName))
            {
                var cachePath = CacheConstants.TranscriptPath(ConfigurationSettings.CachePrefix);

                // load the cache
                Console.Write("- reading {0}... ", Path.GetFileName(cachePath));
                var cache = GetCache(cachePath);
                Console.WriteLine("found {0:N0} regulatory regions. ", cache.RegulatoryElements.Length);

                Console.Write("- writing GFF entries... ");
                foreach (var regulatoryFeature in cache.RegulatoryElements)
                {
                    WriteRegulatoryFeature(writer, referenceNames, regulatoryFeature);
                }
                Console.WriteLine("finished.");
            }
        }

        private static string[] GetUcscReferenceNames(string compressedReferencePath)
        {
            string[] refNames;
            var compressedSequence = new CompressedSequence();

            using (var reader = new CompressedSequenceReader(FileUtilities.GetReadStream(compressedReferencePath), compressedSequence))
            {
                refNames = new string[reader.Metadata.Count];
                for (int refIndex = 0; refIndex < reader.Metadata.Count; refIndex++)
                {
                    refNames[refIndex] = reader.Metadata[refIndex].UcscName;
                }
            }

            return refNames;
        }

        private static void WriteRegulatoryFeature(StreamWriter writer, string[] referenceNames, RegulatoryElement regulatoryFeature)
        {
            writer.Write($"{referenceNames[regulatoryFeature.ReferenceIndex]}\t.\tregulatory feature\t{regulatoryFeature.Start}\t{regulatoryFeature.End}\t.\t.\t.\t");
            WriteGeneralAttributes(writer, regulatoryFeature);
            writer.WriteLine();
        }

        private static void WriteGeneralAttributes(StreamWriter writer, RegulatoryElement regulatoryFeature)
        {
            if (!regulatoryFeature.Id.IsEmpty) writer.Write($"regulatory_feature_id \"{regulatoryFeature.Id}\"; ");
            writer.Write($"regulatory_feature_type \"{regulatoryFeature.Type}\"; ");
        }
    }
}
