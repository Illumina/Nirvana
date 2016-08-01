using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NDesk.Options;
using VariantAnnotation.CommandLine;
using VariantAnnotation.DataStructures;
using VariantAnnotation.FileHandling;
using VariantAnnotation.FileHandling.SupplementaryAnnotations;
using VariantAnnotation.Utilities;

namespace CacheUtils.RegulatoryGFF
{
    public class CreateRegulatoryGff : AbstractCommandLineHandler
    {
        public static int Run(string command, string[] args)
        {
            var ops = new OptionSet
            {
                {
                    "in|i=",
                    "input cache {directory}",
                    v => ConfigurationSettings.InputCacheDir = v
                },
                {
                    "out|o=",
                    "output {file name}",
                    v => ConfigurationSettings.OutputFileName = v
                }
            };

            var commandLineExample = $"{command} --in <cache dir> --out <GFF path>";

            var extractor = new CreateRegulatoryGff("Outputs regulatory regions in a database.", ops, commandLineExample, Constants.Authors);
            extractor.Execute(args);
            return extractor.ExitCode;
        }

        private CreateRegulatoryGff(string programDescription, OptionSet ops, string commandLineExample, string programAuthors, IVersionProvider versionProvider = null) : base(programDescription, ops, commandLineExample, programAuthors, versionProvider)
        {
        }

        protected override void ValidateCommandLine()
        {
            CheckDirectoryExists(ConfigurationSettings.InputCacheDir, "input cache", "--in");
        }

        /// <summary>
        /// returns a datastore specified by the filepath
        /// </summary>
        private static NirvanaDataStore GetDataStore(string dataStorePath)
        {
            var ds = new NirvanaDataStore
            {
                Transcripts = new List<Transcript>(),
                RegulatoryFeatures = new List<RegulatoryFeature>()
            };

            // sanity check: make sure the path exists
            if (!File.Exists(dataStorePath)) return ds;

            var transcriptIntervalTree = new IntervalTree<Transcript>();

            using (var reader = new NirvanaDatabaseReader(dataStorePath))
            {
                reader.PopulateData(ds, transcriptIntervalTree);
            }

            return ds;
        }

        protected override void ProgramExecution()
        {
            // check the cache directory integrity
            CacheDirectory cacheDirectory;
            var dataSourceVersions = new List<DataSourceVersion>();
            NirvanaDatabaseCommon.CheckDirectoryIntegrity(ConfigurationSettings.InputCacheDir, dataSourceVersions, out cacheDirectory);

            // grab the cache files
            var cacheFiles = Directory.GetFiles(ConfigurationSettings.InputCacheDir, "*.ndb").Select(Path.GetFileName).ToList();

            using (var writer = GZipUtilities.GetStreamWriter(ConfigurationSettings.OutputFileName))
            {
                foreach (var cacheFile in cacheFiles)
                {
                    var cachePath = Path.Combine(ConfigurationSettings.InputCacheDir, cacheFile);
                    var ucscReferenceName = Path.GetFileNameWithoutExtension(cacheFile);

                    Console.Write("- reading {0}... ", cacheFile);

                    // load the datastore
                    var inputDataStore = GetDataStore(cachePath);
                    Console.Write("{0} regulatory regions. ", inputDataStore.RegulatoryFeatures.Count);

                    Console.Write("Writing GFF entries... ");
                    foreach (var regulatoryFeature in inputDataStore.RegulatoryFeatures) Write(writer, ucscReferenceName, regulatoryFeature);
                    Console.WriteLine("finished.");
                }
            }
        }

        private static void WriteRegulatoryFeature(StreamWriter writer, string ucscReferenceName, RegulatoryFeature regulatoryFeature)
        {
            // write the general data
            writer.Write($"{ucscReferenceName}\t.\tregulatory feature\t{regulatoryFeature.Start}\t{regulatoryFeature.End}\t.\t.\t.\t");

            WriteGeneralAttributes(writer, regulatoryFeature);
            writer.WriteLine();
        }

        private static void WriteGeneralAttributes(StreamWriter writer, RegulatoryFeature regulatoryFeature)
        {
            if (!string.IsNullOrEmpty(regulatoryFeature.StableId)) writer.Write($"regulatory_feature_id \"{regulatoryFeature.StableId}\"; ");
        }

        private static void Write(StreamWriter writer, string ucscReferenceName, RegulatoryFeature regulatoryFeature)
        {
            // write the regulatory feature
            WriteRegulatoryFeature(writer, ucscReferenceName, regulatoryFeature);
        }
    }
}
