using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommandLine.Utilities;
using SAUtils.DataStructures;
using SAUtils.InputFileParsers.IntermediateAnnotation;
using SAUtils.Interface;
using VariantAnnotation.GeneAnnotation;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.Providers;
using VariantAnnotation.SA;
using VariantAnnotation.Utilities;
using VariantAnnotation.Interface.GeneAnnotation;

namespace SAUtils.MergeInterimTsvs
{
    public sealed class InterimTsvsMerger
    {
        private readonly List<ParallelSaTsvReader> _tsvReaders;
        private readonly List<ParallelIntervalTsvReader> _intervalReaders;
        private readonly List<GeneTsvReader> _geneReaders;
        private readonly SaMiscellaniesReader _miscReader;
        private readonly List<SaHeader> _saHeaders;
        private readonly List<SaHeader> _geneHeaders;
        private readonly string _outputDirectory;
        private readonly GenomeAssembly _genomeAssembly;
        private readonly IDictionary<string, IChromosome> _refNameToChromosome;
        private readonly HashSet<string> _refNames;
        public static readonly HashSet<GenomeAssembly> AssembliesIgnoredInConsistancyCheck = new HashSet<GenomeAssembly> { GenomeAssembly.Unknown, GenomeAssembly.rCRS };

        public InterimTsvsMerger(IEnumerable<string> annotationFiles, IEnumerable<string> intervalFiles, string miscFile, IEnumerable<string> geneFiles, string compressedReference, string outputDirectory)
        {
            _outputDirectory = outputDirectory;

            var refSequenceProvider = new ReferenceSequenceProvider(FileUtilities.GetReadStream(compressedReference));
            _genomeAssembly         = refSequenceProvider.GenomeAssembly;
            _refNameToChromosome    = refSequenceProvider.RefNameToChromosome;
            
            _tsvReaders      = ReaderUtilities.GetSaTsvReaders(annotationFiles);
            _miscReader      = ReaderUtilities.GetMiscTsvReader(miscFile);
            _geneReaders     = ReaderUtilities.GetGeneReaders(geneFiles);
            _intervalReaders = ReaderUtilities.GetIntervalReaders(intervalFiles);

            _saHeaders = new List<SaHeader>();
            _saHeaders.AddRange(ReaderUtilities.GetTsvHeaders(_tsvReaders));
            _saHeaders.AddRange(ReaderUtilities.GetTsvHeaders(_intervalReaders));
            _geneHeaders = ReaderUtilities.GetTsvHeaders(_geneReaders)?.ToList();
            
            _refNames = new HashSet<string>();
            _refNames.UnionWith(ReaderUtilities.GetRefNames(_tsvReaders));
            _refNames.UnionWith(ReaderUtilities.GetRefNames(_intervalReaders));
            if (_miscReader != null) _refNames.UnionWith(_miscReader.RefNames);

            DisplayDataSources(_saHeaders, _geneHeaders);

            MergeUtilities.CheckAssemblyConsistancy(_saHeaders);
        }
        
        private static void DisplayDataSources(IEnumerable<SaHeader> saHeaders, IEnumerable<SaHeader> geneHeaders)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Data sources:\n");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("Name                     Version       Release Date          Misc");
            Console.WriteLine("=======================================================================");
            Console.ResetColor();

            foreach (var header in saHeaders.OrderBy(h => h.GetDataSourceVersion().Name))
            {
                Console.WriteLine(header);
            }

            foreach (var header in geneHeaders.OrderBy(h => h.GetDataSourceVersion().Name))
            {
                Console.WriteLine(header);
            }

            Console.WriteLine();
        }

        private List<(int, string)> GetGlobalMajorAlleleForRefMinors(string refName)
        {
            var globalAlleles = new List<(int, string)>();
            if (_miscReader == null) return globalAlleles;
            foreach (var saMiscellaniese in _miscReader.GetItems(refName))
            {
                globalAlleles.Add((saMiscellaniese.Position, saMiscellaniese.GlobalMajorAllele));
            }
            return globalAlleles;
        }


