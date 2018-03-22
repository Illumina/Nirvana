using System.IO;
using CommandLine.Utilities;
using SAUtils.InputFileParsers.TOPMed;
using SAUtils.TsvWriters;
using VariantAnnotation.Providers;

namespace SAUtils.CreateTopMedTsv
{
    public sealed class TopMedTsvCreator
    {
        private readonly StreamReader _streamReader;
        private readonly ReferenceSequenceProvider _refProvider;
        private readonly DataSourceVersion _version;
        private readonly string _outputDirName;

        public TopMedTsvCreator(StreamReader streamReader, ReferenceSequenceProvider refProvider,
            DataSourceVersion version, string outputDirName)
        {
            _version         = version;
            _refProvider     = refProvider;
            _streamReader    = streamReader;
            _outputDirName   = outputDirName;
        }

        public void CreateTsvs()
        {
            var benchMark = new Benchmark();

            using (var writer = new TopMedTsvWriter(_version, _outputDirName, _refProvider.GenomeAssembly, _refProvider))
            using (var reader = new TopMedReader(_streamReader, _refProvider.RefNameToChromosome))
            {
                TsvWriterUtilities.WriteSortedItems(reader.GetGnomadItems(), writer);
            }

            var timeSpan = Benchmark.ToHumanReadable(benchMark.GetElapsedTime());
            TsvWriterUtilities.WriteCompleteInfo("TOPMed", _version.Version, timeSpan);
        }
    }
}