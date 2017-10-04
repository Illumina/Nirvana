using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommandLine.Utilities;
using Compression.Utilities;
using SAUtils.DataStructures;
using SAUtils.InputFileParsers.IntermediateAnnotation;
using SAUtils.Interface;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.Providers;
using VariantAnnotation.SA;
using VariantAnnotation.Utilities;
using VariantAnnotation.Interface.GeneAnnotation;
using VariantAnnotation.GeneAnnotation;

namespace SAUtils.MergeInterimTsvs
{
    public sealed class MergeInterimTsvs
    {
        private List<SaTsvReader> _tsvReaders;
        private List<IntervalTsvReader> _intervalReaders;
        private List<GeneTsvReader> _geneReaders;
        private SaMiscellaniesReader _miscReader;
        private readonly List<InterimSaHeader> _interimSaHeaders;
        private readonly List<InterimIntervalHeader> _intervalHeaders;
        private readonly List<InterimHeader> _geneHeaders;
        private readonly string _outputDirectory;
        private readonly GenomeAssembly _genomeAssembly;
        private readonly IDictionary<string, IChromosome> _refChromDict;
        private List<string> _allRefNames;
        private readonly HashSet<GenomeAssembly> _assembliesIgnoredInConsistancyCheck = new HashSet<GenomeAssembly>() { GenomeAssembly.Unknown, GenomeAssembly.rCRS };

        /// <summary>
        /// constructor
        /// </summary>
        public MergeInterimTsvs(List<string> annotationFiles, List<string> intervalFiles, string miscFile, List<string> geneFiles, string compressedReference, string outputDirectory, List<string> chrWhiteList = null)
        {
            _outputDirectory = outputDirectory;

            var refSequenceProvider = new ReferenceSequenceProvider(FileUtilities.GetReadStream(compressedReference));
            _genomeAssembly = refSequenceProvider.GenomeAssembly;
            _refChromDict = refSequenceProvider.GetChromosomeDictionary();
            _interimSaHeaders = new List<InterimSaHeader>();
            _intervalHeaders = new List<InterimIntervalHeader>();
            _geneHeaders = new List<InterimHeader>();
            _allRefNames = new List<string>();
            var headers = new List<InterimHeader>();
            SetSaTsvReaders(annotationFiles, headers);
            SetIntervalReaders(intervalFiles, headers);
            SetGeneReaders(geneFiles, headers);
            SetMiscTsvReader(miscFile);
            DisplayDataSources(headers);

            //SetChrWhiteList(chrWhiteList);
            CheckAssemblyConsistancy();
        }

        private void SetGeneReaders(List<string> geneFiles, List<InterimHeader> headers)
        {
            if (geneFiles == null) return;

            _geneReaders = new List<GeneTsvReader>(geneFiles.Count);
            foreach (var fileName in geneFiles)
            {
                var geneReader = new GeneTsvReader(new FileInfo(fileName));
                var header = geneReader.GetHeader();
                if (header == null) throw new InvalidDataException("Data file lacks version information!!");
                headers.Add(header);
                _geneHeaders.Add(header);
                _geneReaders.Add(geneReader);
            }
        }

        private void DisplayDataSources(List<InterimHeader> headers)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Data sources:\n");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("Name                     Version       Release Date          Misc");
            Console.WriteLine("=======================================================================");
            Console.ResetColor();

            foreach (var header in headers.OrderBy(h => h.GetDataSourceVersion().Name))
            {
                Console.WriteLine(header);
            }

            Console.WriteLine();
        }

        //private void SetChrWhiteList(List<string> chrWhiteList)
        //{
        //    if (chrWhiteList != null)
        //    {
        //        Console.WriteLine("Creating SA for the following chromosomes only:");
        //        foreach (var refSeq in chrWhiteList)
        //        {
        //            InputFileParserUtilities.ChromosomeWhiteList.Add(_chromosomeRenamer.GetEnsemblReferenceName(refSeq));
        //            Console.Write(refSeq + ",");
        //        }
        //        Console.WriteLine();
        //    }
        //    else InputFileParserUtilities.ChromosomeWhiteList = null;
        //}

