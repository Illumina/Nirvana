using System;
using System.Collections.Generic;
using System.IO;
using IO;
using VariantAnnotation.Providers;

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

        public MiniSaExtractor(string compressedRefFile, string saPath, int begin, int end, string datasourceName = null,
            string outputDir = null)
        {
            _begin  = begin;
            _end    = end;
            _saPath = saPath;

            var refChromDict = new ReferenceSequenceProvider(FileUtilities.GetReadStream(compressedRefFile)).RefNameToChromosome;

            //string referenceName = GetReferenceName(saPath, refChromDict);
            //_miniSaPath = GetMiniSaPath(referenceName, begin, end, datasourceName, outputDir);

            Console.WriteLine($"MiniSA output to: {_miniSaPath}");
        }

        private static string GetMiniSaPath(string referenceName, int begin, int end, string dataSourceName, string outputDir)
        {
            string miniSaPath = dataSourceName == null
                ? $"{referenceName}_{begin}_{end}.nsa"
                : $"{referenceName}_{begin}_{end}_{dataSourceName}.nsa";

            if (outputDir != null) miniSaPath = Path.Combine(outputDir, miniSaPath);
            return miniSaPath;
        }

        //private static string GetReferenceName(string saPath, IDictionary<string, IChromosome> refChromDict)
        //{
        //    ISupplementaryAnnotationHeader header;

        //    using (var stream = FileUtilities.GetReadStream(saPath))
        //    using (var reader = new ExtendedBinaryReader(stream))
        //    {
        //        header = SaReader.GetHeader(reader);
        //    }

        //    return refChromDict[header.ReferenceSequenceName].UcscName;
        //}

        //private static SaWriter GetSaWriter(string saPath, ISupplementaryAnnotationHeader header,
        //    List<ISupplementaryInterval> smallVariantIntervals, List<ISupplementaryInterval> svIntervals,
        //    List<ISupplementaryInterval> allVariantIntervals,List<(int,string)> globalMajorAlleleInRefMinors)
        //{
        //    var stream    = FileUtilities.GetCreateStream(saPath);
        //    var idxStream = FileUtilities.GetCreateStream(saPath + ".idx");
        //    return new SaWriter(stream, idxStream, header, smallVariantIntervals, svIntervals, allVariantIntervals,globalMajorAlleleInRefMinors);
        //}

        //private static SaReader GetSaReader(string saPath)
        //{
        //    var stream    = FileUtilities.GetReadStream(saPath);
        //    var idxStream = FileUtilities.GetReadStream(saPath + ".idx");
        //    return new SaReader(stream, idxStream);
        //}

        public int Extract()
        {
            var count = 0;

            //using (var reader = GetSaReader(_saPath))
            //{
            //    var smallVariantIntervals = GetIntervals("small variants", reader.SmallVariantIntervals);
            //    var svIntervals           = GetIntervals("SVs",            reader.SvIntervals);
            //    var allVariantIntervals   = GetIntervals("all variants",   reader.AllVariantIntervals);
            //    var globalMajorAlleles = GetGlobaleMajorAlleleAndRefMinors(reader.GlobalMajorAlleleInRefMinors);

            //    using (var writer = GetSaWriter(_miniSaPath, reader.Header, smallVariantIntervals, svIntervals,
            //            allVariantIntervals,globalMajorAlleles))
            //    {
            //        for (int position = _begin; position <= _end; position++)
            //        {
            //            var saPosition = reader.GetAnnotation(position);
            //            if (saPosition == null) continue;

            //            writer.Write(saPosition, position);
            //            count++;
            //        }
            //    }
            //}

            return count;
        }

        private List<(int,string)> GetGlobaleMajorAlleleAndRefMinors(IEnumerable<(int Position, string)> readerGlobalMajorAlleleInRefMinors)
        {
            var overlappedRefMinors = new List<(int,string)>();
            foreach (var refMinor in readerGlobalMajorAlleleInRefMinors)
            {
                if(refMinor.Position>=_begin && refMinor.Position<=_end)
                    overlappedRefMinors.Add(refMinor);
            }
            return overlappedRefMinors;
        }

        //private List<ISupplementaryInterval> GetIntervals(string description,
        //    IEnumerable<Interval<ISupplementaryInterval>> intervals)
        //{
        //    var miniIntervals  = new List<ISupplementaryInterval>();
        //    var targetInterval = new Interval(_begin, _end);

        //    var allIntervals = intervals;

        //    if (allIntervals != null)
        //    {
        //        foreach (var interval in allIntervals)
        //        {
        //            if (targetInterval.Overlaps(interval.Begin, interval.End)) miniIntervals.Add(interval.Value);
        //        }
        //    }

        //    Console.WriteLine($"Found {miniIntervals.Count} supplementary intervals for {description}.");
        //    return miniIntervals;
        //}
    }
}
