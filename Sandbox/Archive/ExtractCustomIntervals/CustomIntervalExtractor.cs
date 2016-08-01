
using System;
using System.Globalization;
using System.IO;
using Illumina.VariantAnnotation.FileHandling.CustomInterval;

namespace ExtractCustomIntervals
{
	public class CustomIntervalExtractor
	{
		private readonly int _begin;
		private readonly int _end;

		private readonly CustomIntervalReader _reader;
		private readonly CustomIntervalWriter _writer;

		public CustomIntervalExtractor(string inputCustIntervalPath, string outputDir, int begin, int end)
		{
			_begin = begin;
			_end = end;

			_reader = new CustomIntervalReader(inputCustIntervalPath);

			
			string outputFileName = outputDir +Path.DirectorySeparatorChar + _reader.GetReferenceName() +'_'+ _reader.GetIntervalType()
				+ '_' + begin.ToString(CultureInfo.InvariantCulture) + '_' + end.ToString(CultureInfo.InvariantCulture) + ".nci";

			Console.WriteLine("Creating custom interval db: "+ outputFileName);
			_writer = new CustomIntervalWriter(outputFileName, _reader.GetReferenceName(), _reader.GetIntervalType(), _reader.DataVersion);
		}

		public int Extract()
		{
			var count = 0;
			var interval = _reader.GetNextCustomInterval();
			while (interval!=null)
			{
				if (interval.Overlaps(_begin, _end))
				{
					_writer.WriteInterval(interval);
					count++;
				}

				interval = _reader.GetNextCustomInterval();
			}
			_writer.Dispose();
			return count;
		}
	}
}
