using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Compression.Utilities;
using SAUtils.DataStructures;
using SAUtils.InputFileParsers.IntermediateAnnotation;
using SAUtils.Interface;
using VariantAnnotation.Utilities;

namespace SAUtils.MergeInterimTsvs
{
    public static class ReaderUtilities
    {
        public static List<SaHeader> GetTsvHeaders(IEnumerable<ITsvReader> tsvReaders)
        {
            if (tsvReaders == null) return null;
            var headers = new List<SaHeader>();
            foreach (var tsvReader in tsvReaders)
            {
                var header = tsvReader.SaHeader;
                if (header == null) throw new InvalidDataException("Data file lacks version information!!");
                headers.Add(header);
            }
            return headers;
        }

        public static IEnumerable<string> GetRefNames(IEnumerable<ITsvReader> tsvReaders)
        {
            return tsvReaders?.SelectMany(tsvReader => tsvReader.RefNames);
        }

        public static List<GeneTsvReader> GetGeneReaders(IEnumerable<string> geneFiles)
        {
            if (geneFiles == null) return null;

            var readers = new List<GeneTsvReader>();
            foreach (var fileName in geneFiles)
            {
                var streamReader = GZipUtilities.GetAppropriateStreamReader(fileName);
                var geneReader = new GeneTsvReader(streamReader);
                readers.Add(geneReader);
            }
            return readers;
        }

        public static List<IntervalTsvReader> GetIntervalReaders(IEnumerable<string> intervalFiles)
        {
            if (intervalFiles == null) return null;

            var readers = new List<IntervalTsvReader>();
            foreach (var fileName in intervalFiles)
            {
                var tsvStreamReader = GZipUtilities.GetAppropriateStreamReader(fileName);
                var indexFileStream = FileUtilities.GetReadStream(fileName + TsvIndex.FileExtension);
                readers.Add(new IntervalTsvReader(tsvStreamReader, indexFileStream));
            }

            return readers;
        }

        public static List<SaTsvReader> GetSaTsvReaders(IEnumerable<string> saTsvFiles)
        {
            if (saTsvFiles == null) return null;
            var readers = new List<SaTsvReader>();

            foreach (var fileName in saTsvFiles)
            {
                var tsvStreamReader = GZipUtilities.GetAppropriateStreamReader(fileName);
                var indexFileStream = FileUtilities.GetReadStream(fileName + TsvIndex.FileExtension);
                readers.Add(new SaTsvReader(tsvStreamReader, indexFileStream));
            }

            return readers;
        }

        public static SaMiscellaniesReader GetMiscTsvReader(string miscFile)
        {
            if (String.IsNullOrEmpty(miscFile)) return null;

            var streamReader = GZipUtilities.GetAppropriateStreamReader(miscFile);
            var indexStream = FileUtilities.GetReadStream(miscFile + TsvIndex.FileExtension);
            return new SaMiscellaniesReader(streamReader, indexStream);
        }
    }
}