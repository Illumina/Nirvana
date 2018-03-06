using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommandLine.Utilities;
using Compression.Utilities;
using SAUtils.DataStructures;
using SAUtils.InputFileParsers;
using SAUtils.InputFileParsers.ClinGen;
using SAUtils.InputFileParsers.ClinVar;
using SAUtils.InputFileParsers.Cosmic;
using SAUtils.InputFileParsers.CustomAnnotation;
using SAUtils.InputFileParsers.CustomInterval;
using SAUtils.InputFileParsers.DbSnp;
using SAUtils.InputFileParsers.DGV;
using SAUtils.InputFileParsers.EVS;
using SAUtils.InputFileParsers.ExAc;
using SAUtils.InputFileParsers.MitoMap;
using SAUtils.InputFileParsers.OneKGen;
using SAUtils.TsvWriters;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.Providers;
using VariantAnnotation.Sequence;
using VariantAnnotation.Utilities;

namespace SAUtils.CreateIntermediateTsvs
{
    internal sealed class CreateIntermediateTsvs
    {
        #region fileNames
        private readonly List<string> _customAnnotationFiles;
        private readonly List<string> _customIntervalFiles;
        private readonly string _onekGFileName;
        private readonly string _onekGSvFileName;
        private readonly string _clinGenFileName;
        private readonly string _clinVarFileName;
        private readonly string _cosmicTsvFileName;
        private readonly string _cosmicVcfFileName;
        private readonly string _dbSnpFileName;
        private readonly string _dgvFile;
        private readonly string _evsFile;
        private readonly string _exacFile;
        private readonly List<string> _mitoMapVarFileNames;
        private readonly List<string> _mitoMapSvFileNames;
        private readonly string _outputDirectory;
        #endregion

        #region members
        private readonly IDictionary<string, IChromosome> _refNamesDictionary;
        private readonly GenomeAssembly _genomeAssembly;
        //private readonly ISequenceProvider _sequenceProvider;
        private readonly string _compressedReferencePath;

        #endregion
        public CreateIntermediateTsvs(string compressedReferencePath, string outputDirectory, string dbSnpFileName, string cosmicVcfFileName, string cosmicTsvFileName, string clinVarFileName, string onekGFileName, string evsFile, string exacFile, string dgvFile, string onekGSvFileName, string clinGenFileName, List<string> mitoMapVarFileNames, List<string> mitoMapSvFileNames, List<string> customAnnotationFiles, List<string> customIntervalFiles)
        {
            _outputDirectory = outputDirectory;
            _dbSnpFileName = dbSnpFileName;
            _cosmicVcfFileName = cosmicVcfFileName;
            _cosmicTsvFileName = cosmicTsvFileName;
            _clinVarFileName = clinVarFileName;
            _onekGFileName = onekGFileName;
            _evsFile = evsFile;
            _exacFile = exacFile;
            _customAnnotationFiles = customAnnotationFiles;
            _dgvFile = dgvFile;
            _onekGSvFileName = onekGSvFileName;
            _clinGenFileName = clinGenFileName;
            _mitoMapVarFileNames = mitoMapVarFileNames;
            _mitoMapSvFileNames = mitoMapSvFileNames;
            _customIntervalFiles = customIntervalFiles;
            _compressedReferencePath = compressedReferencePath;
            var sequenceProvider = new ReferenceSequenceProvider(FileUtilities.GetReadStream(_compressedReferencePath));
            _refNamesDictionary = sequenceProvider.RefNameToChromosome;
            _genomeAssembly = sequenceProvider.GenomeAssembly;

        }

