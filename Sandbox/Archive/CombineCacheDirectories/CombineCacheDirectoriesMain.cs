using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Illumina.DataDumperImport.Utilities;
using Illumina.ErrorHandling.Exceptions;
using Illumina.VariantAnnotation.DataStructures;
using Illumina.VariantAnnotation.FileHandling;
using Illumina.VariantAnnotation.FileHandling.SupplementaryAnnotations;
using Illumina.VariantAnnotation.Utilities;
using NDesk.Options;

namespace CombineCacheDirectories
{
    class CombineCacheDirectoriesMain : AbstractCommandLineHandler
    {
        #region members

        private static int _numEnsemblCacheDirs;
        private static int _numRefSeqCacheDirs;

        #endregion

        // constructor
        private CombineCacheDirectoriesMain(string programDescription, OptionSet ops, string commandLineExample, string programAuthors)
            : base(programDescription, ops, commandLineExample, programAuthors)
        { }

        private static void CheckCacheDirectory(CacheDirectory cacheDir)
        {
            switch (cacheDir.TranscriptDataSource)
            {
                case TranscriptDataSource.Ensembl:
                    _numEnsemblCacheDirs++;
                    break;
                case TranscriptDataSource.RefSeq:
                    _numRefSeqCacheDirs++;
                    break;
                default:
                    throw new ApplicationException($"Unexpected transcript data source ({cacheDir.TranscriptDataSource}) found in {cacheDir}");
            }
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

        /// <summary>
        /// returns the name of the current reference sequence
        /// </summary>
        private static NirvanaDatabaseHeader GetCurrentCache(NirvanaDataStore ds, NirvanaDataStore ds2)
        {
            return ds.CacheHeader ?? ds2.CacheHeader;
        }

        private static void CheckCacheIntegrity(NirvanaDataStore ds, NirvanaDataStore ds2)
        {
            // integrity checking is only necessary if we have two cache headers
            if ((ds.CacheHeader == null) || (ds2.CacheHeader == null)) return;

            var ch = ds.CacheHeader;
            var ch2 = ds2.CacheHeader;

            if (ch.GenomeAssembly != ch2.GenomeAssembly)
            {
                throw new UserErrorException($"Found a mismatch in genome assemblies: {ch.GenomeAssembly} vs {ch2.GenomeAssembly}");
            }

            if (ch.VepVersion != ch2.VepVersion)
            {
                throw new UserErrorException($"Found a mismatch in VEP versions: {ch.VepVersion} vs {ch2.VepVersion}");
            }

            if (ch.SchemaVersion != ch2.SchemaVersion)
            {
                throw new UserErrorException($"Found a mismatch in schema versions: {ch.SchemaVersion} vs {ch2.SchemaVersion}");
            }
        }

        /// <summary>
        /// validates the command line
        /// </summary>
        protected override void ValidateCommandLine()
        {
            CheckDirectoryExists(ConfigurationSettings.InputCacheDir, "input cache", "--in");
            CheckDirectoryExists(ConfigurationSettings.InputCacheDir2, "input cache 2", "--in2");
            CheckDirectoryExists(ConfigurationSettings.OutputCacheDir, "output cache", "--out");
        }

        /// <summary>
        /// executes the program
        /// </summary>
        protected override void ProgramExecution()
        {
            CacheDirectory cacheDirectory;
            CacheDirectory cacheDirectory2;

            var dataSourceVersions = new List<DataSourceVersion>();
            NirvanaDatabaseCommon.CheckDirectoryIntegrity(ConfigurationSettings.InputCacheDir, dataSourceVersions, out cacheDirectory);
            NirvanaDatabaseCommon.CheckDirectoryIntegrity(ConfigurationSettings.InputCacheDir2, dataSourceVersions, out cacheDirectory2);

            CheckCacheDirectory(cacheDirectory);
            CheckCacheDirectory(cacheDirectory2);

            if (cacheDirectory.SchemaVersion != cacheDirectory2.SchemaVersion)
            {
                throw new ApplicationException(
                    $"Expected both cache directories to have the same schema version. Cache directory 1: {cacheDirectory.SchemaVersion}, cache directory 2: {cacheDirectory2.SchemaVersion}");
            }

            if (cacheDirectory.VepVersion != cacheDirectory2.VepVersion)
            {
                throw new ApplicationException(
                    $"Expected both cache directories to have the same VEP version. Cache directory 1: {cacheDirectory.VepVersion}, cache directory 2: {cacheDirectory2.VepVersion}");
            }

            if ((_numEnsemblCacheDirs != 1) || (_numRefSeqCacheDirs != 1))
            {
                throw new ApplicationException(
                    $"Expected both of the supplied cache directories to have different transcript data sources. Cache directory 1: {cacheDirectory.TranscriptDataSource}, cache directory 2: {cacheDirectory2.TranscriptDataSource}");
            }

            var cache1Files = Directory.GetFiles(ConfigurationSettings.InputCacheDir, "*.ndb").Select(Path.GetFileName).ToList();
            var cache2Files = Directory.GetFiles(ConfigurationSettings.InputCacheDir2, "*.ndb").Select(Path.GetFileName).ToList();

            var allCacheFiles = new HashSet<string>();
            foreach (var cacheFile in cache1Files) allCacheFiles.Add(cacheFile);
            foreach (var cacheFile in cache2Files) allCacheFiles.Add(cacheFile);

            foreach (var filename in allCacheFiles)
            {
                var cache1Path = Path.Combine(ConfigurationSettings.InputCacheDir, filename);
                var cache2Path = Path.Combine(ConfigurationSettings.InputCacheDir2, filename);
                var outputPath = Path.Combine(ConfigurationSettings.OutputCacheDir, filename);

                Console.WriteLine("- reading {0}", filename);

                var inputDataStore = GetDataStore(cache1Path);
                var inputDataStore2 = GetDataStore(cache2Path);
                var outputDataStore = new NirvanaDataStore();

                CheckCacheIntegrity(inputDataStore, inputDataStore2);

                var currentCache = GetCurrentCache(inputDataStore, inputDataStore2);

                using (var writer = new NirvanaDatabaseWriter(outputPath))
                {
                    // combine the transcripts
                    var transcripts = new HashSet<Transcript>();
                    foreach (var transcript in inputDataStore.Transcripts) transcripts.Add(transcript);
                    foreach (var transcript in inputDataStore2.Transcripts) transcripts.Add(transcript);

                    outputDataStore.Transcripts = new List<Transcript>(transcripts.Count);
                    outputDataStore.Transcripts.AddRange(transcripts);

                    // combine the genes
                    var genes = new HashSet<Gene>();

                    if (inputDataStore.Genes != null)
                    {
                        foreach (var gene in inputDataStore.Genes) genes.Add(gene);
                    }

                    if (inputDataStore2.Genes != null)
                    {
                        foreach (var gene in inputDataStore2.Genes) genes.Add(gene);
                    }

                    outputDataStore.Genes = new List<Gene>(genes.Count);
                    outputDataStore.Genes.AddRange(genes);

                    // combine the regulatory features
                    var regulatoryFeatures = new HashSet<RegulatoryFeature>();
                    foreach (var regulatoryFeature in inputDataStore.RegulatoryFeatures) regulatoryFeatures.Add(regulatoryFeature);
                    foreach (var regulatoryFeature in inputDataStore2.RegulatoryFeatures) regulatoryFeatures.Add(regulatoryFeature);

                    outputDataStore.RegulatoryFeatures = new List<RegulatoryFeature>(regulatoryFeatures.Count);
                    outputDataStore.RegulatoryFeatures.AddRange(regulatoryFeatures);

                    // populate the transcript objects
                    DataStoreUtilities.PopulateTranscriptObjects(outputDataStore);

                    // write the Nirvana database file
                    outputDataStore.CacheHeader = new NirvanaDatabaseHeader(currentCache.ReferenceSequenceName,
                        DateTime.UtcNow.Ticks, currentCache.VepReleaseTicks, currentCache.VepVersion,
                        NirvanaDatabaseCommon.SchemaVersion, NirvanaDatabaseCommon.DataVersion,
                        currentCache.GenomeAssembly, TranscriptDataSource.BothRefSeqAndEnsembl);

                    Console.WriteLine("- writing {0} transcripts, {1} regulatory regions\n",
                        outputDataStore.Transcripts.Count, outputDataStore.RegulatoryFeatures.Count);

                    writer.Write(outputDataStore, currentCache.ReferenceSequenceName);
                }
            }
        }

        static int Main(string[] args)
        {
            var ops = new OptionSet
            {
                {
                    "in|1=",
                    "input cache {directory}",
                    v => ConfigurationSettings.InputCacheDir = v
                },
                {
                    "in2|2=",
                    "input cache 2 {directory}",
                    v => ConfigurationSettings.InputCacheDir2 = v
                },
                {
                    "out|o=",
                    "output cache {directory}",
                    v => ConfigurationSettings.OutputCacheDir = v
                }
            };

            var commandLineExample = "--in <cache directory> --in2 <cache directory> --out <cache directory>";

            var combiner = new CombineCacheDirectoriesMain("Combines two cache directories into one cache directory.", ops, commandLineExample, Constants.Authors);
            combiner.Execute(args);
            return combiner.ExitCode;
        }
    }
}
