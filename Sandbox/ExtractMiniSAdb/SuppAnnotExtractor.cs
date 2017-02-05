using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using VariantAnnotation.DataStructures;
using VariantAnnotation.DataStructures.SupplementaryAnnotations;
using VariantAnnotation.FileHandling.SupplementaryAnnotations;
using VariantAnnotation.Utilities;

namespace ExtractMiniSAdB
{
    public sealed class SuppAnnotExtractor
    {
        #region members

        private readonly SupplementaryAnnotationReader _reader;
        private readonly SupplementaryAnnotationWriter _writer;
        private readonly int _begin;
        private readonly int _end;
        private readonly ChromosomeRenamer _renamer;

        #endregion

        public SuppAnnotExtractor(string compressedRefFile, string inputSuppAnnotFile, int begin, int end,
            string datasourceName = null, string outDirectory = null)
        {
            _renamer = ChromosomeRenamer.GetChromosomeRenamer(FileUtilities.GetReadStream(compressedRefFile));

            long intervalsPosition;
            var saHeader = SupplementaryAnnotationReader.GetHeader(inputSuppAnnotFile, out intervalsPosition);

            _begin = begin;
            _end = end;
            string miniSuppAnnotFile;

            if (datasourceName == null)
            {
                miniSuppAnnotFile = _renamer.GetUcscReferenceName(saHeader.ReferenceSequenceName)
                                      + '_' + begin.ToString(CultureInfo.InvariantCulture) + '_' +
                                      end.ToString(CultureInfo.InvariantCulture) + ".nsa";

                if (outDirectory != null) miniSuppAnnotFile = Path.Combine(outDirectory, miniSuppAnnotFile);
            }
            else
            {
                miniSuppAnnotFile = _renamer.GetUcscReferenceName(saHeader.ReferenceSequenceName)
                                      + '_' + begin.ToString(CultureInfo.InvariantCulture) + '_' +
                                      end.ToString(CultureInfo.InvariantCulture) + '_' + datasourceName + ".nsa";
                if (outDirectory != null) miniSuppAnnotFile = Path.Combine(outDirectory, miniSuppAnnotFile);
            }


            _writer = new SupplementaryAnnotationWriter(miniSuppAnnotFile, saHeader.ReferenceSequenceName, saHeader.DataSourceVersions);

            Console.WriteLine("MiniSA output to: " + miniSuppAnnotFile);
        }

        public int Extract()
        {
            var count = 0;
            using (_reader)
            using (_writer)
            {
                for (var i = _begin; i <= _end; i++)
                {
                    var sa = _reader.GetAnnotation(i) as SupplementaryAnnotationPosition;

                    if (sa != null)
                    {
                        count++;
                        _writer.Write(new SupplementaryPositionCreator(sa), i);
                    }
                }
                var miniSaInterval = new AnnotationInterval(_begin, _end);

                // get the supplementary intervals overlapping the mini SA interval

                var suppIntervals = new List<SupplementaryInterval>();
                var readerSuppInterval = _reader.GetSupplementaryIntervals(_renamer);
                if (readerSuppInterval != null)
                    foreach (var interval in readerSuppInterval)
                    {
                        if (miniSaInterval.Overlaps(interval.Start, interval.End)) suppIntervals.Add(interval as SupplementaryInterval);
                    }
                Console.WriteLine("Found {0} supplementary intervals.", suppIntervals.Count);
                _writer.SetIntervalList(suppIntervals);
            }
            return count;
        }

    }
}
