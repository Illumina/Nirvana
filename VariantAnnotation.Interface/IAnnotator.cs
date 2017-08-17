using System.Collections.Generic;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.GeneAnnotation;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Sequence;

namespace VariantAnnotation.Interface
{
	public interface IAnnotator
	{
		GenomeAssembly GenomeAssembly { get; }
		IAnnotatedPosition Annotate(IPosition position);
		IList<IAnnotatedGene> GetAnnotatedGenes();
		void EnableMitochondrialAnnotation();
	}
}