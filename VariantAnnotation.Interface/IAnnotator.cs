using System.Collections.Generic;
using Genome;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.GeneAnnotation;
using VariantAnnotation.Interface.Positions;

namespace VariantAnnotation.Interface
{
	public interface IAnnotator
	{
		GenomeAssembly Assembly { get; }
		IAnnotatedPosition Annotate(IPosition position);
		IList<IAnnotatedGene> GetAnnotatedGenes();
		void EnableMitochondrialAnnotation();
	}
}