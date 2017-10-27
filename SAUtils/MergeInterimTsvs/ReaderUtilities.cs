using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SAUtils.DataStructures;
using SAUtils.InputFileParsers.IntermediateAnnotation;
using SAUtils.Interface;

namespace SAUtils.MergeInterimTsvs
{
    public static class ReaderUtilities
    {
        public static List<SaHeader> GetTsvHeaders(IEnumerable<IParallelTsvReader> tsvReaders)
        {
            return tsvReaders?.Select(tsvReader => tsvReader.SaHeader).ToList();
        }

        public static IEnumerable<string> GetRefNames(IEnumerable<IParallelTsvReader> tsvReaders)
        {
            return tsvReaders?.SelectMany(tsvReader => tsvReader.RefNames);
        }

        public static List<ParallelGeneTsvReader> GetGeneReaders(IEnumerable<string> geneFiles)
        {
            return geneFiles?.Select(fileName => new ParallelGeneTsvReader(fileName)).ToList();
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