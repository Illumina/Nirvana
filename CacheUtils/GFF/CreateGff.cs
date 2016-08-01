using System;
using System.Collections.Generic;
using System.IO;
using NDesk.Options;
using VariantAnnotation.DataStructures;
using VariantAnnotation.FileHandling;
using VariantAnnotation.FileHandling.SupplementaryAnnotations;
using VariantAnnotation.Utilities;
using System.Linq;
using VariantAnnotation.CommandLine;

namespace CacheUtils.GFF
{
    public class CreateGff : AbstractCommandLineHandler
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

            var extractor = new CreateGff("Outputs exon coordinates for all transcripts in a database.", ops, commandLineExample, Constants.Authors);
            extractor.Execute(args);
            return extractor.ExitCode;
        }

        private CreateGff(string programDescription, OptionSet ops, string commandLineExample, string programAuthors,
            IVersionProvider versionProvider = null)
            : base(programDescription, ops, commandLineExample, programAuthors, versionProvider)
        {}

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
                    Console.Write("{0} transcripts. ", inputDataStore.Transcripts.Count);

                    Console.Write("Writing GFF entries... ");
                    foreach (var transcript in inputDataStore.Transcripts) Write(writer, ucscReferenceName, transcript);
                    Console.WriteLine("finished.");
                }
            }
        }

        private static void WriteTranscript(StreamWriter writer, string ucscReferenceName, Transcript transcript)
        {
            // write the general data
            var strand = transcript.OnReverseStrand ? '-' : '+';
            writer.Write($"{ucscReferenceName}\t{transcript.TranscriptDataSource}\ttranscript\t{transcript.Start}\t{transcript.End}\t.\t{strand}\t.\t");

            WriteGeneralAttributes(writer, transcript);
            writer.WriteLine();
        }

        private static void WriteGeneralAttributes(StreamWriter writer, Transcript transcript)
        {
            if (!string.IsNullOrEmpty(transcript.GeneStableId)) writer.Write($"gene_id \"{transcript.GeneStableId}\"; ");
            if (!string.IsNullOrEmpty(transcript.GeneSymbol)) writer.Write($"gene_name \"{transcript.GeneSymbol}\"; ");
            writer.Write($"gene_symbol_source \"{transcript.GeneSymbolSource}\"; ");

            if (!string.IsNullOrEmpty(transcript.StableId)) writer.Write($"transcript_id \"{transcript.StableId}\"; ");
            writer.Write($"transcript_type \"{transcript.BioType}\"; ");

            if (transcript.IsCanonical) writer.Write("tag \"canonical\"; ");

            if (!string.IsNullOrEmpty(transcript.ProteinId)) writer.Write($"protein_id \"{transcript.ProteinId}\"; ");
        }

        private static void WriteExon(StreamWriter writer, string ucscReferenceName, Transcript transcript, Exon exon, int exonIndex)
        {
            // write the general data
            var strand = transcript.OnReverseStrand ? '-' : '+';
            var phase = exon.Phase != 0 ? exon.Phase.ToString() : ".";
            writer.Write($"{ucscReferenceName}\t{transcript.TranscriptDataSource}\texon\t{exon.Start}\t{exon.End}\t.\t{strand}\t{phase}\t");

            WriteGeneralAttributes(writer, transcript);

            var exonNumber = transcript.OnReverseStrand ? transcript.Exons.Length - exonIndex : exonIndex + 1;
            writer.WriteLine($"exon_number {exonNumber};");
        }

        private static void Write(StreamWriter writer, string ucscReferenceName, Transcript transcript)
        {
            // write the transcript
            WriteTranscript(writer, ucscReferenceName, transcript);

            // write all of the exons
            for (int exonIndex = 0; exonIndex < transcript.Exons.Length; exonIndex++)
                WriteExon(writer, ucscReferenceName, transcript, transcript.Exons[exonIndex], exonIndex);
        }
    }
}
