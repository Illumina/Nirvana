using System;
using VariantAnnotation.Interface.IO;

namespace VariantAnnotation.Interface.GeneAnnotation
{
	public interface IAnnotatedGene:IJsonSerializer,IComparable<IAnnotatedGene>
	{
        string GeneName { get; }
        IGeneAnnotationSource[] Annotations { get; }
        void Write(IExtendedBinaryWriter writer);
	}


}