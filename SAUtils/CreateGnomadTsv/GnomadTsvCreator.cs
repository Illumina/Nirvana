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

        public GnomadTsvCreator(StreamReader[] streamReaders, ReferenceSequenceProvider refProvider, DataSourceVersion version, string outputDirectory)
        {
            _version          = version;
            _refProvider      = refProvider;
            _outputDirectory  = outputDirectory;
            _streamReaders    = streamReaders;
        }

        public void CreateTsvs()
        {
            var benchMark = new Benchmark();

            using (var writer = new GnomadTsvWriter(_version, _outputDirectory, _refProvider.GenomeAssembly, _refProvider))
            {
                var count = 0;
                foreach (var fileStreamReader in _streamReaders)
                {
                    var exacReader = new GnomadReader(fileStreamReader, _refProvider.GetChromosomeDictionary());
                    TsvWriterUtilities.WriteSortedItems(exacReader.GetEnumerator(), writer);
                    Console.WriteLine($"ingested {count++} file in " + benchMark.GetElapsedTime());
                }
                
            }

            var timeSpan = Benchmark.ToHumanReadable(benchMark.GetElapsedTime());
            TsvWriterUtilities.WriteCompleteInfo("gnomAD", _version.Version, timeSpan);
        }
    }
}