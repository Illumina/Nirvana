using System;

namespace VariantAnnotation.DataStructures.SupplementaryAnnotations
{
	public static class SupplementaryAnnotationFactory
	{
		internal static ISupplementaryAnnotation CreateSupplementaryAnnotation(DataSourceCommon.DataSource dataSource)
		{
			switch (dataSource)
			{
				case DataSourceCommon.DataSource.OneKg:
					return new OneKGenAnnotation();
				case DataSourceCommon.DataSource.DbSnp:
					return new DbSnpAnnotation();
				case DataSourceCommon.DataSource.Evs:
					return new EvsAnnotation();
				case DataSourceCommon.DataSource.Exac:
					return new ExacAnnotation();
				default:
					throw new Exception("invalid data source");
			}
		}
	}
}