        public void CreateTsvs()
        {
            //CreateDbsnpGaTsv(_dbSnpFileName);
            //CreateOnekgTsv(_onekGFileName);
            //CreateClinvarTsv(_clinVarFileName);
            //CreateExacTsv(_exacFile);
            //CreateEvsTsv(_evsFile);
            //CreateCosmicTsv(_cosmicVcfFileName, _cosmicTsvFileName);
            //CreateSvTsv(InterimSaCommon.DgvTag, _dgvFile);
            //CreateSvTsv(InterimSaCommon.ClinGenTag, _clinGenFileName);
            //CreateSvTsv(InterimSaCommon.OnekSvTag, _onekGSvFileName);
            //ThreadPool.SetMaxThreads(Environment.ProcessorCount, Environment.ProcessorCount);

            var tasks = new List<Task>
            {
                Task.Factory.StartNew(() => CreateDbsnpGaTsv(_dbSnpFileName)),
                Task.Factory.StartNew(() => CreateOnekgTsv(_onekGFileName)),
                Task.Factory.StartNew(() => CreateClinvarTsv(_clinVarFileName)),
                Task.Factory.StartNew(() => CreateExacTsv(_exacFile)),
                Task.Factory.StartNew(() => CreateEvsTsv(_evsFile)),
                Task.Factory.StartNew(() => CreateCosmicTsv(_cosmicVcfFileName, _cosmicTsvFileName)),
                Task.Factory.StartNew(() => CreateSvTsv(InterimSaCommon.DgvTag, _dgvFile)),
                Task.Factory.StartNew(() => CreateSvTsv(InterimSaCommon.ClinGenTag, _clinGenFileName)),
                Task.Factory.StartNew(() => CreateSvTsv(InterimSaCommon.OnekSvTag, _onekGSvFileName)),
                Task.Factory.StartNew(() => CreateMitoMapVarTsv(_mitoMapVarFileNames)),
                Task.Factory.StartNew(() => CreateMitoMapSvTsv(_mitoMapSvFileNames))
            };

            tasks.AddRange(_customAnnotationFiles.Select(customAnnotationFile => Task.Factory.StartNew(() => CreateCutomAnnoTsv(customAnnotationFile))));
            tasks.AddRange(_customIntervalFiles.Select(customIntervalFile => Task.Factory.StartNew(() => CreateCustIntervalTsv(customIntervalFile))));

            try
            {
                Task.WaitAll(tasks.ToArray());
            }
            catch (AggregateException ae)
            {
                ae.Handle(x =>
                {
                    Console.WriteLine(x);
                    return true;
                });
                throw;
            }
        }

        private void CreateMitoMapSvTsv(IReadOnlyList<string> mitoMapSvFileNames)
        {
            if (mitoMapSvFileNames.Count == 0 || mitoMapSvFileNames.Any(string.IsNullOrEmpty)) return;
            var benchMark = new Benchmark();
            var rootDirectory = new FileInfo(mitoMapSvFileNames[0]).Directory;
            var version = DataSourceVersionReader.GetSourceVersion(Path.Combine(rootDirectory.ToString(), "mitoMapSV"));
            var sequenceProvider =
                new ReferenceSequenceProvider(FileUtilities.GetReadStream(_compressedReferencePath));
            sequenceProvider.LoadChromosome(new Chromosome("chrM", "MT", 24));
            var mitoMapSvReaders = new List<MitoMapSvReader>();

            foreach (var mitoMapFileName in mitoMapSvFileNames)
            {
                mitoMapSvReaders.Add(new MitoMapSvReader(new FileInfo(mitoMapFileName), sequenceProvider));
            }

            var mergedMitoMapItems = MitoMapSvReader.MergeAndSort(mitoMapSvReaders);

            using (var writer = new IntervalTsvWriter(_outputDirectory, version,
                GenomeAssembly.rCRS.ToString(), SaTsvCommon.MitoMapSchemaVersion, InterimSaCommon.MitoMapTag,
                ReportFor.StructuralVariants))
                CreateSvTsv(mergedMitoMapItems, writer);
            var timeSpan = Benchmark.ToHumanReadable(benchMark.GetElapsedTime());
            TsvWriterUtilities.WriteCompleteInfo(InterimSaCommon.MitoMapTag, version.Version, timeSpan);
        }

