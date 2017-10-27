using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommandLine.Utilities;
using SAUtils.DataStructures;
using SAUtils.InputFileParsers.IntermediateAnnotation;
using SAUtils.Interface;
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
        public static readonly HashSet<GenomeAssembly> AssembliesIgnoredInConsistancyCheck = new HashSet<GenomeAssembly>() { GenomeAssembly.Unknown, GenomeAssembly.rCRS };

        /// <summary>
        /// constructor
        /// </summary>
        public MergeInterimTsvs(List<string> annotationFiles, List<string> intervalFiles, string miscFile, List<string> geneFiles, string compressedReference, string outputDirectory)
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

            MergeUtilities.CheckAssemblyConsistancy(_interimSaHeaders, _intervalHeaders);
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

        private IEnumerable<IEnumerable<ISupplementaryInterval>> GetIntervalEnumerables(string refName)
        {
            return _intervalReaders?.Select(intervalReader => intervalReader.GetAnnotationItems(refName)).ToList();
        }


        private List<(int, string)> GetGlobalMajorAlleleForRefMinors(string refName)
        {
            var globalAlleles = new List<(int, string)>();
            if (_miscReader == null) return globalAlleles;
            foreach (var saMiscellaniese in _miscReader.GetAnnotationItems(refName))
            {
                globalAlleles.Add((saMiscellaniese.Position, saMiscellaniese.GlobalMajorAllele));
            }
            return globalAlleles;
        }


        private List<IEnumerator<IAnnotatedGene>> GetGeneEnumerators()
        {
            var geneAnnotationList = new List<IEnumerator<IAnnotatedGene>>();
            if (_geneReaders == null) return geneAnnotationList;

            foreach (var geneReader in _geneReaders)
            {
                var dataEnumerator = geneReader.GetAnnotationItems().GetEnumerator();
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
                var dataEnumerator = tsvReader.GetAnnotationItems(refName).GetEnumerator();
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

            _miscReader = new SaMiscellaniesReader(new FileInfo(miscFile));
            _allRefNames.AddRange(_miscReader.GetAllRefNames());
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
            var geneEnumerators = GetGeneEnumerators();

            var geneAnnotationDatabasePath = Path.Combine(_outputDirectory, SaDataBaseCommon.OmimDatabaseFileName);
            var geneAnnotationStream = FileUtilities.GetCreateStream(geneAnnotationDatabasePath);
            var databaseHeader = new SupplementaryAnnotationHeader("", DateTime.Now.Ticks, SaDataBaseCommon.DataVersion, _geneHeaders.Select(x => x.GetDataSourceVersion()), _genomeAssembly);

            List<IAnnotatedGene> geneAnnotations;
            using (var writer = new GeneDatabaseWriter(geneAnnotationStream, databaseHeader))
                while ((geneAnnotations = MergeUtilities.GetMinItems(geneEnumerators)) != null)
                {
                    var mergedGeneAnnotation = MergeGeneAnnotations(geneAnnotations);
                    writer.Write(mergedGeneAnnotation);
                }
        }

        private IAnnotatedGene MergeGeneAnnotations(List<IAnnotatedGene> geneAnnotations)
        {
            if (geneAnnotations == null || geneAnnotations.Count == 0) return null;

            var annotations = geneAnnotations.SelectMany(x => x.Annotations).ToArray();

            return new AnnotatedGene(geneAnnotations[0].GeneName, annotations);
        }

        private void MergeChrom(string refName)
        {
            var creationBench = new Benchmark();
            var currentChrAnnotationCount = 0;
            int refMinorCount;

            var iSaEnumerators = GetSaEnumerators(refName);

            var globalMajorAlleleInRefMinors = GetGlobalMajorAlleleForRefMinors(refName);

            var ucscRefName = _refChromDict[refName].UcscName;
            var dataSourceVersions = MergeUtilities.GetDataSourceVersions(_interimSaHeaders, _intervalHeaders);

            var header = new SupplementaryAnnotationHeader(ucscRefName, DateTime.Now.Ticks,
                SaDataBaseCommon.DataVersion, dataSourceVersions, _genomeAssembly);

            var intervalEnumerables = GetIntervalEnumerables(refName);
            var intervals = GetIntervals(intervalEnumerables).OrderBy(x => x.Start).ThenBy(x => x.End).ToList();

            var smallVariantIntervals = GetSpecificIntervals(ReportFor.SmallVariants, intervals);
            var svIntervals = GetSpecificIntervals(ReportFor.StructuralVariants, intervals);
            var allVariantsIntervals = GetSpecificIntervals(ReportFor.AllVariants, intervals);

            var saPath = Path.Combine(_outputDirectory, $"{ucscRefName}.nsa");

            using (var stream = FileUtilities.GetCreateStream(saPath))
            using (var idxStream = FileUtilities.GetCreateStream(saPath + ".idx"))
            using (var blockSaWriter = new SaWriter(stream, idxStream, header, smallVariantIntervals, svIntervals, allVariantsIntervals, globalMajorAlleleInRefMinors))
            {
                InterimSaPosition currPosition;
                while ((currPosition = GetNextInterimPosition(iSaEnumerators)) != null)
                {
                    var saPosition = currPosition.Convert();
                    blockSaWriter.Write(saPosition, currPosition.Position);
                    currentChrAnnotationCount++;
                }

                refMinorCount = blockSaWriter.RefMinorCount;
            }

            Console.WriteLine($"{ucscRefName,-23}  {currentChrAnnotationCount,10:n0}   {intervals.Count,6:n0}    {refMinorCount,6:n0}   {creationBench.GetElapsedIterationTime(currentChrAnnotationCount, "variants", out double _)}");
        }

        private static List<ISupplementaryInterval> GetSpecificIntervals(ReportFor reportFor, IEnumerable<ISupplementaryInterval> intervals)
        {
            return intervals.Where(interval => interval.ReportingFor == reportFor).ToList();
        }

        private List<ISupplementaryInterval> GetIntervals(IEnumerable<IEnumerable<ISupplementaryInterval>> interimIntervalEnumerators)
        {
            var intervals = new List<ISupplementaryInterval>();
            if (interimIntervalEnumerators == null ) return intervals;

            foreach (var intervalEnumerator in interimIntervalEnumerators)
            {
                intervals.AddRange(intervalEnumerator);
            }

            return intervals;
        }

        private InterimSaPosition GetNextInterimPosition(List<IEnumerator<IInterimSaItem>> iSaEnumerators)
        {
            var minItems = MergeUtilities.GetMinItems(iSaEnumerators);
            if (minItems == null) return null;

            var interimSaPosition = new InterimSaPosition();
            interimSaPosition.AddSaItems(minItems);

            return interimSaPosition;
        }

       
    }
}
