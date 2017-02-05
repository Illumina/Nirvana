using VariantAnnotation.FileHandling;
using VariantAnnotation.Interface;

namespace VariantAnnotation.DataStructures.SupplementaryAnnotations
{
    sealed class EvsAnnotation:ISupplementaryAnnotation
	{
		#region member

		public string NumEvsSamples;
		public string EvsCoverage;
		public string EvsAfr;
		public string EvsAll;
		public string EvsEur;

		#endregion
		public bool HasConflicts { get; private set; }

		public void Read(ExtendedBinaryReader reader)
		{
			NumEvsSamples = reader.ReadAsciiString();
			EvsCoverage = reader.ReadAsciiString();
			EvsAfr = reader.ReadAsciiString();
			EvsAll = reader.ReadAsciiString();
			EvsEur = reader.ReadAsciiString();
		}

		public void AddAnnotationToVariant(IAnnotatedAlternateAllele jsonVariant)
		{
			jsonVariant.EvsCoverage = EvsCoverage;
			jsonVariant.EvsSamples = NumEvsSamples;

			jsonVariant.EvsAlleleFrequencyAfricanAmerican = EvsAfr;
			jsonVariant.EvsAlleleFrequencyEuropeanAmerican = EvsEur;
			jsonVariant.EvsAlleleFrequencyAll = EvsAll;
		}

		public void Write(ExtendedBinaryWriter writer)
		{
			writer.WriteOptAscii(NumEvsSamples);
			writer.WriteOptAscii(EvsCoverage);
			writer.WriteOptAscii(EvsAfr);
			writer.WriteOptAscii(EvsAll);
			writer.WriteOptAscii(EvsEur);
		}

		public void MergeAnnotations(ISupplementaryAnnotation other)
		{

			var otherAnnotation = other as EvsAnnotation;

			if (otherAnnotation?.NumEvsSamples == null) return;

			if (NumEvsSamples == null)
			{
				EvsAll = otherAnnotation.EvsAll;
				EvsAfr = otherAnnotation.EvsAfr;
				EvsEur = otherAnnotation.EvsEur;

				EvsCoverage = otherAnnotation.EvsCoverage;
				NumEvsSamples = otherAnnotation.NumEvsSamples;
			}
			else
			{
				HasConflicts = true;
				Clear();
			}
		}

		public void Clear()
		{
			EvsAll = null;
			EvsAfr = null;
			EvsEur = null;

			EvsCoverage = null;
			NumEvsSamples = null;
		}
	}
}