        private List<IEnumerator<ISupplementaryInterval>> GetIntervalEnumerators(string refName)
        {
            if (_intervalReaders == null) return null;

            var interimIntervalEnumerators = new List<IEnumerator<ISupplementaryInterval>>();
            foreach (var intervalReader in _intervalReaders)
            {
                var dataEnumerator = intervalReader.GetEnumerator(refName);
                if (!dataEnumerator.MoveNext()) continue;

                interimIntervalEnumerators.Add(dataEnumerator);
            }
            return interimIntervalEnumerators;
        }


        private List<Tuple<int, string>> GetGlobalMajorAlleleForRefMinors(string refName)
        {
            var globalAlleles = new List<Tuple<int, string>>();
            if (_miscReader == null) return globalAlleles;
            foreach (var saMiscellaniese in _miscReader.GetAnnotationItems(refName))
            {
                globalAlleles.Add(Tuple.Create(saMiscellaniese.Position, saMiscellaniese.GlobalMajorAllele));
            }
            return globalAlleles;
        }


        private List<IEnumerator<IAnnotatedGene>> GetGeneAnnotationEnumerator()
        {
            var geneAnnotationList = new List<IEnumerator<IAnnotatedGene>>();
            if (_geneReaders == null) return geneAnnotationList;

            foreach (var geneReader in _geneReaders)
            {
                var dataEnumerator = geneReader.GetEnumerator();
                if (!dataEnumerator.MoveNext()) continue;
                geneAnnotationList.Add(dataEnumerator);
            }
            return geneAnnotationList;
        }

        private List<IEnumerator<IInterimSaItem>> GetSaEnumerators(string refName)
        {
            var saItemsList = new List<IEnumerator<IInterimSaItem>>();
            if (_tsvReaders == null) return saItemsList;
            foreach (var tsvReader in _tsvReaders)
            {
                var dataEnumerator = tsvReader.GetEnumerator(refName);
                if (!dataEnumerator.MoveNext()) continue;

                saItemsList.Add(dataEnumerator);
            }

            return saItemsList;
        }

        private void SetIntervalReaders(List<string> intervalFiles, List<InterimHeader> headers)
        {
            if (intervalFiles == null) return;

            _intervalReaders = new List<IntervalTsvReader>(intervalFiles.Count);
            foreach (var fileName in intervalFiles)
            {
                var intervalReader = new IntervalTsvReader(new FileInfo(fileName));

                var header = intervalReader.GetHeader();
                if (header == null) throw new InvalidDataException("Data file lacks version information!!");
                headers.Add(header);
                _intervalHeaders.Add(header);

                _allRefNames.AddRange(intervalReader.GetAllRefNames());
                _intervalReaders.Add(intervalReader);
            }
        }

        private void SetSaTsvReaders(List<string> annotationFiles, List<InterimHeader> headers)
        {
            if (annotationFiles == null) return;
            _tsvReaders = new List<SaTsvReader>(annotationFiles.Count);

            foreach (var fileName in annotationFiles)
            {
                var tsvReader = new SaTsvReader(new FileInfo(fileName));

                var header = tsvReader.GetHeader();
                if (header == null) throw new InvalidDataException("Data file lacks version information!!");
                headers.Add(header);
                _interimSaHeaders.Add(header);

                _allRefNames.AddRange(tsvReader.GetAllRefNames());
                _tsvReaders.Add(tsvReader);
            }

            _allRefNames = _allRefNames.Distinct().ToList();
        }

        private void SetMiscTsvReader(string miscFile)
        {
            if (string.IsNullOrEmpty(miscFile)) return;

            var miscFileReader = GZipUtilities.GetAppropriateStreamReader(miscFile);
            var indexFileStream = new FileStream(miscFile + ".tvi", FileMode.Open);
            _miscReader = new SaMiscellaniesReader(miscFileReader, indexFileStream);
            _allRefNames.AddRange(_miscReader.GetAllRefNames());


        }

