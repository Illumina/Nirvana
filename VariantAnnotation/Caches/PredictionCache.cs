using System.Collections.Generic;
using System.Collections.Immutable;
using Genome;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Caches;
using VariantAnnotation.Interface.Providers;

namespace VariantAnnotation.Caches
{
    public sealed class PredictionCache : IPredictionCache
    {
        private readonly Prediction[] _predictions;
	    public string Name { get; } = string.Empty;
	    public GenomeAssembly Assembly { get; }
        public IEnumerable<IDataSourceVersion> DataSourceVersions { get; } = new List<IDataSourceVersion>();
        private readonly ImmutableArray<string> _descriptions;

        public PredictionCache(GenomeAssembly genomeAssembly, Prediction[] predictions, ImmutableArray<string> descriptions)
        {
            Assembly      = genomeAssembly;
            _predictions  = predictions;
            _descriptions = descriptions;
        }

        public PredictionScore GetProteinFunctionPrediction(int predictionIndex, char newAminoAcid,
            int aaPosition)
        {
            var entry = _predictions[predictionIndex].GetPrediction(newAminoAcid, aaPosition);

            return entry == null
                ? null
                : new PredictionScore(_descriptions[entry.EnumIndex], entry.Score);
        }
    }
}