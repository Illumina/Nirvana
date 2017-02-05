using VariantAnnotation.FileHandling;
using VariantAnnotation.Interface;

namespace VariantAnnotation.DataStructures.SupplementaryAnnotations
{
	public interface ISupplementaryAnnotation
	{
		bool HasConflicts { get; }
		void Read(ExtendedBinaryReader reader);
		void AddAnnotationToVariant(IAnnotatedAlternateAllele jsonVariant);	
		void MergeAnnotations(ISupplementaryAnnotation other);
		void Clear();
		void Write(ExtendedBinaryWriter writer);
	}

}