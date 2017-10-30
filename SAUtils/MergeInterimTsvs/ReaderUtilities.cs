using System.Collections.Generic;
using System.Linq;
using SAUtils.DataStructures;
using SAUtils.InputFileParsers.IntermediateAnnotation;
using SAUtils.Interface;

namespace SAUtils.MergeInterimTsvs
{
    public static class ReaderUtilities
    {
        public static List<SaHeader> GetTsvHeaders(IEnumerable<ITsvReader> tsvReaders)
        {
            return tsvReaders?.Select(tsvReader => tsvReader.SaHeader).ToList();
        }

        public static IEnumerable<string> GetRefNames(IEnumerable<ITsvReader> tsvReaders)
        {
            return tsvReaders?.SelectMany(tsvReader => tsvReader.RefNames);
        }

        public static List<GeneTsvReader> GetGeneReaders(IEnumerable<string> geneFiles)
        {
            return geneFiles?.Select(fileName => new GeneTsvReader(fileName)).ToList();
        }

        public static List<ParallelIntervalTsvReader> GetIntervalReaders(IEnumerable<string> intervalFiles)
        {
            return intervalFiles?.Select(fileName => new ParallelIntervalTsvReader(fileName)).ToList();
        }

        public static List<ParallelSaTsvReader> GetSaTsvReaders(IEnumerable<string> saTsvFiles)
        {
            return saTsvFiles?.Select(fileName => new ParallelSaTsvReader(fileName)).ToList();
        }

        public static SaMiscellaniesReader GetMiscTsvReader(string miscFile)
        {
            return string.IsNullOrEmpty(miscFile) ? null : new SaMiscellaniesReader(miscFile);
        }
    }
}