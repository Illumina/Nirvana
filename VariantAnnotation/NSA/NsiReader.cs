using System.Collections.Generic;
using System.IO;
using Compression.Algorithms;
using ErrorHandling.Exceptions;
using Genome;
using Intervals;
using IO;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.IO;
using VariantAnnotation.Providers;
using VariantAnnotation.SA;
using Variants;

namespace VariantAnnotation.NSA
{
    public sealed class NsiReader : INsiReader
    {
        private readonly Stream _stream;
        public GenomeAssembly Assembly { get; }
        public IDataSourceVersion Version { get; }
        public string JsonKey { get; }
        public ReportFor ReportFor { get; }
        private readonly Dictionary<ushort, IntervalArray<string>> _intervalArrays;

        private const int MaxStreamLength = 10 * 1048576;
        public NsiReader(Stream stream)
        {
            _stream = stream;
            var compressData = new byte[MaxStreamLength];
            int length = stream.Read(compressData, 0, MaxStreamLength);
            //uncompress
            var zstd = new Zstandard();
            var decompressedLength = zstd.GetDecompressedLength(compressData, length);
            var decompressedData = new byte[decompressedLength];
            zstd.Decompress(compressData, length, decompressedData, decompressedLength);

            using (var memStream = new MemoryStream(decompressedData, 0, decompressedLength))
            using (var memReader = new ExtendedBinaryReader(memStream))
            {
                Version   = DataSourceVersion.Read(memReader);
                Assembly  = (GenomeAssembly)memReader.ReadByte();
                JsonKey   = memReader.ReadAsciiString();
                ReportFor = (ReportFor)memReader.ReadByte();
                int schemaVersion = memReader.ReadOptInt32();

                if (schemaVersion != SaCommon.SchemaVersion)
                    throw new UserErrorException($"Schema version mismatch!! Expected {SaCommon.SchemaVersion}, observed {schemaVersion} for {JsonKey}");

                
                int count = memReader.ReadOptInt32();
                var suppIntervals = new Dictionary<ushort, List<Interval<string>>>();
                for (var i = 0; i < count; i++)
                {
                    var saInterval = new SuppInterval(memReader);
                    if (suppIntervals.TryGetValue(saInterval.Chromosome.Index, out var intervals)) intervals.Add(new Interval<string>(saInterval.Start, saInterval.End, saInterval.GetJsonString()));
                    else suppIntervals[saInterval.Chromosome.Index] = new List<Interval<string>> { new Interval<string>(saInterval.Start, saInterval.End, saInterval.GetJsonString()) };
                }

                _intervalArrays = new Dictionary<ushort, IntervalArray<string>>(suppIntervals.Count);
                foreach ((ushort chromIndex, List<Interval<string>> intervals) in suppIntervals)
                {
                    _intervalArrays[chromIndex] = new IntervalArray<string>(intervals.ToArray());
                }
            }
            
        }

        public IEnumerable<string> GetAnnotation(IVariant variant)
        {
            if (!_intervalArrays.ContainsKey(variant.Chromosome.Index)) return null;

            var overlappingSvs = _intervalArrays[variant.Chromosome.Index]
                .GetAllOverlappingIntervals(variant.Start, variant.End);

            if (overlappingSvs == null) return null;
            
            var jsonStrings = new List<string>();
            foreach (var interval in overlappingSvs)
            {
                var (reciprocalOverlap, annotationOverlap) = SuppIntervalUtilities.GetOverlapFractions(
                    new ChromosomeInterval(variant.Chromosome, interval.Begin, interval.End), variant);
                jsonStrings.Add(AddOverlapToAnnotation(interval.Value, reciprocalOverlap, annotationOverlap));
            }

            return jsonStrings;
        }

        private static string AddOverlapToAnnotation(string jsonString, double? reciprocalOverlap, double? annotationOverlap)
        {
            if (reciprocalOverlap != null)
                jsonString+=JsonObject.Comma + "\"reciprocalOverlap\":" + reciprocalOverlap.Value.ToString("0.#####");
            if (annotationOverlap != null)
                jsonString += JsonObject.Comma + "\"annotationOverlap\":" + annotationOverlap.Value.ToString("0.#####");
            return jsonString;
        }

        public void Dispose()
        {
            _stream?.Dispose();
        }
    }
}