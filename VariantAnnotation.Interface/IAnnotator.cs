using System.Collections.Generic;
using Genome;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Positions;

namespace VariantAnnotation.Interface
{
	public interface IAnnotator
	{
		GenomeAssembly Assembly { get; }
		IAnnotatedPosition Annotate(IPosition position);
		IEnumerable<string> GetGeneAnnotations();
		void EnableMitochondrialAnnotation();
	}
}