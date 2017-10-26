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

namespace SAUtils.MergeInterimTsvs
{
    public sealed class InterimTsvsMerger:IDisposable
    {
        private readonly List<SaTsvReader> _tsvReaders;
        private readonly List<IntervalTsvReader> _intervalReaders;
        private readonly List<GeneTsvReader> _geneReaders;
        private readonly SaMiscellaniesReader _miscReader;
        private readonly List<SaHeader> _saHeaders;
        //private readonly List<SaHeader> _smallAnnotationHeaders;
        //private readonly List<SaHeader> _intervalHeaders;
        private readonly List<SaHeader> _geneHeaders;
        private readonly string _outputDirectory;
        private readonly GenomeAssembly _genomeAssembly;
        private readonly IDictionary<string, IChromosome> _refChromDict;
        private readonly HashSet<string> _allRefNames;
        public static readonly HashSet<GenomeAssembly> AssembliesIgnoredInConsistancyCheck = new HashSet<GenomeAssembly>() { GenomeAssembly.Unknown, GenomeAssembly.rCRS };

        /// <summary>
        /// constructor
        /// </summary>
        public InterimTsvsMerger(IEnumerable<string> annotationFiles, IEnumerable<string> intervalFiles, string miscFile, IEnumerable<string> geneFiles, string compressedReference, string outputDirectory)
        {
            _outputDirectory = outputDirectory;

            var refSequenceProvider = new ReferenceSequenceProvider(FileUtilities.GetReadStream(compressedReference));
            _genomeAssembly = refSequenceProvider.GenomeAssembly;
            _refChromDict = refSequenceProvider.GetChromosomeDictionary();
            _allRefNames = new HashSet<string>();
            _saHeaders= new List<SaHeader>();

            _tsvReaders = ReaderUtilities.GetSaTsvReaders(annotationFiles);
            _saHeaders.AddRange(ReaderUtilities.GetTsvHeaders(_tsvReaders));
            _allRefNames.UnionWith(ReaderUtilities.GetRefNames(_tsvReaders));

            _intervalReaders = ReaderUtilities.GetIntervalReaders(intervalFiles);
            _saHeaders.AddRange(ReaderUtilities.GetTsvHeaders(_intervalReaders));
            _allRefNames.UnionWith(ReaderUtilities.GetRefNames(_intervalReaders));

            _geneReaders = ReaderUtilities.GetGeneReaders(geneFiles);
            _geneHeaders = ReaderUtilities.GetTsvHeaders(_geneReaders)?.ToList();
            _saHeaders.AddRange(_geneHeaders);

            _miscReader = ReaderUtilities.GetMiscTsvReader(miscFile);
            _allRefNames.UnionWith(_miscReader.RefNames);

            DisplayDataSources(_saHeaders);

            MergeUtilities.CheckAssemblyConsistancy(_saHeaders);
        }

        
        private static void DisplayDataSources(IEnumerable<SaHeader> headers)
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
                    var mergedGeneAnnotation = MergeUtilities.MergeGeneAnnotations(geneAnnotations);
                    writer.Write(mergedGeneAnnotation);
                }
        }

        private void MergeChrom(string refName)
        {
            var creationBench = new Benchmark();
            var currentChrAnnotationCount = 0;
            int refMinorCount;

            var saEnumerators = GetSaEnumerators(refName);

            var globalMajorAlleleInRefMinors = GetGlobalMajorAlleleForRefMinors(refName);

            var ucscRefName = _refChromDict[refName].UcscName;
            var dataSourceVersions = MergeUtilities.GetDataSourceVersions(_saHeaders);

            var header = new SupplementaryAnnotationHeader(ucscRefName, DateTime.Now.Ticks,
                SaDataBaseCommon.DataVersion, dataSourceVersions, _genomeAssembly);

            //we need a list because we will enumerate over it multiple times
            var intervals = MergeUtilities.GetIntervals(_intervalReaders,refName).OrderBy(x => x.Start).ThenBy(x => x.End).ToList();

            var svIntervals           = MergeUtilities.GetSpecificIntervals(ReportFor.StructuralVariants, intervals);
            var allVariantsIntervals  = MergeUtilities.GetSpecificIntervals(ReportFor.AllVariants, intervals);
            var smallVariantIntervals = MergeUtilities.GetSpecificIntervals(ReportFor.SmallVariants, intervals);

            var saPath = Path.Combine(_outputDirectory, $"{ucscRefName}.nsa");

            using (var stream = FileUtilities.GetCreateStream(saPath))
            using (var idxStream = FileUtilities.GetCreateStream(saPath + ".idx"))
            using (var blockSaWriter = new SaWriter(stream, idxStream, header, smallVariantIntervals, svIntervals, allVariantsIntervals, globalMajorAlleleInRefMinors))
            {
                InterimSaPosition currPosition;
                while ((currPosition = GetNextInterimPosition(saEnumerators)) != null)
                {
                    var saPosition = currPosition.Convert();
                    blockSaWriter.Write(saPosition, currPosition.Position);
                    currentChrAnnotationCount++;
                }

                refMinorCount = blockSaWriter.RefMinorCount;
            }

            Console.WriteLine($"{ucscRefName,-23}  {currentChrAnnotationCount,10:n0}   {intervals.Count,6:n0}    {refMinorCount,6:n0}   {creationBench.GetElapsedIterationTime(currentChrAnnotationCount, "variants", out double _)}");
        }

        private static InterimSaPosition GetNextInterimPosition(List<IEnumerator<IInterimSaItem>> iSaEnumerators)
        {
            var minItems = MergeUtilities.GetMinItems(iSaEnumerators);
            if (minItems == null) return null;

            var interimSaPosition = new InterimSaPosition();
            interimSaPosition.AddSaItems(minItems);

            return interimSaPosition;
        }

        public void Dispose()
        {
            _miscReader?.Dispose();

            if (_tsvReaders != null)
                foreach (var tsvReader in _tsvReaders)
                {
                    tsvReader.Dispose();
                }

            if (_intervalReaders != null)
                foreach (var intervalReader in _intervalReaders)
                {
                    intervalReader.Dispose();
                }

            if (_geneReaders != null)
                foreach (var geneReader in _geneReaders)
                {
                    geneReader.Dispose();
                }

        }
        
    }
}