        private void CreateMitoMapVarTsv(IReadOnlyList<string> mitoMapFileNames)
        {
            if (mitoMapFileNames.Count == 0 || mitoMapFileNames.Any(string.IsNullOrEmpty)) return;
            var benchMark = new Benchmark();
            var rootDirectory = new FileInfo(mitoMapFileNames[0]).Directory;
            var version = DataSourceVersionReader.GetSourceVersion(Path.Combine(rootDirectory.ToString(), "mitoMapVar"));
            var sequenceProvider =
                new ReferenceSequenceProvider(FileUtilities.GetReadStream(_compressedReferencePath));
            sequenceProvider.LoadChromosome(new Chromosome("chrM", "MT", 24));
            var mitoMapVarReaders = new List<MitoMapVariantReader>();
            foreach (var mitoMapFileName in mitoMapFileNames)
            {
                mitoMapVarReaders.Add(new MitoMapVariantReader(new FileInfo(mitoMapFileName), sequenceProvider));
            }
            var mergedMitoMapVarItems = MitoMapVariantReader.MergeAndSort(mitoMapVarReaders);
            const string outputFilePrefix = InterimSaCommon.MitoMapTag;
            using (var writer = new MitoMapVarTsvWriter(version, _outputDirectory, outputFilePrefix, sequenceProvider))
                TsvWriterUtilities.WriteSortedItems(mergedMitoMapVarItems, writer);
            var timeSpan = Benchmark.ToHumanReadable(benchMark.GetElapsedTime());
            TsvWriterUtilities.WriteCompleteInfo(InterimSaCommon.MitoMapTag, version.Version, timeSpan);
        }

        private void CreateSvTsv(string sourceName, string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return;

            var benchMark = new Benchmark();
            //Console.WriteLine($"Creating TSV from {fileName}");
            var dataSource = "";
            var version = DataSourceVersionReader.GetSourceVersion(fileName);
            switch (sourceName)
            {
                case InterimSaCommon.DgvTag:
                    dataSource = "DGV";
                    using (var writer = new IntervalTsvWriter(_outputDirectory, version,
                        _genomeAssembly.ToString(), SaTsvCommon.DgvSchemaVersion, InterimSaCommon.DgvTag, ReportFor.StructuralVariants))
                    {
                        var reader = new DgvReader(new FileInfo(fileName), _refNamesDictionary);
                        CreateSvTsv(reader.GetDgvItems(), writer);
                    }
                    break;
                case InterimSaCommon.ClinGenTag:
                    dataSource = "ClinGen";
                    using (var writer = new IntervalTsvWriter(_outputDirectory, version,
                        _genomeAssembly.ToString(), SaTsvCommon.ClinGenSchemaVersion, InterimSaCommon.ClinGenTag,
                        ReportFor.StructuralVariants))
                    {
                        var reader = new ClinGenReader(new FileInfo(fileName), _refNamesDictionary);
                        CreateSvTsv(reader.GetClinGenItems(), writer);
                    }

                    break;
                case InterimSaCommon.OnekSvTag:
                    dataSource = "OnekSv";
                    using (var writer = new IntervalTsvWriter(_outputDirectory, version,
                        _genomeAssembly.ToString(), SaTsvCommon.OneKgenSchemaVersion, InterimSaCommon.OnekSvTag,
                        ReportFor.StructuralVariants))
                    {
                        var reader = new OneKGenSvReader(new FileInfo(fileName), _refNamesDictionary);
                        CreateSvTsv(reader.GetOneKGenSvItems(), writer);
                    }

                    break;

                default:
                    Console.WriteLine("invalid source name");
                    break;
            }

            var timeSpan = Benchmark.ToHumanReadable(benchMark.GetElapsedTime());
            TsvWriterUtilities.WriteCompleteInfo(dataSource, version.Version, timeSpan);
        }

