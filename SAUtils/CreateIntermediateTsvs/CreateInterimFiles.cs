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
using SAUtils.InputFileParsers.MitoMAP;
using SAUtils.InputFileParsers.OneKGen;
using SAUtils.TsvWriters;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.Providers;
using VariantAnnotation.Sequence;
using VariantAnnotation.Utilities;

namespace SAUtils.CreateIntermediateTsvs
{
    internal sealed class CreateInterimFiles
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
        public CreateInterimFiles(string compressedReferencePath, string outputDirectory, string dbSnpFileName, string cosmicVcfFileName, string cosmicTsvFileName, string clinVarFileName, string onekGFileName, string evsFile, string exacFile, string dgvFile, string onekGSvFileName, string clinGenFileName, List<string> mitoMapVarFileNames, List<string> mitoMapSvFileNames, List<string> customAnnotationFiles, List<string> customIntervalFiles)
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
            _refNamesDictionary = sequenceProvider.GetChromosomeDictionary();
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
                ae.Handle((x) =>
                {
                    Console.WriteLine(x);
                    return true;
                });
                throw;
            }
        }

        private void CreateMitoMapSvTsv(List<string> mitoMapSvFileNames)
        {
            if (mitoMapSvFileNames.Count == 0 || mitoMapSvFileNames.Any(String.IsNullOrEmpty)) return;
            var benchMark = new Benchmark();
            var rootDirectory = new FileInfo(mitoMapSvFileNames[0]).Directory;
            var version = GetDataSourceVersion(Path.Combine(rootDirectory.ToString(), "mitoMap"));
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
                _genomeAssembly.ToString(), SaTSVCommon.MitoMapSchemaVersion, InterimSaCommon.MitoMapSvTag,
                ReportFor.StructuralVariants))
                CreateSvTsv(mergedMitoMapItems, writer);
            var timeSpan = Benchmark.ToHumanReadable(benchMark.GetElapsedTime());
            WriteCompleteInfo(InterimSaCommon.MitoMapSvTag, version.Version, timeSpan);
        }

        private void CreateMitoMapVarTsv(List<string> mitoMapFileNames)
        {
            if (mitoMapFileNames.Count == 0 || mitoMapFileNames.Any(String.IsNullOrEmpty)) return;
            var benchMark = new Benchmark();
            var rootDirectory = new FileInfo(mitoMapFileNames[0]).Directory;
            var version = GetDataSourceVersion(Path.Combine(rootDirectory.ToString(), "mitoMap"));
            var sequenceProvider =
                new ReferenceSequenceProvider(FileUtilities.GetReadStream(_compressedReferencePath));
            sequenceProvider.LoadChromosome(new Chromosome("chrM", "MT", 24));
            var mitoMapVarReaders = new List<MitoMapVariantReader>();
            foreach (var mitoMapFileName in mitoMapFileNames)
            {
                mitoMapVarReaders.Add(new MitoMapVariantReader(new FileInfo(mitoMapFileName), sequenceProvider));
            }
            var mergedMitoMapVarItems = MitoMapVariantReader.MergeAndSort(mitoMapVarReaders);
            var outputFilePrefix = InterimSaCommon.MitoMapVarTag;
                using (var writer = new MitoMapVarTsvWriter(version, _outputDirectory, outputFilePrefix, sequenceProvider))
                    WriteSortedItems(mergedMitoMapVarItems, writer);         
            var timeSpan = Benchmark.ToHumanReadable(benchMark.GetElapsedTime());
            WriteCompleteInfo(InterimSaCommon.MitoMapVarTag, version.Version, timeSpan);
        }

        private void CreateSvTsv(string sourceName, string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return;

            var benchMark = new Benchmark();
            //Console.WriteLine($"Creating TSV from {fileName}");
            var dataSource = "";
            DataSourceVersion version = GetDataSourceVersion(fileName); ;
            switch (sourceName)
            {
                case InterimSaCommon.DgvTag:
                    dataSource = "DGV";
                    using (var writer = new IntervalTsvWriter(_outputDirectory, version,
                        _genomeAssembly.ToString(), SaTSVCommon.DgvSchemaVersion, InterimSaCommon.DgvTag, ReportFor.StructuralVariants))
                    {
                        CreateSvTsv(new DgvReader(new FileInfo(fileName), _refNamesDictionary).GetEnumerator(), writer);
                    }
                    break;
                case InterimSaCommon.ClinGenTag:
                    dataSource = "ClinGen";
                    using (var writer = new IntervalTsvWriter(_outputDirectory, version,
                        _genomeAssembly.ToString(), SaTSVCommon.ClinGenSchemaVersion, InterimSaCommon.ClinGenTag,
                        ReportFor.StructuralVariants))
                    {
                        CreateSvTsv(new ClinGenReader(new FileInfo(fileName), _refNamesDictionary).GetEnumerator(), writer);
                    }

                    break;
                case InterimSaCommon.OnekSvTag:
                    dataSource = "OnekSv";
                    using (var writer = new IntervalTsvWriter(_outputDirectory, version,
                        _genomeAssembly.ToString(), SaTSVCommon.OneKgenSchemaVersion, InterimSaCommon.OnekSvTag,
                        ReportFor.StructuralVariants))
                    {
                        CreateSvTsv(new OneKGenSvReader(new FileInfo(fileName), _refNamesDictionary).GetEnumerator(), writer);
                    }

                    break;

                default:
                    Console.WriteLine("invalid source name");
                    break;
            }

            var timeSpan = Benchmark.ToHumanReadable(benchMark.GetElapsedTime());
            WriteCompleteInfo(dataSource, version.Version, timeSpan);
        }

        private void CreateSvTsv(IEnumerator<SupplementaryDataItem> siItems, IntervalTsvWriter writer)
        {
            while (siItems.MoveNext())
            {
                var siItem = siItems.Current.GetSupplementaryInterval();
                writer.AddEntry(siItem.Chromosome.EnsemblName, siItem.Start, siItem.End, siItem.GetJsonString());
            }
        }

        private void CreateCustIntervalTsv(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return;

            var benchMark = new Benchmark();

            var version = GetDataSourceVersion(fileName);
            var reader = new CustomIntervalParser(new FileInfo(fileName), _refNamesDictionary);
            using (var writer = new IntervalTsvWriter(_outputDirectory, version,
                _genomeAssembly.ToString(), SaTSVCommon.CustIntervalSchemaVersion, reader.KeyName,
                ReportFor.AllVariants))
            {
                foreach (var custInterval in reader)
                {
                    writer.AddEntry(custInterval.Chromosome.EnsemblName, custInterval.Start, custInterval.End, custInterval.GetJsonString());
                }
            }

            var timeSpan = Benchmark.ToHumanReadable(benchMark.GetElapsedTime());
            WriteCompleteInfo("customInterval", fileName, timeSpan);
        }

        private void CreateCutomAnnoTsv(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return;

            Console.WriteLine($"Creating TSV from {fileName}");
            var version = GetDataSourceVersion(fileName);

            var customReader = new CustomAnnotationReader(new FileInfo(fileName), _refNamesDictionary);
            using (var writer = new CustomAnnoTsvWriter(version, _outputDirectory, _genomeAssembly, customReader.IsPositional, new ReferenceSequenceProvider(FileUtilities.GetReadStream(_compressedReferencePath))))
            {
                WriteSortedItems(customReader.GetEnumerator(), writer);
            }

            Console.WriteLine($"Finished {fileName}");

        }

        private void CreateCosmicTsv(string vcfFile, string tsvFile)
        {
            if (string.IsNullOrEmpty(tsvFile) || string.IsNullOrEmpty(vcfFile)) return;

            var benchMark = new Benchmark();

            var version = GetDataSourceVersion(vcfFile);
            using (var writer = new CosmicTsvWriter(version, _outputDirectory, _genomeAssembly, new ReferenceSequenceProvider(FileUtilities.GetReadStream(_compressedReferencePath))))
            {
                var cosmicReader = new MergedCosmicReader(vcfFile, tsvFile, _refNamesDictionary);
                WriteSortedItems(cosmicReader.GetEnumerator(), writer);
            }

            var timeSpan = Benchmark.ToHumanReadable(benchMark.GetElapsedTime());
            WriteCompleteInfo("COSMIC", version.Version, timeSpan);
        }

        private void CreateEvsTsv(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return;
            var benchMark = new Benchmark();

            var version = GetDataSourceVersion(fileName);
            using (var writer = new EvsTsvWriter(version, _outputDirectory, _genomeAssembly, new ReferenceSequenceProvider(FileUtilities.GetReadStream(_compressedReferencePath))))
            {
                var evsReader = new EvsReader(new FileInfo(fileName), _refNamesDictionary);
                WriteSortedItems(evsReader.GetEnumerator(), writer);
            }
            var timeSpan = Benchmark.ToHumanReadable(benchMark.GetElapsedTime());
            WriteCompleteInfo("EVS", version.Version, timeSpan);
        }

        private void CreateExacTsv(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return;
            var benchMark = new Benchmark();

            var version = GetDataSourceVersion(fileName);
            using (var writer = new ExacTsvWriter(version, _outputDirectory, _genomeAssembly, new ReferenceSequenceProvider(FileUtilities.GetReadStream(_compressedReferencePath))))
            {
                var exacReader = new ExacReader(new FileInfo(fileName), _refNamesDictionary);
                WriteSortedItems(exacReader.GetEnumerator(), writer);
            }

            var timeSpan = Benchmark.ToHumanReadable(benchMark.GetElapsedTime());
            WriteCompleteInfo("EXaC", version.Version, timeSpan);
        }

        private void CreateClinvarTsv(string fileName)
        {
            if (fileName == null) return;
            var benchMark = new Benchmark();

            var version = GetDataSourceVersion(fileName);
            //clinvar items do not come in sorted order, hence we need to store them in an array, sort them and then flush them out
            using (var writer = new ClinvarTsvWriter(version, _outputDirectory, _genomeAssembly, new ReferenceSequenceProvider(FileUtilities.GetReadStream(_compressedReferencePath))))
            {
                var clinvarReader = new ClinVarXmlReader(new FileInfo(fileName), new ReferenceSequenceProvider(FileUtilities.GetReadStream(_compressedReferencePath)));
                var clinvarList = clinvarReader.ToList();
                clinvarList.Sort();
                WriteSortedItems(clinvarList.GetEnumerator(), writer);
            }

            var timeSpan = Benchmark.ToHumanReadable(benchMark.GetElapsedTime());
            WriteCompleteInfo("ClinVar", version.Version, timeSpan);
        }

        private void CreateDbsnpGaTsv(string fileName)
        {
            if (fileName == null) return;

            var benchMark = new Benchmark();

            var version = GetDataSourceVersion(fileName);

            using (var tsvWriter = new DbsnpGaTsvWriter(version, _outputDirectory, _genomeAssembly, new ReferenceSequenceProvider(FileUtilities.GetReadStream(_compressedReferencePath))))
            {
                var dbSnpReader = new DbSnpReader(GZipUtilities.GetAppropriateReadStream(fileName), _refNamesDictionary);
                WriteSortedItems(dbSnpReader.GetEnumerator(), tsvWriter);
            }

            var timeSpan = Benchmark.ToHumanReadable(benchMark.GetElapsedTime());
            WriteCompleteInfo("DbSNP", version.Version, timeSpan);

        }

        private void CreateOnekgTsv(string fileName)
        {
            if (fileName == null) return;
            var benchMark = new Benchmark();

            var version = GetDataSourceVersion(fileName);

            using (var tsvWriter = new OnekgTsvWriter(version, _outputDirectory, _genomeAssembly, new ReferenceSequenceProvider(FileUtilities.GetReadStream(_compressedReferencePath))))
            {
                var onekgReader = new OneKGenReader(new FileInfo(fileName), _refNamesDictionary);
                WriteSortedItems(onekgReader.GetEnumerator(), tsvWriter);
            }
            var timeSpan = Benchmark.ToHumanReadable(benchMark.GetElapsedTime());
            WriteCompleteInfo("OneKg", version.Version, timeSpan);
        }

        private static DataSourceVersion GetDataSourceVersion(string dataFileName)
        {
            var versionFileName = dataFileName + ".version";

            var version = DataSourceVersionReader.GetSourceVersion(versionFileName);
            return version;
        }

        private void WriteSortedItems(IEnumerator<SupplementaryDataItem> saItems, ISaItemTsvWriter writer)
        {
            var itemsMinHeap = new MinHeap<SupplementaryDataItem>();
            var currentRefIndex = Int32.MaxValue;

            var benchmark = new Benchmark();
            while (saItems.MoveNext())
            {
                var saItem = saItems.Current;
                //if (!SupplementaryAnnotationUtilities.IsRefAlleleValid(_compressedSequence, saItem.Start, saItem.ReferenceAllele))
                //	continue;
                if (currentRefIndex != saItem.Chromosome.Index)
                {
                    if (currentRefIndex != Int32.MaxValue)
                    {
                        //flushing out the remaining items in buffer
                        WriteToPosition(writer, itemsMinHeap, int.MaxValue);
                        //Console.WriteLine($"Wrote out chr{currentRefIndex} items in {benchmark.GetElapsedTime()}");
                        benchmark.Reset();
                    }
                    currentRefIndex = saItem.Chromosome.Index;
                    //Console.WriteLine("Writing items from chromosome:" + currentRefIndex);
                }

                //the items come in sorted order of the pre-trimmed position. 
                //So when writing out, we have to make sure that we do not write past this position. 
                //Once a position has been seen in the stream, we can safely write all positions before that.
                var writeToPos = saItem.Start;

                saItem.Trim();
                itemsMinHeap.Add(saItem);

                WriteToPosition(writer, itemsMinHeap, writeToPos);
            }

            //flushing out the remaining items in buffer
            WriteToPosition(writer, itemsMinHeap, int.MaxValue);
        }


        private static void WriteToPosition(ISaItemTsvWriter writer, MinHeap<SupplementaryDataItem> itemsHeap, int position)
        {
            if (itemsHeap.Count() == 0) return;
            var bufferMin = itemsHeap.GetMin();

            while (bufferMin.Start < position)
            {
                var itemsAtMinPosition = new List<SupplementaryDataItem>();

                while (itemsHeap.Count() > 0 && bufferMin.CompareTo(itemsHeap.GetMin()) == 0)
                    itemsAtMinPosition.Add(itemsHeap.ExtractMin());

                writer.WritePosition(itemsAtMinPosition);

                if (itemsHeap.Count() == 0) break;

                bufferMin = itemsHeap.GetMin();
            }

        }

        private static void WriteCompleteInfo(string dataSourceDescription, string version, string timeSpan)
        {

            Console.Write($"{dataSourceDescription,-20}    {version,-20}");
            Console.ForegroundColor = ConsoleColor.Yellow;

            Console.WriteLine($"{timeSpan,-20}");
            Console.ResetColor();

        }

    }
}