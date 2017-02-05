using VariantAnnotation.FileHandling;

namespace VariantAnnotation.DataStructures.SupplementaryAnnotations
{
	public sealed class AlleleSpecificAnnotation
	{
		public byte SaDataSourceFlag;
		public ISupplementaryAnnotation[] Annotations { get; }

		public double AltAlleleFreq = double.MinValue;

		public AlleleSpecificAnnotation()
		{
			SaDataSourceFlag = new byte();
			Annotations = new ISupplementaryAnnotation[DataSourceCommon.MaxNumberOfDataSources];
		}

		public bool HasDataSource(DataSourceCommon.DataSource dataSource)
		{
			return (SaDataSourceFlag & (byte) dataSource) != 0;
		}



		public static AlleleSpecificAnnotation Read(ExtendedBinaryReader reader)
		{
			var asa = new AlleleSpecificAnnotation {SaDataSourceFlag = reader.ReadByte()};

			foreach (var dataSource in DataSourceCommon.GetAllDataSources())
			{
				if(!asa.HasDataSource(dataSource)) continue;
				var supplementaryAnnotation = SupplementaryAnnotationFactory.CreateSupplementaryAnnotation(dataSource);
				supplementaryAnnotation.Read(reader);
				asa.Annotations[DataSourceCommon.GetIndex(dataSource)] = supplementaryAnnotation;
			}

			return asa;
		}


		public void Write(ExtendedBinaryWriter extendedWriter)
		{
			//adjust the data source flag
			foreach (var dataSource in DataSourceCommon.GetAllDataSources())
			{
				var dataSourceIndex = DataSourceCommon.GetIndex(dataSource);
				if (HasDataSource(dataSource) && Annotations[dataSourceIndex].HasConflicts)
					DataSourceCommon.RemoveDataSource(dataSource,ref SaDataSourceFlag);
			}

			//write the flag
			extendedWriter.Write(SaDataSourceFlag);

			foreach (var dataSource in DataSourceCommon.GetAllDataSources())
			{
				if (!HasDataSource(dataSource)) continue;
				var dataSourceIndex = DataSourceCommon.GetIndex(dataSource);
				Annotations[dataSourceIndex].Write(extendedWriter);
			}
		}



		public void MergeAlleleSpecificAnnotation(AlleleSpecificAnnotation otherAsa)
		{
			foreach (var dataSource in DataSourceCommon.GetAllDataSources())
			{
				var index = DataSourceCommon.GetIndex(dataSource);
				if (HasDataSource(dataSource) && otherAsa.HasDataSource(dataSource))
				{		
					Annotations[index].MergeAnnotations(otherAsa.Annotations[index]);
					break;
				}
				if (!HasDataSource(dataSource) && otherAsa.HasDataSource(dataSource))
				{
					DataSourceCommon.AddDataSource(dataSource,ref SaDataSourceFlag);
					Annotations[index] = otherAsa.Annotations[index];
				}
			}
		}
	}
}