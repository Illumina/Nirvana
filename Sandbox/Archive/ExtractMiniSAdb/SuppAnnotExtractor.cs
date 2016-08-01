using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Illumina.VariantAnnotation.DataStructures;
using Illumina.VariantAnnotation.DataStructures.SupplementaryAnnotations;
using Illumina.VariantAnnotation.FileHandling;
using Illumina.VariantAnnotation.FileHandling.SupplementaryAnnotations;
using Illumina.VariantAnnotation.Utilities;

namespace ExtractMiniSAdb
{
    public class SuppAnnotExtractor
    {
        #region members

	    private readonly SupplementaryAnnotationReader _reader;
        private readonly SupplementaryAnnotationWriter _writer;
        private readonly int _begin;
        private readonly int _end;

        #endregion

        public SuppAnnotExtractor(string compressedRefFile, string inputSuppAnnotFile, int begin, int end, string datasourceName=null, string outDirectory = null)
        {
            AnnotationLoader.Instance.LoadCompressedSequence(compressedRefFile);
            AnnotationLoader.Instance.Load("chr1");

            var chromosomeRenamer = AnnotationLoader.Instance.ChromosomeRenamer;

            _reader = new SupplementaryAnnotationReader(inputSuppAnnotFile);
            _begin  = begin;
            _end    = end;
	        string miniSuppAnnotFile;

			if (datasourceName == null)
			{
				miniSuppAnnotFile = chromosomeRenamer.GetUcscReferenceName(_reader.CacheHeader.ReferenceSequenceName)
									  + '_' + begin.ToString(CultureInfo.InvariantCulture) + '_' +
									  end.ToString(CultureInfo.InvariantCulture) + ".nsa";

				if (outDirectory != null) miniSuppAnnotFile = Path.Combine(outDirectory, miniSuppAnnotFile);
			}
			else
			{
				miniSuppAnnotFile = chromosomeRenamer.GetUcscReferenceName(_reader.CacheHeader.ReferenceSequenceName)
									  + '_' + begin.ToString(CultureInfo.InvariantCulture) + '_' +
									  end.ToString(CultureInfo.InvariantCulture) + '_' + datasourceName + ".nsa";
				if (outDirectory != null) miniSuppAnnotFile = Path.Combine(outDirectory, miniSuppAnnotFile);
			}


			_writer = new SupplementaryAnnotationWriter(miniSuppAnnotFile, _reader.CacheHeader.ReferenceSequenceName, _reader.CacheHeader.DataSourceVersions);

            Console.WriteLine("MiniSA output to: "+miniSuppAnnotFile);
        }

        public int Extract()
        {
            
            var count = 0;
            using (_reader)
            using (_writer)
            {
                for (var i = _begin; i <= _end; i++)
                {
					var sa = _reader.GetAnnotation(i);
					if (sa !=null)
                    {
                        count++;
                        _writer.Write(sa, i);
                    }
                }    
				var miniSaInterval= new AnnotationInterval(_begin, _end);

				// get the supplementary intervals overlapping the mini SA interval

				var suppIntervals = new List<SupplementaryInterval>();
	            var readerSuppInterval = _reader.GetSupplementaryIntervals();
				if (readerSuppInterval!=null)
					foreach (var interval in readerSuppInterval)
					{
						if (miniSaInterval.Overlaps(interval.Start, interval.End))
							suppIntervals.Add(interval);

					}
	            Console.WriteLine("Found {0} supplementary intervals.", suppIntervals.Count);
	            _writer.SetIntervalList(suppIntervals);
            }
            return count;
        }

    }
}
