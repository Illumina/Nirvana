using System;
using System.Collections.Generic;
using System.Linq;

namespace VariantAnnotation.DataStructures.SupplementaryAnnotations
{
	public static class DataSourceCommon
	{
		public const int MaxNumberOfDataSources = 8;

		public enum DataSource : byte
		{
			OneKg   = 1,
			Evs     = 2,
			Exac    = 4,			
			Cosmic  = 8,
			Clinvar = 16,
			DbSnp   = 32
		}

		public static int GetIndex(DataSource dataSource)
		{
			var index = -1;

			var dataSourceNumber = (int)dataSource;

			while (dataSourceNumber > 0)
			{
				dataSourceNumber = dataSourceNumber >> 1;
				index++;
			}

			return index;

		}

		public static void AddDataSource(DataSource dataSource, ref byte dataSourceFlag)
		{
			dataSourceFlag = (byte) (dataSourceFlag | (byte) dataSource);
		}

		public static void RemoveDataSource(DataSource dataSource, ref byte dataSourceFlag)
		{
			dataSourceFlag = (byte)(dataSourceFlag ^ (byte)dataSource);
		}

		public static IEnumerable<DataSource> GetAllDataSources()
		{
			return Enum.GetValues(typeof(DataSource)).Cast<DataSource>();
		}
	}
}