        private static void CreateSvTsv(IEnumerable<SupplementaryDataItem> siItems, IntervalTsvWriter writer)
        {
            foreach (var siItem in siItems)
            {
                var interval = siItem.GetSupplementaryInterval();
                writer.AddEntry(interval.Chromosome.EnsemblName, interval.Start, interval.End, interval.GetJsonString());
            }
        }

        private void CreateCustIntervalTsv(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return;

            var benchMark = new Benchmark();

            var version = DataSourceVersionReader.GetSourceVersion(fileName);
            var reader = new CustomIntervalParser(new FileInfo(fileName), _refNamesDictionary);
            using (var writer = new IntervalTsvWriter(_outputDirectory, version,
                _genomeAssembly.ToString(), SaTsvCommon.CustIntervalSchemaVersion, reader.KeyName,
                ReportFor.AllVariants))
            {
                foreach (var custInterval in reader.GetCustomIntervals())
                {
                    writer.AddEntry(custInterval.Chromosome.EnsemblName, custInterval.Start, custInterval.End, custInterval.GetJsonString());
                }
            }

            var timeSpan = Benchmark.ToHumanReadable(benchMark.GetElapsedTime());
            TsvWriterUtilities.WriteCompleteInfo("customInterval", fileName, timeSpan);
        }

        private void CreateCutomAnnoTsv(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return;

            Console.WriteLine($"Creating TSV from {fileName}");
            var version = DataSourceVersionReader.GetSourceVersion(fileName);

            var customReader = new CustomAnnotationReader(new FileInfo(fileName), _refNamesDictionary);
            using (var writer = new CustomAnnoTsvWriter(version, _outputDirectory, _genomeAssembly, customReader.IsPositional, new ReferenceSequenceProvider(FileUtilities.GetReadStream(_compressedReferencePath))))
            {
                TsvWriterUtilities.WriteSortedItems(customReader.GetCustomItems(), writer);
            }

            Console.WriteLine($"Finished {fileName}");

        }

        private void CreateCosmicTsv(string vcfFile, string tsvFile)
        {
            if (string.IsNullOrEmpty(tsvFile) || string.IsNullOrEmpty(vcfFile)) return;

            var benchMark = new Benchmark();

            var version = DataSourceVersionReader.GetSourceVersion(vcfFile);
            using (var writer = new CosmicTsvWriter(version, _outputDirectory, _genomeAssembly, new ReferenceSequenceProvider(FileUtilities.GetReadStream(_compressedReferencePath))))
            {
                var tsvReader = GZipUtilities.GetAppropriateStreamReader(tsvFile);
                var vcfReader = GZipUtilities.GetAppropriateStreamReader(vcfFile);
                var reader = new MergedCosmicReader(vcfReader, tsvReader, _refNamesDictionary);

                TsvWriterUtilities.WriteSortedItems(reader.GetCosmicItems(), writer);

            }

            var timeSpan = Benchmark.ToHumanReadable(benchMark.GetElapsedTime());
            TsvWriterUtilities.WriteCompleteInfo("COSMIC", version.Version, timeSpan);
        }
        private void CreateEvsTsv(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return;
            var benchMark = new Benchmark();

            var version = DataSourceVersionReader.GetSourceVersion(fileName);
            using (var writer = new EvsTsvWriter(version, _outputDirectory, _genomeAssembly, new ReferenceSequenceProvider(FileUtilities.GetReadStream(_compressedReferencePath))))
            {
                var evsReader = new EvsReader(GZipUtilities.GetAppropriateStreamReader(fileName), _refNamesDictionary);
                TsvWriterUtilities.WriteSortedItems(evsReader.GetEvsItems(), writer);
            }
            var timeSpan = Benchmark.ToHumanReadable(benchMark.GetElapsedTime());
            TsvWriterUtilities.WriteCompleteInfo("EVS", version.Version, timeSpan);
        }