        private List<IEnumerator<IInterimSaItem>> GetSaEnumerators(string refName)
        {
            var saItemsList = new List<IEnumerator<IInterimSaItem>>();
            if (_tsvReaders == null) return saItemsList;
            foreach (var tsvReader in _tsvReaders)
            {
                var dataEnumerator = tsvReader.GetItems(refName).GetEnumerator();
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


            MergeGene(_geneReaders, _geneHeaders, _outputDirectory, _genomeAssembly);
            //dispose the gene annotators
            foreach (var geneReader in _geneReaders)
            {
                geneReader.Dispose();
            }
            
            Parallel.ForEach(_refNames, new ParallelOptions { MaxDegreeOfParallelism = 4 }, MergeChrom);
            
        }

        private static void MergeGene(IReadOnlyList<GeneTsvReader> geneReaders, IEnumerable<SaHeader> geneHeaders, string outputDirectory, GenomeAssembly assembly)
        {
            var geneAnnotationDatabasePath = Path.Combine(outputDirectory, SaDataBaseCommon.GeneLevelAnnotationFileName);
            var geneAnnotationStream       = FileUtilities.GetCreateStream(geneAnnotationDatabasePath);
            var databaseHeader             = new SupplementaryAnnotationHeader("", DateTime.Now.Ticks, SaDataBaseCommon.DataVersion, geneHeaders.Select(x => x.GetDataSourceVersion()), assembly);

            using (var writer = new GeneDatabaseWriter(geneAnnotationStream, databaseHeader))
            {
                foreach (var annotatedGene in GetAnnotatedGenes(geneReaders)?? Enumerable.Empty<IAnnotatedGene>())
                {
                    writer.Write(annotatedGene);
                }
            }
            
        }

        private static IEnumerable<IAnnotatedGene> GetAnnotatedGenes(IReadOnlyList<GeneTsvReader> geneReaders)
        {
            if (geneReaders == null) return null;

            var geneAnnotations = new Dictionary<string, IAnnotatedGene>();

            foreach (var reader in geneReaders)
            {
                foreach (var annotatedGene in reader.GetItems()?? Enumerable.Empty<IAnnotatedGene>())
                {
                    var geneName = annotatedGene.GeneName;
                    if (!geneAnnotations.TryAdd(geneName, annotatedGene))
                    {
                        geneAnnotations[geneName] = new AnnotatedGene(geneName, geneAnnotations[geneName].Annotations.Concat(annotatedGene.Annotations).ToArray());
                    }
                }
                
            }

            return geneAnnotations.Values;
        }


        private void MergeChrom(string refName)
        {
            var creationBench = new Benchmark();
            var currentChrAnnotationCount = 0;
            int refMinorCount;

            var saEnumerators = GetSaEnumerators(refName);

            //return;
            var globalMajorAlleleInRefMinors = GetGlobalMajorAlleleForRefMinors(refName);

            var dataSourceVersions = MergeUtilities.GetDataSourceVersions(_saHeaders);

            var ucscRefName = _refNameToChromosome[refName].UcscName;

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
                int position;
                ISaPosition saPosition;
                (position, saPosition) = GetNextInterimPosition(saEnumerators);

                while (saPosition!=null)
                {
                    blockSaWriter.Write(saPosition, position);
                    currentChrAnnotationCount++;
                    (position, saPosition) = GetNextInterimPosition(saEnumerators);
                }

                refMinorCount = blockSaWriter.RefMinorCount;
            }

            Console.WriteLine($"{ucscRefName,-23}  {currentChrAnnotationCount,10:n0}   {intervals.Count,6:n0}    {refMinorCount,6:n0}   {creationBench.GetElapsedIterationTime(currentChrAnnotationCount, "variants", out double _)}");
        }

        private static (int, ISaPosition) GetNextInterimPosition(List<IEnumerator<IInterimSaItem>> iSaEnumerators)
        {
            var minItems = MergeUtilities.GetMinItems(iSaEnumerators);
            return MergeUtilities.GetSaPosition(minItems);
        }
    }
}
