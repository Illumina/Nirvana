using System;
using System.IO;
using CommandLine.Utilities;
using SAUtils.InputFileParsers;
using SAUtils.TsvWriters;
using VariantAnnotation.Providers;

namespace SAUtils.CreateGnomadTsv
{
    public sealed class GnomadTsvCreator
    {
        private readonly StreamReader[] _streamReaders;
        private readonly ReferenceSequenceProvider _refProvider;
        private readonly DataSourceVersion _version;
        private readonly string _outputDirectory;
        private readonly string _sequencingDataType;

        public GnomadTsvCreator(StreamReader[] streamReaders, ReferenceSequenceProvider refProvider,
            DataSourceVersion version, string outputDirectory, string sequencingDataType)
        {
            _version            = version;
            _refProvider        = refProvider;
            _outputDirectory    = outputDirectory;
            _streamReaders      = streamReaders;
            _sequencingDataType = sequencingDataType;
        }

        public void CreateTsvs()
        {
            var benchMark = new Benchmark();

            using (var writer = new GnomadTsvWriter(_version, _outputDirectory, _refProvider.GenomeAssembly, _refProvider, _sequencingDataType))
            {
                var count = 0;

                foreach (var fileStreamReader in _streamReaders)
                {
                    var reader = new GnomadReader(fileStreamReader, _refProvider.RefNameToChromosome);
                    TsvWriterUtilities.WriteSortedItems(reader.GetGnomadItems(), writer);
                    Console.WriteLine($"ingested {count++} file in " + benchMark.GetElapsedTime());
                }
            }

            var timeSpan = Benchmark.ToHumanReadable(benchMark.GetElapsedTime());
            TsvWriterUtilities.WriteCompleteInfo("gnomAD", _version.Version, timeSpan);
        }
    }
}