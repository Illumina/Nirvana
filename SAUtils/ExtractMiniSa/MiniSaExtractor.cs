using System;
using System.Collections.Generic;
using System.IO;
using VariantAnnotation.DataStructures.Intervals;
using VariantAnnotation.FileHandling.Binary;
using VariantAnnotation.FileHandling.SA;
using VariantAnnotation.Interface;
using VariantAnnotation.Utilities;

namespace SAUtils.ExtractMiniSa
{
    public sealed class MiniSaExtractor
    {
        #region members

        private readonly int _begin;
        private readonly int _end;
        private readonly string _saPath;
        private readonly string _miniSaPath;

        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        public MiniSaExtractor(string compressedRefFile, string saPath, int begin, int end, string datasourceName = null,
            string outputDir = null)
        {
            _begin  = begin;
            _end    = end;
            _saPath = saPath;

            var renamer       = ChromosomeRenamer.GetChromosomeRenamer(FileUtilities.GetReadStream(compressedRefFile));
            var referenceName = GetReferenceName(saPath, renamer);
            _miniSaPath       = GetMiniSaPath(referenceName, begin, end, datasourceName, outputDir);

            Console.WriteLine($"MiniSA output to: {_miniSaPath}");
        }

        private string GetMiniSaPath(string referenceName, int begin, int end, string dataSourceName, string outputDir)
        {
            string miniSaPath = dataSourceName == null
                ? $"{referenceName}_{begin}_{end}.nsa"
                : $"{referenceName}_{begin}_{end}_{dataSourceName}.nsa";

            if (outputDir != null) miniSaPath = Path.Combine(outputDir, miniSaPath);
            return miniSaPath;
        }

        private string GetReferenceName(string saPath, ChromosomeRenamer renamer)
        {
            ISupplementaryAnnotationHeader header;

            using (var stream = FileUtilities.GetReadStream(saPath))
            using (var reader = new ExtendedBinaryReader(stream))
            {
                header = SaReader.GetHeader(reader);
            }

            return renamer.GetUcscReferenceName(header.ReferenceSequenceName);
        }

        private SaWriter GetSaWriter(string saPath, ISupplementaryAnnotationHeader header,
            List<IInterimInterval> smallVariantIntervals, List<IInterimInterval> svIntervals,
            List<IInterimInterval> allVariantIntervals)
        {
            var stream    = FileUtilities.GetCreateStream(saPath);
            var idxStream = FileUtilities.GetCreateStream(saPath + ".idx");
            return new SaWriter(stream, idxStream, header, smallVariantIntervals, svIntervals, allVariantIntervals);
        }

        private SaReader GetSaReader(string saPath)
        {
            var stream    = FileUtilities.GetReadStream(saPath);
            var idxStream = FileUtilities.GetReadStream(saPath + ".idx");
            return new SaReader(stream, idxStream);
        }

        public int Extract()
        {
            var count = 0;

            using (var reader = GetSaReader(_saPath))
            {
                var smallVariantIntervals = GetIntervals("small variants", reader.SmallVariantIntervals);
                var svIntervals           = GetIntervals("SVs",            reader.SvIntervals);
                var allVariantIntervals   = GetIntervals("all variants",   reader.AllVariantIntervals);

                using (var writer = GetSaWriter(_miniSaPath, reader.Header, smallVariantIntervals, svIntervals,
                        allVariantIntervals))
                {
                    for (var position = _begin; position <= _end; position++)
                    {
                        var saPosition = reader.GetAnnotation(position);
                        if (saPosition == null) continue;

                        var isRefMinor = reader.IsRefMinor(position);
                        writer.Write(saPosition, position, isRefMinor);
                        count++;
                    }
                }
            }

            return count;
        }

        private List<IInterimInterval> GetIntervals(string description,
            IEnumerable<Interval<IInterimInterval>> intervals)
        {
            var miniIntervals  = new List<IInterimInterval>();
            var targetInterval = new AnnotationInterval(_begin, _end);

            var allIntervals = intervals;

            if (allIntervals != null)
            {
                foreach (var interval in allIntervals)
                {
                    if (targetInterval.Overlaps(interval.Begin, interval.End)) miniIntervals.Add(interval.Value);
                }
            }

            Console.WriteLine($"Found {miniIntervals.Count} supplementary intervals for {description}.");
            return miniIntervals;
        }
    }
}
