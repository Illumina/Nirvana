using System;
using System.Collections.Generic;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.SA;

namespace VariantAnnotation.GeneFusions.IO
{
    public interface IGeneFusionSaReader : ISaMetadata, IDisposable
    {
        void LoadAnnotations();
        // ReSharper disable once ParameterTypeCanBeEnumerable.Global
        void AddAnnotations(IGeneFusionPair[] fusionPairs, IList<ISupplementaryAnnotation> supplementaryAnnotations);
    }
}