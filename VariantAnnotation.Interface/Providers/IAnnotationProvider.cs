using System;
using System.Collections.Generic;
using Genome;
using VariantAnnotation.Interface.AnnotatedPositions;

namespace VariantAnnotation.Interface.Providers
{
	public interface IAnnotationProvider : IProvider, IDisposable
    {
		void Annotate(IAnnotatedPosition annotatedPosition);
        void PreLoad(Chromosome chromosome, List<int> positions);
    }
}