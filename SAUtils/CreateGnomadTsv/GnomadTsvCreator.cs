using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using CommandLine.Utilities;
using Compression.FileHandling;
using Compression.Utilities;
using SAUtils.DataStructures;
using SAUtils.InputFileParsers;
using SAUtils.TsvWriters;
using VariantAnnotation.Providers;
using VariantAnnotation.Utilities;

namespace SAUtils.CreateGnomadTsv
{
    public sealed class GnomadTsvCreator
    {
        private readonly string[] _filePaths;
        private readonly ReferenceSequenceProvider _refSeqProvider;
        private readonly DataSourceVersion _version;
        private readonly string _outputDirectory;
        private readonly string _sequencingDataType;
        private const string HeaderFileName = "header.txt.gz";

        private readonly Dictionary<string, string> _jsonKeyDictionary = new Dictionary<string, string>
        {
            {"genome", InterimSaCommon.GnomadTag },
            {"exome", InterimSaCommon.GnomadExomeTag }
        };

        public GnomadTsvCreator(string[] filePaths, ReferenceSequenceProvider refSeqProvider,
            DataSourceVersion version, string outputDirectory, string sequencingDataType)
        {
            _filePaths          = filePaths;
            _version            = version;
            _refSeqProvider        = refSeqProvider;
            _outputDirectory    = outputDirectory;
            _sequencingDataType = sequencingDataType;
        }

        public void CreateTsvsParallel()
        {
            var benchMark = new Benchmark();

            if (_filePaths.Length > 1)
            {
                //create the header file
                using (var headerWriter =
                    new StreamWriter(new BlockGZipStream(FileUtilities.GetCreateStream(Path.Combine(_outputDirectory, HeaderFileName)),
                        CompressionMode.Compress)))
                {
                    var header = SaTsvWriter.GetHeader(_version, SaTsvCommon.SchemaVersion, _refSeqProvider.GenomeAssembly.ToString(),
                        "gnomad", null, true, false);
                    headerWriter.Write(header);
                }
            }

            Parallel.ForEach(_filePaths, new ParallelOptions { MaxDegreeOfParallelism = 4 }, CreateTsvFrom);

            Console.WriteLine("Merging header and chromosome tsvs..");
            var jsonKey = _jsonKeyDictionary[_sequencingDataType];
            var fileName = jsonKey + "_" + _version.Version.Replace(" ", "_") + ".tsv.gz";
            var filePath = Path.Combine(_outputDirectory, fileName);

            SaUtilsCommon.CombineFiles(_outputDirectory, HeaderFileName, "chr*.tsv.gz",  filePath, true);

            Console.WriteLine("Creating tsv index...");
            SaUtilsCommon.BuildTsvIndex(filePath);

            var timeSpan = Benchmark.ToHumanReadable(benchMark.GetElapsedTime());
            TsvWriterUtilities.WriteCompleteInfo("gnomAD", _version.Version, timeSpan);
        }

        
        private void CreateTsvFrom(string filePath)
        {
            var benchmark = new Benchmark();
            var chrom = GetChromName(filePath);
            ISaItemTsvWriter writer;
            if (chrom == "all")
                writer = new GnomadTsvWriter(_version, _outputDirectory, _refSeqProvider.GenomeAssembly, _refSeqProvider,
                    _sequencingDataType); 
            else
                writer = new LiteGnomadTsvWriter(Path.Combine(_outputDirectory, chrom + ".tsv.gz"), _refSeqProvider);

            Console.WriteLine("Starting to parse "+chrom);
            using(var tsvReader = GZipUtilities.GetAppropriateStreamReader(filePath))
            using (writer)
            {
                var reader = new GnomadReader(tsvReader, _refSeqProvider.RefNameToChromosome);
                TsvWriterUtilities.WriteSortedItems(reader.GetGnomadItems(), writer);
            }

            var timeSpan = Benchmark.ToHumanReadable(benchmark.GetElapsedTime());
            Console.WriteLine("Completed chrom " + chrom + " in:"+ timeSpan);

        }

        private string GetChromName(string filePath)
        {
            var pathSplits = filePath.Split(Path.PathSeparator);
            var fileName = pathSplits[pathSplits.Length - 1];

            var nameSplits = fileName.Split('.');

            foreach (var split in nameSplits)
            {
                if (split.StartsWith("chr")) return split;
            }

            return "all";
        }
    }
}