        private void CreateExacTsv(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return;
            var benchMark = new Benchmark();

            var version = DataSourceVersionReader.GetSourceVersion(fileName);
            using (var writer = new ExacTsvWriter(version, _outputDirectory, _genomeAssembly, new ReferenceSequenceProvider(FileUtilities.GetReadStream(_compressedReferencePath))))
            {
                var exacReader = new ExacReader(new FileInfo(fileName), _refNamesDictionary);
                TsvWriterUtilities.WriteSortedItems(exacReader.GetExacItems(), writer);
            }

            var timeSpan = Benchmark.ToHumanReadable(benchMark.GetElapsedTime());
            TsvWriterUtilities.WriteCompleteInfo("EXaC", version.Version, timeSpan);
        }

        private void CreateClinvarTsv(string fileName)
        {
            if (fileName == null) return;
            var benchMark = new Benchmark();

            var version = DataSourceVersionReader.GetSourceVersion(fileName);
            //clinvar items do not come in sorted order, hence we need to store them in an array, sort them and then flush them out
            using (var writer = new ClinvarTsvWriter(version, _outputDirectory, _genomeAssembly, new ReferenceSequenceProvider(FileUtilities.GetReadStream(_compressedReferencePath))))
            {
                var clinvarReader = new ClinVarXmlReader(new FileInfo(fileName), new ReferenceSequenceProvider(FileUtilities.GetReadStream(_compressedReferencePath)));
                TsvWriterUtilities.WriteSortedItems(clinvarReader.GetItems(), writer);
            }

            var timeSpan = Benchmark.ToHumanReadable(benchMark.GetElapsedTime());
            TsvWriterUtilities.WriteCompleteInfo("ClinVar", version.Version, timeSpan);
        }

        private void CreateDbsnpGaTsv(string fileName)
        {
            if (fileName == null) return;

            var benchMark = new Benchmark();

            var version = DataSourceVersionReader.GetSourceVersion(fileName);

            var dbsnpWriter = new SaTsvWriter(_outputDirectory, version, _genomeAssembly.ToString(),
                SaTsvCommon.DbSnpSchemaVersion, InterimSaCommon.DbsnpTag, null, true, new ReferenceSequenceProvider(FileUtilities.GetReadStream(_compressedReferencePath)));

            var globalAlleleWriter = new SaTsvWriter(_outputDirectory, version, _genomeAssembly.ToString(),
                SaTsvCommon.DbSnpSchemaVersion, InterimSaCommon.GlobalAlleleTag, "GMAF", false, new ReferenceSequenceProvider(FileUtilities.GetReadStream(_compressedReferencePath)));
            using (var tsvWriter = new DbsnpGaTsvWriter(dbsnpWriter, globalAlleleWriter))
            {
                var dbSnpReader = new DbSnpReader(GZipUtilities.GetAppropriateReadStream(fileName), _refNamesDictionary);
                TsvWriterUtilities.WriteSortedItems(dbSnpReader.GetDbSnpItems(), tsvWriter);
            }

            var timeSpan = Benchmark.ToHumanReadable(benchMark.GetElapsedTime());
            TsvWriterUtilities.WriteCompleteInfo("DbSNP", version.Version, timeSpan);

        }

        private void CreateOnekgTsv(string fileName)
        {
            if (fileName == null) return;
            var benchMark = new Benchmark();

            var version = DataSourceVersionReader.GetSourceVersion(fileName);

            using (var tsvWriter = new OnekgTsvWriter(version, _outputDirectory, _genomeAssembly, new ReferenceSequenceProvider(FileUtilities.GetReadStream(_compressedReferencePath))))
            {
                var onekgReader = new OneKGenReader(new FileInfo(fileName), _refNamesDictionary);
                TsvWriterUtilities.WriteSortedItems(onekgReader.GetOneKGenItems(), tsvWriter);
            }
            var timeSpan = Benchmark.ToHumanReadable(benchMark.GetElapsedTime());
            TsvWriterUtilities.WriteCompleteInfo("OneKg", version.Version, timeSpan);
        }

    }
}