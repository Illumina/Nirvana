using System.Collections.Generic;
using System.IO;
using System.Linq;
using VariantAnnotation.DataStructures.ProteinFunction;
using VariantAnnotation.FileHandling;
using VariantAnnotation.FileHandling.PredictionCache;
using VariantAnnotation.Interface;

namespace VariantAnnotation.DataStructures
{
    public sealed class PredictionCache : IDataSource
    {
        public readonly IFileHeader Header;
        public readonly Prediction.Entry[] LookupTable;
        public readonly Prediction[] Predictions;
	    public int PredictionCount => Predictions.Length;

        public static PredictionCache Empty => new PredictionCache(null, null, null);
        public bool IsEmpty => Header == null && LookupTable == null && Predictions == null;
        public GenomeAssembly GenomeAssembly => Header.GenomeAssembly;
        public IEnumerable<IDataSourceVersion> DataSourceVersions { get; }

        /// <summary>
        /// constructor
        /// </summary>
        private PredictionCache(IFileHeader header, Prediction.Entry[] lookupTable, Prediction[] predictions)
        {
            Header       = header;
            LookupTable  = lookupTable;
            Predictions  = predictions;
        }

        /// <summary>
        /// deserializes the prediction cache associated with a particular index entry (reference seq)
        /// </summary>
        public static PredictionCache Read(BinaryReader reader, Prediction.Entry[] lookupTable, IndexEntry indexEntry,
            IFileHeader header)
        {
            var predictions = new Prediction[indexEntry.Count];
            for (int i = 0; i < indexEntry.Count; i++) predictions[i] = Prediction.Read(reader, lookupTable);

            return new PredictionCache(header, lookupTable, predictions);
        }

	    public PredictionCache GetMergedCache(PredictionCache otherCache)
	    {
		    var lut = LookupTable.ToList();
			lut.AddRange(otherCache.LookupTable);

		    var predictions = Predictions.ToList();
			predictions.AddRange(otherCache.Predictions);

			return new PredictionCache(Header, lut.ToArray(), predictions.ToArray());
	    }

        public void Clear() {}
    }
}
