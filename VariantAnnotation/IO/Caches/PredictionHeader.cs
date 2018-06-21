using System.IO;
using System.Text;
using Compression.FileHandling;
using IO;
using VariantAnnotation.Caches.DataStructures;

namespace VariantAnnotation.IO.Caches
{
    public sealed class PredictionHeader : Header
    {
        public readonly PredictionCacheCustomHeader Custom;
        public readonly Prediction.Entry[] LookupTable;

        public PredictionHeader(Header header, PredictionCacheCustomHeader customHeader, Prediction.Entry[] lookupTable)
            : base(header.Identifier, header.SchemaVersion, header.DataVersion, header.Source,
                header.CreationTimeTicks, header.Assembly)
        {
            Custom      = customHeader;
            LookupTable = lookupTable;
        }

        public new void Write(BinaryWriter writer)
        {
            base.Write(writer);
            Custom.Write(writer);
        }

        public static PredictionHeader Read(Stream stream, BlockStream blockStream)
        {
            Header baseHeader;
            PredictionCacheCustomHeader customHeader;
            Prediction.Entry[] lookupTable;

            using (var reader = new BinaryReader(stream, Encoding.Default, true))
            {
                baseHeader = Read(reader);
                customHeader = PredictionCacheCustomHeader.Read(reader);
            }

            using (var reader = new ExtendedBinaryReader(blockStream, Encoding.Default, true))
            {
                lookupTable = ReadLookupTable(reader);
            }

            return new PredictionHeader(baseHeader, customHeader, lookupTable);
        }

        private static Prediction.Entry[] ReadLookupTable(ExtendedBinaryReader reader)
        {
            int numEntries = reader.ReadInt32();
            var lut = new Prediction.Entry[numEntries];
            for (var i = 0; i < numEntries; i++) lut[i] = Prediction.Entry.ReadEntry(reader);
            return lut;
        }
    }
}