        private void CheckAssemblyConsistancy()
        {
            var uniqueAssemblies = _interimSaHeaders.Select(x => x.GenomeAssembly)
                .Concat(_intervalHeaders.Select(x => x.GenomeAssembly))
                .Where(x => !_assembliesIgnoredInConsistancyCheck.Contains(x))
                .Distinct()
                .ToList();

            if (uniqueAssemblies.Count > 1)
                throw new InvalidDataException($"ERROR: The genome assembly for all data sources should be the same. Found {string.Join(", ", uniqueAssemblies.ToArray())}");
        }

        public void Merge()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("SA File Creation:\n");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("Name                     Annotations Intervals RefMinors Creation Time");
            Console.WriteLine("=======================================================================================");
            Console.ResetColor();


            MergeGene();

            _allRefNames = _allRefNames.Distinct().ToList();
            Parallel.ForEach(_allRefNames, new ParallelOptions { MaxDegreeOfParallelism = 4 }, MergeChrom);


            //foreach (var refName in _allRefNames)
            //{
            //     if (refName !="1") continue;
            //    MergeChrom(refName);
            //}
        }

        private void MergeGene()
        {
            var geneAnnotationList = GetGeneAnnotationEnumerator();

            List<IAnnotatedGene> geneAnnotations = null;
            var geneAnnotationDatabasePath = Path.Combine(_outputDirectory, SaDataBaseCommon.OmimDatabaseFileName);
            var geneAnnotationStream = FileUtilities.GetCreateStream(geneAnnotationDatabasePath);
            var databaseHeader = new SupplementaryAnnotationHeader("", DateTime.Now.Ticks, SaDataBaseCommon.DataVersion, _geneHeaders.Select(x => x.GetDataSourceVersion()), _genomeAssembly);
            using (var writer = new GeneDatabaseWriter(geneAnnotationStream, databaseHeader))
                while ((geneAnnotations = GetMinItems(geneAnnotationList)) != null)
                {
                    var mergedGeneAnnotation = MergeGeneAnnotations(geneAnnotations);
                    writer.Write(mergedGeneAnnotation);
                }
        }

        private IAnnotatedGene MergeGeneAnnotations(List<IAnnotatedGene> geneAnnotations)
        {
            if (geneAnnotations == null || geneAnnotations.Count == 0) return null;

            if (geneAnnotations[0].GeneName=="AGRN")
                Console.WriteLine("bug");
            var annotations = geneAnnotations.SelectMany(x => x.Annotations).ToArray();

            return new AnnotatedGene(geneAnnotations[0].GeneName, annotations);
        }

        private void MergeChrom(string refName)
        {
            var creationBench = new Benchmark();
            var currentChrAnnotationCount = 0;
            int refMinorCount;

            var iInterimSaItemsList = GetSaEnumerators(refName);

            var globalMajorAlleleInRefMinors = GetGlobalMajorAlleleForRefMinors(refName);

            var ucscRefName = _refChromDict[refName].UcscName;
            var dataSourceVersions = GetDataSourceVersions(_interimSaHeaders, _intervalHeaders);

            var header = new SupplementaryAnnotationHeader(ucscRefName, DateTime.Now.Ticks,
                SaDataBaseCommon.DataVersion, dataSourceVersions, _genomeAssembly);

            var interimIntervalEnumerators = GetIntervalEnumerators(refName);
            var intervals = GetIntervals(interimIntervalEnumerators).OrderBy(x => x.Start).ThenBy(x => x.End).ToList();

            var smallVariantIntervals = GetSpecificIntervals(ReportFor.SmallVariants, intervals);
            var svIntervals = GetSpecificIntervals(ReportFor.StructuralVariants, intervals);
            var allVariantsIntervals = GetSpecificIntervals(ReportFor.AllVariants, intervals);

            var saPath = Path.Combine(_outputDirectory, $"{ucscRefName}.nsa");

            using (var stream = FileUtilities.GetCreateStream(saPath))
            using (var idxStream = FileUtilities.GetCreateStream(saPath + ".idx"))
            using (var blockSaWriter = new SaWriter(stream, idxStream, header, smallVariantIntervals, svIntervals, allVariantsIntervals, globalMajorAlleleInRefMinors))
            {
                InterimSaPosition currPosition;
                while ((currPosition = GetNextInterimPosition(iInterimSaItemsList)) != null)
                {
                    var saPosition = currPosition.Convert();
                    blockSaWriter.Write(saPosition, currPosition.Position);
                    currentChrAnnotationCount++;
                }

                refMinorCount = blockSaWriter.RefMinorCount;
            }

            Console.WriteLine($"{ucscRefName,-23}  {currentChrAnnotationCount,10:n0}   {intervals.Count,6:n0}    {refMinorCount,6:n0}   {creationBench.GetElapsedIterationTime(currentChrAnnotationCount, "variants", out double lookupsPerSecond)}");
        }

        private static List<ISupplementaryInterval> GetSpecificIntervals(ReportFor reportFor, IEnumerable<ISupplementaryInterval> intervals)
        {
            return intervals.Where(interval => interval.ReportingFor == reportFor).ToList();
        }

        private static IEnumerable<IDataSourceVersion> GetDataSourceVersions(List<InterimSaHeader> interimSaHeaders,
            List<InterimIntervalHeader> intervalHeaders)
        {
            var versions = new List<IDataSourceVersion>();

            foreach (var header in interimSaHeaders)
            {
                var version = header.GetDataSourceVersion();
                versions.Add(version);
            }

            foreach (var header in intervalHeaders)
            {
                var version = header.GetDataSourceVersion();
                versions.Add(version);
            }

            return versions;
        }

        private List<ISupplementaryInterval> GetIntervals(List<IEnumerator<ISupplementaryInterval>> interimIntervalEnumerators)
        {
            var intervals = new List<ISupplementaryInterval>();
            if (interimIntervalEnumerators == null || interimIntervalEnumerators.Count == 0) return intervals;

            foreach (var intervalEnumerator in interimIntervalEnumerators)
            {
                ISupplementaryInterval currInterval;
                while ((currInterval = intervalEnumerator.Current) != null)
                {
                    intervals.Add(currInterval);
                    if (intervalEnumerator.MoveNext()) continue;
                    break;
                }
            }

            return intervals;
        }

        private InterimSaPosition GetNextInterimPosition(List<IEnumerator<IInterimSaItem>> interimSaItemsList)
        {
            var minItems = GetMinItems(interimSaItemsList);
            if (minItems == null) return null;

            var interimSaPosition = new InterimSaPosition();
            interimSaPosition.AddSaItems(minItems);

            return interimSaPosition;
        }

        private List<T> GetMinItems<T>(List<IEnumerator<T>> interimSaItemsList) where T : IComparable<T>
        {
            if (interimSaItemsList.Count == 0) return null;

            var minItem = GetMinItem(interimSaItemsList);
            var minItems = new List<T>();
            var removeList = new List<IEnumerator<T>>();

            foreach (var saEnumerator in interimSaItemsList)
            {
                if (minItem.CompareTo(saEnumerator.Current) < 0) continue;

                while (minItem.CompareTo(saEnumerator.Current) == 0)
                {
                    minItems.Add(saEnumerator.Current);
                    if (saEnumerator.MoveNext()) continue;
                    removeList.Add(saEnumerator);
                    break;
                }
            }

            RemoveEnumerators(removeList, interimSaItemsList);
            return minItems.Count == 0 ? null : minItems;
        }

        private T GetMinItem<T>(List<IEnumerator<T>> interimSaItemsList) where T : IComparable<T>
        {
            var minItem = interimSaItemsList[0].Current;
            foreach (var saEnumerator in interimSaItemsList)
            {
                if (minItem.CompareTo(saEnumerator.Current) > 0)
                    minItem = saEnumerator.Current;
            }
            return minItem;
        }

        private void RemoveEnumerators<T>(List<IEnumerator<T>> removeList, List<IEnumerator<T>> interimSaItemsList)
        {
            if (removeList.Count == 0) return;

            foreach (var enumerator in removeList)
            {
                interimSaItemsList.Remove(enumerator);
            }
        }
    }
}
