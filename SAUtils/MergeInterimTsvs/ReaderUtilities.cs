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

        public static List<IntervalTsvReader> GetIntervalReaders(IEnumerable<string> intervalFiles)
        {
            return intervalFiles?.Select(fileName => new IntervalTsvReader(fileName)).ToList();
        }

        public static List<SaTsvReader> GetSaTsvReaders(IEnumerable<string> saTsvFiles)
        {
            return saTsvFiles?.Select(fileName => new SaTsvReader(fileName)).ToList();
        }

        public static SaMiscellaniesReader GetMiscTsvReader(string miscFile)
        {
            return string.IsNullOrEmpty(miscFile) ? null : new SaMiscellaniesReader(miscFile);
        }
    }
}