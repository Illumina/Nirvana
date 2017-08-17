using VariantAnnotation.Interface.IO;

namespace VariantAnnotation.Interface.GeneAnnotation
{
	public interface IAnnotatedGene:IJsonSerializer
	{
        string GeneName { get; }
        IGeneAnnotation[] Annotations { get; }
	}
}